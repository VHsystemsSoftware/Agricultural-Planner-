using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using VHS.Services.Produce.DTO;

namespace VHS.Client.Services.Produce
{
    public class ProductClientService
    {
        private readonly HttpClient _httpClient;

        public ProductClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProductDTO>?> GetAllProductsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ProductDTO>>("api/product");
        }

        public async Task<ProductDTO?> GetProductByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<ProductDTO>($"api/product/{id}");
        }

        public async Task<ProductDTO?> CreateProductAsync(ProductDTO produceType)
        {
            var response = await _httpClient.PostAsJsonAsync("api/product", produceType);
            return await response.Content.ReadFromJsonAsync<ProductDTO>();
        }

        public async Task UpdateProductAsync(ProductDTO productDto)
        {
            await _httpClient.PutAsJsonAsync($"api/product/{productDto.Id}", productDto);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"api/product/{id}");
        }

        public string GetProductImageUrl(Guid productId)
        {
            var imageUrl = new Uri(_httpClient.BaseAddress, $"/api/product/image/{productId}").ToString();
            return imageUrl;
        }

        public async Task<ProductDTO?> ValidateAndUploadImageAsync(IBrowserFile file)
        {
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 3 * 2048 * 2048));
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(streamContent, "file", file.Name);

            var response = await _httpClient.PostAsync("api/product/validate-image", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<ProductDTO>();
        }
    }
}
