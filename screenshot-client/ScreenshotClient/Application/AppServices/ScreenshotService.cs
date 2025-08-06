using ScreenshotClient.Domain.Interfaces;
using System.Runtime.InteropServices;
using ScreenshotCommonLib.Enums;
using System.Threading.Tasks;
using ScreenCapture.NET;
using System.Linq;
using SkiaSharp;
using System.IO;
using System;

namespace ScreenshotClient.AppServices
{
    public class ScreenshotService : IScreenshotService, IDisposable
    {
        private IScreenCaptureService? _captureService;
        private IScreenCapture? _screenCapture;
        private bool _disposed;

        public async Task<byte[]> CaptureAreaAsync(int x, int y, int width, int height)
        {
            return await Task.Run(() => CaptureArea(x, y, width, height));
        }

        public async Task SaveScreenshotAsync(byte[] imageData, string filePath, ImageFormat format)
        {
            await Task.Run(() => SaveScreenshot(imageData, filePath, format));
        }

        private byte[] CaptureArea(int x, int y, int width, int height)
        {
            EnsureScreenCaptureInitialized();

            if (_screenCapture == null)
                throw new InvalidOperationException("Screen capture not initialized");

            var zone = _screenCapture.RegisterCaptureZone(x, y, width, height);
            _screenCapture.CaptureScreen();
            
            byte[] result;
            using (zone.Lock())
            {
                var rawData = zone.RawBuffer;
                result = rawData.ToArray();
            }
            
            return result;
        }

        private void EnsureScreenCaptureInitialized()
        {
            if (_captureService != null && _screenCapture != null)
                return;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _captureService = new DX11ScreenCaptureService();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _captureService = new X11ScreenCaptureService();
                }
                else
                {
                    throw new PlatformNotSupportedException("Unsupported platform for screen capture");
                }

                var graphicsCards = _captureService.GetGraphicsCards();
                var displays = _captureService.GetDisplays(graphicsCards.First());
                _screenCapture = _captureService.GetScreenCapture(displays.First());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize screen capture: {ex.Message}", ex);
            }
        }



        private static void SaveScreenshot(byte[] imageData, string filePath, ImageFormat format)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var inputStream = new MemoryStream(imageData);
            using var bitmap = SKBitmap.Decode(inputStream);
            
            if (bitmap == null)
                throw new InvalidOperationException("Failed to decode image data");

            var skFormat = ConvertToSkiaFormat(format);
            var quality = format == ImageFormat.JPEG ? 90 : 100;

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(skFormat, quality);
            using var fileStream = File.Create(filePath);
            
            data.SaveTo(fileStream);
        }

        private static SKEncodedImageFormat ConvertToSkiaFormat(ImageFormat format)
        {
            return format switch
            {
                ImageFormat.PNG => SKEncodedImageFormat.Png,
                ImageFormat.JPEG => SKEncodedImageFormat.Jpeg,
                ImageFormat.BMP => SKEncodedImageFormat.Bmp,
                ImageFormat.WEBP => SKEncodedImageFormat.Webp,
                _ => SKEncodedImageFormat.Png
            };
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _screenCapture?.Dispose();
                _captureService?.Dispose();
                _disposed = true;
            }
        }
    }
}