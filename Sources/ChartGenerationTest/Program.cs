using Microcharts;
using SkiaSharp;
using System;
using System.Diagnostics;

namespace ChartGenerationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var entries = new[]
            {
                new Entry(212)
                {
                    Label = "UWP",
                    ValueLabel = "212",
                    Color = SKColor.Parse("#2c3e50"),
                },
                new Entry(-248)
                {
                    Label = "Android",
                    ValueLabel = "248",
                    Color = SKColor.Parse("#77d065")
                },
                new Entry(-128)
                {
                    Label = "iOS",
                    ValueLabel = "128",
                    Color = SKColor.Parse("#b455b6")
                },
                new Entry(514)
                {
                    Label = "Shared",
                    ValueLabel = "514",
                    Color = SKColor.Parse("#3498db")
                }
            };

            var chart = new LineChart
            {
                Entries = entries,
                IsValueLabelNearValuePoints = true,
                Orientation = ChartOrientation.Vertical,
                BackgroundColor = SKColors.White,
                //HoleRadius = 0.5f,
            };

            using (var bmp = new SKBitmap(512, 512))
            using (var canvas = new SKCanvas(bmp))
            using (var stream = new SKFileWStream("image.png"))
            {
                chart.Draw(canvas, 512, 512);
                SKPixmap.Encode(stream, bmp, SKEncodedImageFormat.Png, 100);
            }

            Process.Start("explorer", "image.png");
        }
    }
}
