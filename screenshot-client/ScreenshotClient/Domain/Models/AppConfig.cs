using ScreenshotCommonLib.Enums;
using Newtonsoft.Json;

namespace ScreenshotClient.Models
{
    public class AppConfig
    {
        [JsonProperty("hotKey")]
        public string HotKey { get; set; } = "Ctrl+P";

        [JsonProperty("apiUrl")]
        public string ApiUrl { get; set; } = "http://localhost:5000";

        [JsonProperty("saveDirectory")]
        public string SaveDirectory { get; set; } = "./screenshots";

        [JsonProperty("imageFormat")]
        public ImageFormat ImageFormat { get; set; } = ImageFormat.PNG;

        [JsonProperty("toastDisplayTime")]
        public int ToastDisplayTime { get; set; } = 3000;

        [JsonProperty("toastPosition")]
        public ToastPosition ToastPosition { get; set; } = ToastPosition.TopRight;

        [JsonProperty("compressionQuality")]
        public int CompressionQuality { get; set; } = 90;

        [JsonProperty("enableLogging")]
        public bool EnableLogging { get; set; } = true;

        [JsonProperty("logLevel")]
        public string LogLevel { get; set; } = "Information";
    }
}