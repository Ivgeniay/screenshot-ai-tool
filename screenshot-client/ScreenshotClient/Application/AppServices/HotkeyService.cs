using ScreenshotClient.Domain.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SharpHook.Data;
using SharpHook;
using System;

namespace ScreenshotClient.AppServices
{
    public class HotkeyService : IHotkeyService, IDisposable
    {
        private readonly ConcurrentDictionary<string, HotkeyInfo> _registeredHotkeys = new();
        private readonly IGlobalHook _globalHook;
        private bool _disposed;

        public HotkeyService()
        {
            _globalHook = new TaskPoolGlobalHook();
            _globalHook.KeyPressed += OnKeyPressed;
        }

        public bool RegisterHotkey(string hotkeyString, Action callback)
        {
            if (string.IsNullOrWhiteSpace(hotkeyString))
                throw new ArgumentException("Строка хоткея не может быть пустой", nameof(hotkeyString));
            
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (_registeredHotkeys.ContainsKey(hotkeyString))
                return false;

            try
            {
                var (keyCode, modifiers) = ParseHotkeyString(hotkeyString);
                var hotkeyInfo = new HotkeyInfo(hotkeyString, callback, keyCode, modifiers);
                
                _registeredHotkeys[hotkeyString] = hotkeyInfo;
                
                if (!_globalHook.IsRunning)
                {
                    Task.Run(() => _globalHook.RunAsync());
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void UnregisterHotkey(string hotkeyString)
        {
            _registeredHotkeys.TryRemove(hotkeyString, out _);
        }

        public void UnregisterAllHotkeys()
        {
            _registeredHotkeys.Clear();
        }

        public bool IsHotkeyRegistered(string hotkeyString)
        {
            return _registeredHotkeys.ContainsKey(hotkeyString);
        }

        private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            foreach (var hotkey in _registeredHotkeys.Values)
            {
                if (IsHotkeyMatch(hotkey, e))
                {
                    hotkey.Callback.Invoke();
                    break;
                }
            }
        }

        private static bool IsHotkeyMatch(HotkeyInfo hotkey, KeyboardHookEventArgs e)
        {
            if (e.Data.KeyCode != hotkey.KeyCode)
                return false;

            if (hotkey.Modifiers == (ushort)EventMask.None)
                return true;

            return CheckAnyModifierMatch(hotkey.Modifiers, e.RawEvent.Mask);
        }

        private static bool CheckModifiers(EventMask expectedModifiers, EventMask actualMask)
        {
            return (actualMask & expectedModifiers) == expectedModifiers;
        }

        private static (KeyCode keyCode, EventMask modifiers) ParseHotkeyString(string hotkeyString)
        {
            var parts = hotkeyString.Split('+');
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim().ToLower();
            }
            var modifiers = EventMask.None;
            var keyCode = KeyCode.VcUndefined;

            foreach (var part in parts)
            {
                var trimmed = part.Trim().ToLower();
                switch (trimmed)
                {
                    case "ctrl":
                        modifiers |= EventMask.LeftCtrl | EventMask.RightCtrl;
                        break;
                    case "alt":
                        modifiers |= EventMask.LeftAlt | EventMask.RightAlt;
                        break;
                    case "shift":
                        modifiers |= EventMask.LeftShift | EventMask.RightShift;
                        break;
                    case "win":
                        modifiers |= EventMask.LeftMeta | EventMask.RightMeta;
                        break;
                    case "leftctrl":
                        modifiers |= EventMask.LeftCtrl;
                        break;
                    case "rightctrl":
                        modifiers |= EventMask.RightCtrl;
                        break;
                    case "leftalt":
                        modifiers |= EventMask.LeftAlt;
                        break;
                    case "rightalt":
                        modifiers |= EventMask.RightAlt;
                        break;
                    case "leftshift":
                        modifiers |= EventMask.LeftShift;
                        break;
                    case "rightshift":
                        modifiers |= EventMask.RightShift;
                        break;
                    case "leftwin":
                        modifiers |= EventMask.LeftMeta;
                        break;
                    case "rightwin":
                        modifiers |= EventMask.RightMeta;
                        break;
                    default:
                        if (trimmed.Length == 1 && char.IsLetter(trimmed[0]))
                        {
                            var letter = char.ToUpper(trimmed[0]);
                            keyCode = GetKeyCodeFromLetter(letter);
                        }
                        break;
                }
            }

            return (keyCode, modifiers);
        }

        private static KeyCode GetKeyCodeFromLetter(char letter)
        {
            return letter switch
            {
                'A' => KeyCode.VcA,
                'B' => KeyCode.VcB,
                'C' => KeyCode.VcC,
                'D' => KeyCode.VcD,
                'E' => KeyCode.VcE,
                'F' => KeyCode.VcF,
                'G' => KeyCode.VcG,
                'H' => KeyCode.VcH,
                'I' => KeyCode.VcI,
                'J' => KeyCode.VcJ,
                'K' => KeyCode.VcK,
                'L' => KeyCode.VcL,
                'M' => KeyCode.VcM,
                'N' => KeyCode.VcN,
                'O' => KeyCode.VcO,
                'P' => KeyCode.VcP,
                'Q' => KeyCode.VcQ,
                'R' => KeyCode.VcR,
                'S' => KeyCode.VcS,
                'T' => KeyCode.VcT,
                'U' => KeyCode.VcU,
                'V' => KeyCode.VcV,
                'W' => KeyCode.VcW,
                'X' => KeyCode.VcX,
                'Y' => KeyCode.VcY,
                'Z' => KeyCode.VcZ,
                _ => KeyCode.VcUndefined
            };
        }

        private static bool CheckAnyModifierMatch(EventMask expectedModifiers, EventMask actualMask)
        {
            if (expectedModifiers.HasFlag(EventMask.LeftCtrl | EventMask.RightCtrl))
            {
                if (!actualMask.HasFlag(EventMask.LeftCtrl) && !actualMask.HasFlag(EventMask.RightCtrl))
                    return false;
            }
            else if (expectedModifiers.HasFlag(EventMask.LeftCtrl) && !actualMask.HasFlag(EventMask.LeftCtrl))
                return false;
            else if (expectedModifiers.HasFlag(EventMask.RightCtrl) && !actualMask.HasFlag(EventMask.RightCtrl))
                return false;

            if (expectedModifiers.HasFlag(EventMask.LeftAlt | EventMask.RightAlt))
            {
                if (!actualMask.HasFlag(EventMask.LeftAlt) && !actualMask.HasFlag(EventMask.RightAlt))
                    return false;
            }
            else if (expectedModifiers.HasFlag(EventMask.LeftAlt) && !actualMask.HasFlag(EventMask.LeftAlt))
                return false;
            else if (expectedModifiers.HasFlag(EventMask.RightAlt) && !actualMask.HasFlag(EventMask.RightAlt))
                return false;

            if (expectedModifiers.HasFlag(EventMask.LeftShift | EventMask.RightShift))
            {
                if (!actualMask.HasFlag(EventMask.LeftShift) && !actualMask.HasFlag(EventMask.RightShift))
                    return false;
            }
            else if (expectedModifiers.HasFlag(EventMask.LeftShift) && !actualMask.HasFlag(EventMask.LeftShift))
                return false;
            else if (expectedModifiers.HasFlag(EventMask.RightShift) && !actualMask.HasFlag(EventMask.RightShift))
                return false;

            if (expectedModifiers.HasFlag(EventMask.LeftMeta | EventMask.RightMeta))
            {
                if (!actualMask.HasFlag(EventMask.LeftMeta) && !actualMask.HasFlag(EventMask.RightMeta))
                    return false;
            }
            else if (expectedModifiers.HasFlag(EventMask.LeftMeta) && !actualMask.HasFlag(EventMask.LeftMeta))
                return false;
            else if (expectedModifiers.HasFlag(EventMask.RightMeta) && !actualMask.HasFlag(EventMask.RightMeta))
                return false;

            return true;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                UnregisterAllHotkeys();
                _globalHook?.Dispose();
                _disposed = true;
            }
        }

        private record HotkeyInfo(string HotkeyString, Action Callback, KeyCode KeyCode, EventMask Modifiers);
    }
}