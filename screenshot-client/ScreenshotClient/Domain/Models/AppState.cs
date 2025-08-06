namespace ScreenshotClient.Models
{
    public enum AppState
    {
        Idle,
        SelectingArea,
        ProcessingScreenshot,
        SendingToAI,
        ShowingResult
    }
}