using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MedCentre.security;

public class CaptchaGenerator
    {
        private static readonly Random random = new Random();
        private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        
        public string CurrentCaptchaText { get; private set; }

        public string GenerateCaptchaText(int length = 4)
        {
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = Chars[random.Next(Chars.Length)];
            }
            CurrentCaptchaText = new string(result);
            return CurrentCaptchaText;
        }

        public Image GenerateCaptchaImage(string captchaText)
        {
            int width = 150;
            int height = 50;
            
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Pbgra32);
            
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(
                    new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    null,
                    new Rect(0, 0, width, height));
                
                for (int i = 0; i < 5; i++)
                {
                    int x1 = random.Next(width);
                    int y1 = random.Next(height);
                    int x2 = random.Next(width);
                    int y2 = random.Next(height);
                    
                    drawingContext.DrawLine(
                        new Pen(new SolidColorBrush(Color.FromRgb(
                            (byte)random.Next(100, 200),
                            (byte)random.Next(100, 200),
                            (byte)random.Next(100, 200))), 1),
                        new Point(x1, y1),
                        new Point(x2, y2));
                }
                
                Typeface typeface = new Typeface(new FontFamily("Arial"), 
                    FontStyles.Italic, FontWeights.Bold, FontStretches.Normal);
                
                FormattedText formattedText = new FormattedText(
                    captchaText,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    24,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(new Image()).PixelsPerDip);
                
                drawingContext.DrawText(
                    formattedText,
                    new Point((width - formattedText.Width) / 2, (height - formattedText.Height) / 2));
            }

            renderBitmap.Render(drawingVisual);
            
            Image image = new Image
            {
                Source = renderBitmap,
                Width = width,
                Height = height,
                Stretch = Stretch.None
            };

            return image;
        }
        
        public bool ValidateCaptcha(string userInput)
        {
            return !string.IsNullOrEmpty(userInput) && 
                   userInput.Trim().Equals(CurrentCaptchaText, StringComparison.OrdinalIgnoreCase);
        }
    }