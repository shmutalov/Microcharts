// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Point.png)
    /// 
    /// Point chart.
    /// </summary>
    public class PointChart : Chart
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public float PointSize { get; set; } = 14;

        /// <summary>
        /// 
        /// </summary>
        public PointMode PointMode { get; set; } = PointMode.Circle;

        /// <summary>
        /// 
        /// </summary>
        public byte PointAreaAlpha { get; set; } = 100;

        /// <summary>
        /// 
        /// </summary>
        private float ValueRange => MaxValue - MinValue;

        /// <summary>
        /// 
        /// </summary>
        public bool IsValueAlwaysOnTop { get; set; } = false;

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemHeight"></param>
        /// <param name="headerHeight"></param>
        /// <returns></returns>
        public float CalculateYOrigin(float itemHeight, float headerHeight)
        {
            if (MaxValue <= 0)
            {
                return headerHeight;
            }

            if (MinValue > 0)
            {
                return headerHeight + itemHeight;
            }

            return headerHeight + ((MaxValue / ValueRange) * itemHeight);
        }

        /// <inheritdoc />
        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var valueLabelSizes = MeasureValueLabels();
            var footerHeight = CalculateFooterHeight(valueLabelSizes);
            var headerHeight = CalculateHeaderHeight(valueLabelSizes);
            var itemSize = CalculateItemSize(width, height, footerHeight, headerHeight);
            var origin = CalculateYOrigin(itemSize.Height, headerHeight);
            var points = CalculatePoints(itemSize, origin, headerHeight);

            DrawPointAreas(canvas, points, origin);
            DrawPoints(canvas, points);
            DrawFooter(canvas, points, itemSize, height, footerHeight);
            DrawValueLabel(canvas, points, itemSize, height, valueLabelSizes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="footerHeight"></param>
        /// <param name="headerHeight"></param>
        /// <returns></returns>
        protected SKSize CalculateItemSize(int width, int height, float footerHeight, float headerHeight)
        {
            var w = (width - ((Entries.Count + 1) * Margin)) / Entries.Count;
            var h = height - Margin - footerHeight - headerHeight;
            return new SKSize(w, h);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemSize"></param>
        /// <param name="origin"></param>
        /// <param name="headerHeight"></param>
        /// <returns></returns>
        protected SKPoint[] CalculatePoints(SKSize itemSize, float origin, float headerHeight)
        {
            var result = new SKPoint[Entries.Count];

            for (var i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];

                var x = Margin + (itemSize.Width / 2) + (i * (itemSize.Width + Margin));
                var y = headerHeight + (((MaxValue - entry.Value) / ValueRange) * itemSize.Height);

                result[i] = new SKPoint(x, y);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="height"></param>
        /// <param name="footerHeight"></param>
        protected void DrawFooter(SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight)
        {
            DrawLabels(canvas, points, itemSize, height, footerHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="height"></param>
        /// <param name="footerHeight"></param>
        protected void DrawLabels(SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];

                if (string.IsNullOrEmpty(entry.Label))
                {
                    continue;
                }

                using (var paint = new SKPaint
                {
                    TextSize = LabelTextSize,
                    IsAntialias = true,
                    Color = entry.TextColor,
                    IsStroke = false
                })
                {
                    var bounds = new SKRect();
                    var text = entry.Label;
                    paint.MeasureText(text, ref bounds);

                    if (bounds.Width > itemSize.Width)
                    {
                        text = text.Substring(0, Math.Min(3, text.Length));
                        paint.MeasureText(text, ref bounds);
                    }

                    if (bounds.Width > itemSize.Width)
                    {
                        text = text.Substring(0, Math.Min(1, text.Length));
                        paint.MeasureText(text, ref bounds);
                    }

                    var posX = point.X - (bounds.Width / 2);
                    var posY = height - (Margin + (LabelTextSize / 2));

                    // draw outline
                    if (entry.LabelStrokeWidth > 0f && entry.LabelStrokeColor != SKColor.Empty)
                    {
                        using (var strokePaint = new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = entry.LabelStrokeWidth,
                            TextSize = LabelTextSize,
                            FakeBoldText = true,
                            IsAntialias = true,
                            Color = entry.LabelStrokeColor,
                            IsStroke = true
                        })
                            canvas.DrawText(text, posX, posY, strokePaint);
                    }
                    
                    canvas.DrawText(text, posX, posY, paint);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        protected void DrawPoints(SKCanvas canvas, SKPoint[] points)
        {
            if (points.Length > 0 && PointMode != PointMode.None)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    var entry = Entries[i];
                    ref var point = ref points[i];
                    canvas.DrawPoint(point, entry.Color, PointSize, PointMode);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="origin"></param>
        protected void DrawPointAreas(SKCanvas canvas, SKPoint[] points, float origin)
        {
            if (points.Length > 0 && PointAreaAlpha > 0)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    var entry = Entries.ElementAt(i);
                    ref var point = ref points[i];
                    var y = Math.Min(origin, point.Y);

                    using (var shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, origin),
                        new SKPoint(0, point.Y),
                        new[]
                        {
                            entry.Color.WithAlpha(PointAreaAlpha),
                            entry.Color.WithAlpha((byte)(PointAreaAlpha / 3))
                        },
                        null,
                        SKShaderTileMode.Clamp))
                    using (var paint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = entry.Color.WithAlpha(PointAreaAlpha),
                    })
                    {
                        paint.Shader = shader;
                        var height = Math.Max(2, Math.Abs(origin - point.Y));
                        canvas.DrawRect(SKRect.Create(point.X - (PointSize / 2), y, PointSize, height), paint);
                    }
                }
            }
        }

        /// <summary>
        /// Draw value labels
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="height"></param>
        /// <param name="valueLabelSizes"></param>
        protected void DrawValueLabel_(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float height, SKRect[] valueLabelSizes)
        {
            if (points.Length == 0)
            {
                return;
            }

            for (int i = 0; i < points.Length; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];

                if (string.IsNullOrEmpty(entry.ValueLabel))
                {
                    continue;
                }

                using (new SKAutoCanvasRestore(canvas))
                using (var paint = new SKPaint
                {
                    TextSize = LabelTextSize,
                    FakeBoldText = false,
                    IsAntialias = true,
                    Color = entry.Color,
                    IsStroke = false
                })
                {
                    var bounds = new SKRect();
                    var text = entry.ValueLabel;
                    paint.MeasureText(text, ref bounds);

                    canvas.RotateDegrees(90);
                    canvas.Translate(Margin, -point.X + (bounds.Height / 2));

                    // draw outline
                    if (entry.ValueStrokeWidth > 0f && entry.ValueStrokeColor != SKColor.Empty)
                    {
                        using (var strokePaint = new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = entry.ValueStrokeWidth,
                            TextSize = LabelTextSize,
                            FakeBoldText = true,
                            IsAntialias = true,
                            Color = entry.ValueStrokeColor,
                            IsStroke = true
                        })
                            canvas.DrawText(text, 0, 0, strokePaint);
                    }

                    canvas.DrawText(text, 0, 0, paint);
                }
            }
        }

        /// <summary>
        /// Draw value labels
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="height"></param>
        /// <param name="valueLabelSizes"></param>
        protected void DrawValueLabel(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float height, SKRect[] valueLabelSizes)
        {
            if (points.Length == 0)
            {
                return;
            }

            for (int i = 0; i < points.Length; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];

                if (string.IsNullOrEmpty(entry.ValueLabel))
                {
                    continue;
                }

                using (new SKAutoCanvasRestore(canvas))
                using (var paint = new SKPaint
                {
                    TextSize = LabelTextSize,
                    FakeBoldText = false,
                    IsAntialias = true,
                    Color = entry.Color,
                    IsStroke = false
                })
                {
                    var bounds = new SKRect();
                    var text = entry.ValueLabel;
                    paint.MeasureText(text, ref bounds);

                    if (bounds.Width > itemSize.Width)
                    {
                        text = text.Substring(0, Math.Min(3, text.Length));
                        paint.MeasureText(text, ref bounds);
                    }

                    if (bounds.Width > itemSize.Width)
                    {
                        text = text.Substring(0, Math.Min(1, text.Length));
                        paint.MeasureText(text, ref bounds);
                    }

                    var posX = point.X - (bounds.Width / 2);
                    //var posY = height - (Margin + (LabelTextSize / 2));
                    var posY = IsValueAlwaysOnTop ? Margin : point.Y - 20f;

                    // draw outline
                    if (entry.ValueStrokeWidth > 0f && entry.ValueStrokeColor != SKColor.Empty)
                    {
                        using (var strokePaint = new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = entry.ValueStrokeWidth,
                            TextSize = LabelTextSize,
                            FakeBoldText = true,
                            IsAntialias = true,
                            Color = entry.ValueStrokeColor,
                            IsStroke = true
                        })
                            canvas.DrawText(text, posX, posY, strokePaint);
                    }

                    canvas.DrawText(text, posX, posY, paint);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueLabelSizes"></param>
        /// <returns></returns>
        protected float CalculateFooterHeight(SKRect[] valueLabelSizes)
        {
            var result = Margin;

            if (Entries.Any(e => !string.IsNullOrEmpty(e.Label)))
            {
                result += LabelTextSize + Margin;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueLabelSizes"></param>
        /// <returns></returns>
        protected float CalculateHeaderHeight(SKRect[] valueLabelSizes)
        {
            if (Entries.Count == 0)
            {
                return Margin;
            }

            var result = Margin;
            var maxValueWidth = valueLabelSizes.Max(x => x.Width);
            if (maxValueWidth > 0)
            {
                result += maxValueWidth + Margin;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected SKRect[] MeasureValueLabels()
        {
            using (var paint = new SKPaint
            {
                TextSize = LabelTextSize
            })
            {
                return Entries.Select(e =>
                {
                    if (string.IsNullOrEmpty(e.ValueLabel))
                    {
                        return SKRect.Empty;
                    }

                    var bounds = new SKRect();
                    paint.MeasureText(e.ValueLabel, ref bounds);
                    return bounds;
                }).ToArray();
            }
        }

        #endregion
    }
}
