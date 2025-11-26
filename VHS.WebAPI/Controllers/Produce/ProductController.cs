using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using VHS.Services.Produce.DTO;

namespace VHS.WebAPI.Controllers.Produce;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    [HttpGet]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetAllProduceTypes(Guid? farmId = null)
    {
        var produceTypes = await _productService.GetAllProductsAsync(farmId);
        return Ok(produceTypes);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "FarmManagerAndAbove")]
    public async Task<IActionResult> GetProduceTypeById(Guid id)
    {
        var produceType = await _productService.GetProductByIdAsync(id);
        if (produceType == null)
        {
            return NotFound();
        }
        return Ok(produceType);
    }

    [HttpPost]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> CreateProduceType([FromBody] ProductDTO productDto)
    {
        var createdProduceType = await _productService.CreateProductAsync(productDto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetProduceTypeById), new { id = createdProduceType.Id }, createdProduceType);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> UpdateProduceType(Guid id, [FromBody] ProductDTO productDto)
    {
        if (id != productDto.Id)
        {
            return BadRequest("ID mismatch");
        }
        await _productService.UpdateProductAsync(productDto, GetCurrentUserId());
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> DeleteProduceType(Guid id)
    {
        await _productService.DeleteProductAsync(id, GetCurrentUserId());
        return NoContent();
    }

    [HttpGet("image/{productId}")]
    //[Authorize(Policy = "FarmManagerAndAbove")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductImage(Guid productId)
    {
        var imageDataBase64 = await _productService.GetProductImageDataAsync(productId);

        if (string.IsNullOrEmpty(imageDataBase64))
        {
            return NotFound();
        }

        string pureBase64String;
        string contentType;

        var match = Regex.Match(imageDataBase64, @"^data:(?<contentType>[^;]+);base64,(?<base64Data>.+)$");
        if (match.Success)
        {
            contentType = match.Groups["contentType"].Value;
            pureBase64String = match.Groups["base64Data"].Value;
        }
        else
        {
            pureBase64String = imageDataBase64;
            contentType = "image/png";
        }

        try
        {
            var imageData = Convert.FromBase64String(pureBase64String);
            return File(imageData, contentType);
        }
        catch (FormatException)
        {
            return BadRequest("Invalid image data format after stripping prefix.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing image: {ex.Message}");
        }
    }

    [HttpPost("validate-image")]
    [Authorize(Policy = "CanAccessPlanningOperations")]
    public async Task<IActionResult> ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        const int MaxWidth = 2048;
        const int MaxHeight = 2048;
        const long MaxFileSize = 3 * 2048 * 2048;

        if (file.Length > MaxFileSize)
            return BadRequest($"File size exceeds 3MB. Image is {file.Length / 1024}KB.");

        using var image = await Image.LoadAsync(file.OpenReadStream());

        if (image.Width > MaxWidth || image.Height > MaxHeight)
            return BadRequest($"Image exceeds maximum dimensions of 2048x2048px. Image is {image.Width}x{image.Height}.");

        using var outputStream = new MemoryStream();
        await image.SaveAsPngAsync(outputStream);
        var base64 = Convert.ToBase64String(outputStream.ToArray());

        return Ok(new ProductDTO
        {
            ImageData = $"data:image/png;base64,{base64}",
            HasImage = true
        });
    }
}
