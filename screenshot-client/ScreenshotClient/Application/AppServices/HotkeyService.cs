using ScreenshotClient.Domain.Interfaces;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using ScreenshotClient.Interfaces;
using System;

namespace ScreenshotClient.AppServices
{
    public class HotkeyService : IHotkeyService, IDisposable
    {
        private readonly ConcurrentDictionary<string, HotkeyInfo> _registeredHotkeys = new();
        private readonly ConcurrentDictionary<int, HotkeyInfo> _hotkeyById = new();
        private int _nextHotkeyId = 1;
        private bool _disposed;

        public bool RegisterHotkey(string hotkeyString, Action callback)
        {
            if (string.IsNullOrWhiteSpace(hotkeyString))
                throw new ArgumentException("Hotkey string cannot be null or empty", nameof(hotkeyString));
            
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (_registeredHotkeys.ContainsKey(hotkeyString))
                return false;

            var (modifiers, key) = ParseHotkeyString(hotkeyString);
            var hotkeyId = _nextHotkeyId++;

            var success = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? RegisterWindowsHotkey(hotkeyId, modifiers, key)
                : RegisterLinuxHotkey(hotkeyId, modifiers, key);

            if (success)
            {
                var hotkeyInfo = new HotkeyInfo(hotkeyId, hotkeyString, callback, modifiers, key);
                _registeredHotkeys[hotkeyString] = hotkeyInfo;
                _hotkeyById[hotkeyId] = hotkeyInfo;
            }

            return success;
        }

        public void UnregisterHotkey(string hotkeyString)
        {
            if (_registeredHotkeys.TryRemove(hotkeyString, out var hotkeyInfo))
            {
                _hotkeyById.TryRemove(hotkeyInfo.Id, out _);
                
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    UnregisterWindowsHotkey(hotkeyInfo.Id);
                else
                    UnregisterLinuxHotkey(hotkeyInfo.Id);
            }
        }

        public void UnregisterAllHotkeys()
        {
            foreach (var hotkey in _registeredHotkeys.Values)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    UnregisterWindowsHotkey(hotkey.Id);
                else
                    UnregisterLinuxHotkey(hotkey.Id);
            }
            
            _registeredHotkeys.Clear();
            _hotkeyById.Clear();
        }

        public bool IsHotkeyRegistered(string hotkeyString)
        {
            return _registeredHotkeys.ContainsKey(hotkeyString);
        }

        private static (uint modifiers, uint key) ParseHotkeyString(string hotkeyString)
        {
            var parts = hotkeyString.Split('+');
            uint modifiers = 0;
            uint key = 0;

            foreach (var part in parts)
            {
                var trimmed = part.Trim().ToLower();
                switch (trimmed)
                {
                    case "ctrl":
                        modifiers |= RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 0x0002u : 0x04u;
                        break;
                    case "alt":
                        modifiers |= RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 0x0001u : 0x08u;
                        break;
                    case "shift":
                        modifiers |= RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 0x0004u : 0x01u;
                        break;
                    case "win":
                        modifiers |= RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 0x0008u : 0x40u;
                        break;
                    default:
                        if (trimmed.Length == 1)
                            key = char.ToUpper(trimmed[0]);
                        break;
                }
            }

            return (modifiers, key);
        }

        private static bool RegisterWindowsHotkey(int id, uint modifiers, uint key)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;
                
            return true;
        }

        private static bool RegisterLinuxHotkey(int id, uint modifiers, uint key)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return false;
                
            return true;
        }

        private static void UnregisterWindowsHotkey(int id)
        {
        }

        private static void UnregisterLinuxHotkey(int id)
        {
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                UnregisterAllHotkeys();
                _disposed = true;
            }
        }

        private record HotkeyInfo(int Id, string HotkeyString, Action Callback, uint Modifiers, uint Key);
    }
}