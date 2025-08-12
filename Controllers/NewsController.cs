using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly ILogger<NewsController> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public NewsController(ILogger<NewsController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

    [HttpGet("top-headlines")]
    public async Task<IActionResult> GetTopHeadlines([FromQuery] string category = "general", [FromQuery] string country = "us")
    {
        try
        {
            // Debug: Log all configuration values
            _logger.LogInformation("Configuration values:");
            _logger.LogInformation("NewsAPI:ApiKey = {ApiKey}", _configuration["NewsAPI:ApiKey"]);
            _logger.LogInformation("NewsAPI__ApiKey = {ApiKey}", _configuration["NewsAPI__ApiKey"]);
            _logger.LogInformation("Environment variable = {EnvVar}", Environment.GetEnvironmentVariable("NewsAPI__ApiKey"));
            
            // Try multiple ways to get the API key
            var newsApiKey = _configuration["NewsAPI:ApiKey"] 
                             ?? _configuration["NewsAPI__ApiKey"]
                             ?? Environment.GetEnvironmentVariable("NewsAPI__ApiKey");
            
            if (string.IsNullOrEmpty(newsApiKey))
            {
                _logger.LogError("NewsAPI key not configured");
                return StatusCode(500, new { error = "NewsAPI key not configured" });
            }

            var url = $"https://newsapi.org/v2/top-headlines?country={country}&category={category}&apiKey={newsApiKey}";
            _logger.LogInformation("Fetching news from NewsAPI: {Url}", url.Replace(newsApiKey, "***"));

            // Add required User-Agent header
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TodoApp/1.0 (https://github.com/yourusername/todoapp)");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("News API response received: {StatusCode}, Content length: {Length}", 
                    response.StatusCode, content.Length);

                // Log the raw response for debugging (without the API key)
                _logger.LogDebug("Raw NewsAPI response: {Content}", content);

                // Parse and return the JSON response directly
                // This ensures the frontend gets the exact structure from NewsAPI
                var newsData = JsonSerializer.Deserialize<object>(content);
                return Ok(newsData);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("News API returned status code: {StatusCode}, Content: {Content}", 
                response.StatusCode, errorContent);
            
            return StatusCode((int)response.StatusCode, new { error = "News API request failed", details = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching news for category: {Category}", category);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        var categories = new[]
        {
            "general", "business", "technology", "sports", 
            "entertainment", "health", "science"
        };
        
        return Ok(categories);
    }
} 