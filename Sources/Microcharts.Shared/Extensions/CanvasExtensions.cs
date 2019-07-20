// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using SkiaSharp;

    /// <summary>
    /// 
    /// </summary>
    public static class CanvasExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="label"></param>
        /// <param name="labelColor"></param>
        /// <param name="value"></param>
        /// <param name="valueColor"></param>
        /// <param name="textSize"></param>
        /// <param name="labelStrokeColor"></param>
        /// <param name="labelStrokeWidth"></param>
        /// <param name="valueStrokeColor"></param>
        /// <param name="valueStrokeWidth"></param>
        /// <param name="point"></param>
        /// <param name="horizontalAlignment"></param>
        public static void DrawCaptionLabels(
            this SKCanvas canvas, 
            string label, 
            SKColor labelColor, 
            string value, 
            SKColor valueColor, 
            float textSize, 
            SKColor labelStrokeColor,
            float labelStrokeWidth,
            SKColor valueStrokeColor,
            float valueStrokeWidth,
            SKPoint point, 
            SKTextAlign horizontalAlignment)
        {
            var hasLabel = !string.IsNullOrEmpty(label);
            var hasValueLabel = !string.IsNullOrEmpty(value);

            if (!hasLabel && !hasValueLabel)
            {
                return;
            }

            var hasOffset = hasLabel && hasValueLabel;
            var captionMargin = textSize * 0.60f;
            var space = hasOffset ? captionMargin : 0;

            if (hasLabel)
            {
                using (var paint = new SKPaint
                {
                    TextSize = textSize,
                    IsAntialias = true,
                    Color = labelColor,
                    IsStroke = false,
                    FakeBoldText = false,
                    TextAlign = horizontalAlignment,
                })
                {
                    var bounds = new SKRect();
                    var text = label;
                    paint.MeasureText(text, ref bounds);

                    var y = point.Y - ((bounds.Top + bounds.Bottom) / 2) - space;

                    // draw outline
                    if (labelStrokeWidth > 0f && labelStrokeColor != SKColor.Empty)
                    {
                        using (var strokePaint = new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            TextSize = textSize,
                            IsAntialias = true,
                            IsStroke = true,
                            FakeBoldText = true,
                            StrokeWidth = labelStrokeWidth,
                            Color = labelStrokeColor,
                            TextAlign = horizontalAlignment,
                        })
                            canvas.DrawText(text, point.X, y, strokePaint);
                    }

                    canvas.DrawText(text, point.X, y, paint);
                }
            }

            if (hasValueLabel)
            {
                using (var paint = new SKPaint()
                {
                    Style = SKPaintStyle.StrokeAndFill,
                    TextSize = textSize,
                    IsAntialias = true,
                    FakeBoldText = false,
                    Color = valueColor,
                    IsStroke = false,
                    StrokeWidth = 1f,
                    TextAlign = horizontalAlignment,
                })
                {
                    var bounds = new SKRect();
                    var text = value;
                    paint.MeasureText(text, ref bounds);

                    var y = point.Y - ((bounds.Top + bounds.Bottom) / 2) + space;

                    // draw outline
                    if (valueStrokeWidth > 0f && valueStrokeColor != SKColor.Empty)
                    {
                        using (var strokePaint = new SKPaint
                        {
                            Style = SKPaintStyle.StrokeAndFill,
                            TextSize = textSize,
                            IsAntialias = true,
                            FakeBoldText = true,
                            Color = valueStrokeColor,
                            StrokeWidth = valueStrokeWidth,
                            IsStroke = true,
                            TextAlign = horizontalAlignment,
                        })
                            canvas.DrawText(text, point.X, y, strokePaint);
                    }

                    canvas.DrawText(text, point.X, y, paint);
                }
            }
        }

        /// <summary>
        /// Draws the given point.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="point">The point.</param>
        /// <param name="color">The fill color.</param>
        /// <param name="size">The point size.</param>
        /// <param name="mode">The point mode.</param>
        public static void DrawPoint(this SKCanvas canvas, SKPoint point, SKColor color, float size, PointMode mode)
        {
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = color,
            })
            {
                switch (mode)
                {
                    case PointMode.Square:
                        canvas.DrawRect(SKRect.Create(point.X - (size / 2), point.Y - (size / 2), size, size), paint);
                        break;

                    case PointMode.Circle:
                        paint.IsAntialias = true;
                        canvas.DrawCircle(point.X, point.Y, size / 2, paint);
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Draws a line with a gradient stroke.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="startPoint">The starting point.</param>
        /// <param name="startColor">The starting color.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="endColor">The end color.</param>
        /// <param name="size">The stroke size.</param>
        public static void DrawGradientLine(this SKCanvas canvas, SKPoint startPoint, SKColor startColor, SKPoint endPoint, SKColor endColor, float size)
        {
            using (var shader = SKShader.CreateLinearGradient(startPoint, endPoint, new[] { startColor, endColor }, null, SKShaderTileMode.Clamp))
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = size,
                    Shader = shader,
                    IsAntialias = true,
                })
                {
                    canvas.DrawLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, paint);
                }
            }
        }
    }
}
