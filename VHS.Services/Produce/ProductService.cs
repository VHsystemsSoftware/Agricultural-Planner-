using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VHS.Services.Audit;
using VHS.Services.Audit.DTO;
using VHS.Services.Produce.DTO;

namespace VHS.Services;

public interface IProductService
{
	Task<IEnumerable<ProductDTO>> GetAllProductsAsync(Guid? farmId = null);
	Task<ProductDTO?> GetProductByIdAsync(Guid id);
	Task<ProductDTO> CreateProductAsync(ProductDTO productDto, string userId);
	Task UpdateProductAsync(ProductDTO productDto, string userId);
	Task DeleteProductAsync(Guid id, string userId);
	Task<string?> GetProductImageDataAsync(Guid productId);
}
public static class ProductDTOSelect
{
	public static IQueryable<ProductDTO> MapProductToDTO(this IQueryable<Product> data)
	{
		var method = System.Reflection.MethodBase.GetCurrentMethod();
		return data.TagWith(method.Name)
			.Select(x => new ProductDTO
			{
				Id = x.Id,
				ProductCategoryId = x.ProductCategoryId,
				FarmId = x.FarmId,
				Name = x.Name,
				Description = x.Description,
				SeedIdentifier = x.SeedIdentifier,
				SeedSupplier = x.SeedSupplier,
				AddedDateTime = x.AddedDateTime,
				ModifiedDateTime = x.ModifiedDateTime,
                HasImage = !string.IsNullOrEmpty(x.ImageData)
            });
	}
}

public class ProductService : IProductService
{
	private readonly IUnitOfWorkCore _unitOfWork;
	private readonly IAuditLogService _auditLogService;

	public ProductService(IUnitOfWorkCore unitOfWork, IAuditLogService auditLogService)
	{
		_unitOfWork = unitOfWork;
		_auditLogService = auditLogService;
	}

	private static ProductDTO SelectProductToDTO(Product p) => new ProductDTO
	{
		Id = p.Id,
		ProductCategoryId = p.ProductCategoryId,
		FarmId = p.FarmId,
		Name = p.Name,
		Description = p.Description,
		SeedIdentifier = p.SeedIdentifier,
		SeedSupplier = p.SeedSupplier,
		AddedDateTime = p.AddedDateTime,
		ModifiedDateTime = p.ModifiedDateTime
	};

	public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync(Guid? farmId = null)
	{
		return await _unitOfWork.Product
			.Query(x => !farmId.HasValue || x.FarmId == farmId.Value)
			.MapProductToDTO()
			.AsNoTracking()
			.OrderBy(p => p.Name)
			.ToListAsync();
	}

	public async Task<ProductDTO?> GetProductByIdAsync(Guid id)
	{
		var product = await _unitOfWork.Product.GetByIdAsync(id);
		if (product == null)
			return null;

		return SelectProductToDTO(product);
	}

	public async Task<ProductDTO> CreateProductAsync(ProductDTO productDto, string userId)
	{
		var p = new Product
		{
			Id = productDto.Id == Guid.Empty ? Guid.NewGuid() : productDto.Id,
			ProductCategoryId = productDto.ProductCategoryId,
			Name = productDto.Name,
			FarmId = productDto.FarmId,
			Description = productDto.Description,
			ImageData = productDto.ImageData,
			SeedIdentifier = productDto.SeedIdentifier,
			SeedSupplier = productDto.SeedSupplier,
		};

		await _unitOfWork.Product.AddAsync(p);
		var result = await _unitOfWork.SaveChangesAsync();

		if (result > 0)
		{
			var newProductDto = await GetProductByIdAsync(p.Id);
			await CreateAuditLogAsync("Added", userId, p.Id, null, newProductDto);
		}

		return await GetProductByIdAsync(p.Id);
	}

	public async Task UpdateProductAsync(ProductDTO productDto, string userId)
	{
		var product = await _unitOfWork.Product.GetByIdAsync(productDto.Id);
		if (product == null)
			throw new Exception("Product not found");

		var oldProductDto = SelectProductToDTO(product);

		product.Name = productDto.Name;
		product.Description = productDto.Description;
		product.ImageData = string.IsNullOrWhiteSpace(productDto.ImageData) ? null : productDto.ImageData;
		product.SeedIdentifier = productDto.SeedIdentifier;
		product.ProductCategoryId = productDto.ProductCategoryId;
		product.SeedSupplier = productDto.SeedSupplier;

		_unitOfWork.Product.Update(product);
		var result = await _unitOfWork.SaveChangesAsync();

		if (result > 0)
		{
			var newProductDto = SelectProductToDTO(product);
			await CreateAuditLogAsync("Modified", userId, product.Id, oldProductDto, newProductDto);
		}
	}

	public async Task DeleteProductAsync(Guid id, string userId)
	{
		var product = await _unitOfWork.Product.GetByIdAsync(id);
		if (product == null)
			throw new Exception("Product not found");

		var oldProductDto = SelectProductToDTO(product);

		product.DeletedDateTime = DateTime.UtcNow;
		_unitOfWork.Product.Update(product);
		var result = await _unitOfWork.SaveChangesAsync();

		if (result > 0)
		{
			await CreateAuditLogAsync("Deleted", userId, product.Id, oldProductDto, null);
		}
	}

	public async Task<string?> GetProductImageDataAsync(Guid productId)
	{
		var product = await _unitOfWork.Product.GetByIdAsync(productId);
		return product?.ImageData;
	}

	private async Task CreateAuditLogAsync(string action, string userId, Guid entityId, ProductDTO? oldDto, ProductDTO? newDto)
	{
		var auditLog = new AuditLogDTO
		{
			UserId = string.IsNullOrEmpty(userId) ? "SYSTEM" : userId,
			EntityName = nameof(Product),
			Action = action,
			Timestamp = DateTime.UtcNow,
			KeyValues = JsonSerializer.Serialize(new { Id = entityId }),
			OldValues = oldDto == null ? null : JsonSerializer.Serialize(oldDto),
			NewValues = newDto == null ? null : JsonSerializer.Serialize(newDto)
		};

		await _auditLogService.CreateAuditLogAsync(auditLog);
	}
}
