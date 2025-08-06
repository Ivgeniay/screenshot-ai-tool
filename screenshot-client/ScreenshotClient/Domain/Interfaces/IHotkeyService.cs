using System;

namespace ScreenshotClient.Domain.Interfaces
{
    public interface IHotkeyService
    {
        bool RegisterHotkey(string hotkeyString, Action callback);
        void UnregisterHotkey(string hotkeyString);
        void UnregisterAllHotkeys();
        bool IsHotkeyRegistered(string hotkeyString);
    }
}