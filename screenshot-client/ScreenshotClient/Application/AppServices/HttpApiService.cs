using ScreenshotClient.Domain.Interfaces;
using ScreenshotClient.Interfaces;
using ScreenshotCommonLib.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System;

namespace ScreenshotClient.AppServices
{
    public class HttpApiService : IHttpApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfigService _configService;

        public HttpApiService(HttpClient httpClient, IConfigService configService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(HttpClient));
            _configService = configService ?? throw new ArgumentNullException(nameof(IConfigService));
        }

        public async Task<AnalyzeImageResponse> AnalyzeImageAsync(byte[] imageData, string prompt)
        {
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentException("Image data cannot be null or empty", nameof(imageData));
            
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            try
            {
                var config = await _configService.GetConfigAsync();
                var request = new AnalyzeImageRequest
                {
                    Image = imageData,
                    Prompt = prompt
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{config.ApiUrl}/api/analyze", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<AnalyzeImageResponse>(responseJson);
                    return result ?? new AnalyzeImageResponse
                    {
                        Success = false,
                        ErrorMessage = "Failed to deserialize response"
                    };
                }

                return new AnalyzeImageResponse
                {
                    Success = false,
                    ErrorMessage = $"API request failed with status {response.StatusCode}: {responseJson}"
                };
            }
            catch (HttpRequestException ex)
            {
                return new AnalyzeImageResponse
                {
                    Success = false,
                    ErrorMessage = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new AnalyzeImageResponse
                {
                    Success = false,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                };
            }
        }
    }
}