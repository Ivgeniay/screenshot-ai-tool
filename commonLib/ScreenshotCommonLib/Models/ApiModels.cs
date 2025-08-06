namespace ScreenshotCommonLib.Models
{
    public class AnalyzeImageRequest
    {
        public required byte[] Image { get; set; }
        public required string Prompt { get; set; }
    }

    public class AnalyzeImageResponse
    {
        public bool Success { get; set; }
        public string? Result { get; set; }
        public string? ErrorMessage { get; set; }
        public long ProcessingTimeMs { get; set; }
    }

    public class ApiError
    {
        public required string Message { get; set; }
        public string? Details { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
