using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using VHS.Data.Audit;
using VHS.Data.Auth;
using VHS.Data.Auth.Models.Auth;
using VHS.Data.Auth.Seeders;
using VHS.Data.Core;
using VHS.WebAPI.Hubs;
using VHS.WebAPI.Infra.Exceptions;
using VHS.WebAPI.Middlewares;
using VHS.WebAPI.Authorization;


var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var environment = builder.Environment;

builder.Configuration.AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true);

builder.Services.AddMemoryCache();

var loggingDirectory = builder.Configuration.GetValue<string>("LoggingDirectory");
Environment.SetEnvironmentVariable("LoggingDirectory", loggingDirectory);


builder.Host.UseSerilog((context, services, serilogConfiguration) =>
{
	serilogConfiguration.ReadFrom.Configuration(configuration);
});

// Configure Sentry
builder.WebHost.UseSentry(o =>
{
	o.Dsn = builder.Configuration["Sentry:Dsn"];
	o.Debug = builder.Configuration.GetValue<bool>("Sentry:Debug");
	o.TracesSampleRate = builder.Configuration.GetValue<double>("Sentry:TracesSampleRate");
	o.SendDefaultPii = true;
	o.AttachStacktrace = true;
});
services.AddExceptionHandler<GlobalExceptionHandler>();

// Read CORS allowed origins
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

services.AddCors(options =>
{
	options.AddPolicy("AllowBlazorClient", policy =>
	{
		policy.SetIsOriginAllowed(origin =>
		{
			// Allow all localhost origins (including different ports)
			if (origin.Contains("localhost", StringComparison.OrdinalIgnoreCase))
				return true;

			// Allow explicitly defined origins from configuration
			return allowedOrigins?.Contains(origin) ?? false;
		})
		.AllowAnyHeader()
		.AllowAnyMethod()
		.AllowCredentials();
	});
});

// Add Controllers & API Endpoints
services.AddControllers();
services.AddEndpointsApiExplorer();

// Enable Swagger
services.AddSwaggerGen();

var jwtSettings = configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
	throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.");
}

// Add Identity Services BEFORE JWT to avoid conflicts
services.AddIdentity<User, IdentityRole<Guid>>(opts =>
{
	opts.User.RequireUniqueEmail = true;
	opts.Password.RequireDigit = true;
	opts.Password.RequiredLength = 8;
	opts.Password.RequireNonAlphanumeric = false;
	opts.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<VHSAuthDBContext>()
.AddDefaultTokenProviders();

// Configure Authentication with JWT Bearer (override Identity defaults)
services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidIssuer = jwtSettings["Issuer"],
		ValidateAudience = true,
		ValidAudience = jwtSettings["Audience"],
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
	
	// Add event handlers for debugging
	options.Events = new JwtBearerEvents
	{
		OnAuthenticationFailed = context =>
		{
			// Console.WriteLine($"[DEBUG] JWT Authentication failed: {context.Exception.Message}");
			return Task.CompletedTask;
		},
		OnTokenValidated = context =>
		{
			// Console.WriteLine($"[DEBUG] JWT Token validated for user: {context.Principal?.Identity?.Name}");
			return Task.CompletedTask;
		},
		OnChallenge = context =>
		{
			// Console.WriteLine($"[DEBUG] JWT Challenge triggered: {context.Error} - {context.ErrorDescription}");
			// Check if this is an API request (JSON Accept header) vs browser request
			var acceptHeader = context.Request.Headers.Accept.ToString();
			var isApiRequest = acceptHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase);
			
			if (isApiRequest)
			{
				// For API requests, return 401 without redirect
				context.HandleResponse();
				context.Response.StatusCode = 401;
				return context.Response.WriteAsync("Unauthorized");
			}
			else
			{
				// For browser requests, redirect to login page
				context.Response.StatusCode = 302;
				context.Response.Headers.Location = "/login";
				return Task.CompletedTask;
			}
		}
	};
});

// Configure Authorization Policies
services.AddPolicies();

// Add SignalR
services.AddSignalR();
services.AddResponseCompression(opts =>
{
	opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});


// Configure database connection
ServiceInitialization.ConfigureDatabaseProvider(services, configuration);

// Initialize all dependencies for services
ServiceInitialization.Initialize(services, configuration);

// Identity Services already configured above with JWT

var app = builder.Build();

app.Logger.LogInformation("Starting API");

// Automatically updates database schema by auto-applying migrations
using (var scope = app.Services.CreateScope())
{
	void MigrateDatabase<T>() where T : DbContext
	{
		var dbContext = scope.ServiceProvider.GetRequiredService<T>();
		dbContext.Database.Migrate();
	}
	MigrateDatabase<VHSCoreDBContext>();
	MigrateDatabase<VHSAuthDBContext>();
	MigrateDatabase<VHSAuditDBContext>();

	app.Logger.LogInformation("DB Migrated");

	// Identity and Global Admin Seeder
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
	var adminSection = builder.Configuration.GetSection("DefaultGlobalAdmin");
	string email = adminSection["Email"];
	string password = "nPz1N06f@3TT$Qe%";//TODO: remove hardcoded password in production
	string firstName = adminSection["FirstName"];
	string lastName = adminSection["LastName"];

	await IdentitySeeder.SeedRolesAsync(roleManager);
	await IdentitySeeder.SeedAdminUserAsync(userManager, roleManager, email, password, firstName, lastName);
}

// Enable CORS
app.UseCors("AllowBlazorClient");

// Middlewares
// Enable request tracing middleware
app.UseSentryTracing();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Test")
{
	// Enable Swagger middleware
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "VHS.WebAPI v1");
		c.RoutePrefix = "swagger";
		// Visit http://localhost:5001/swagger/index.html (port 5001 for http, 7001 for https set on launch settings)
	});
}

// Remove X-Powered-By header
app.UseMiddleware<RemoveXPoweredByMiddleware>();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseResponseCompression();

// Map SignalR Hub
app.MapHub<VHSNotificationHub>("/hubs/notifications");

app.Logger.LogInformation("Started API");

app.Run();



