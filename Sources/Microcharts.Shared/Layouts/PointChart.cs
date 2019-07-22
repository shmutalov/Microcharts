// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Microcharts
{
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
        protected float ValueRange => MaxValue - MinValue;

        /// <summary>
        /// If set, value labels will be drawn always just near actual chart points
        /// </summary>
        public bool IsValueLabelNearValuePoints { get; set; } = true;

        /// <summary>
        /// Chart orientation
        /// </summary>
        public ChartOrientation Orientation { get; set; } = ChartOrientation.Vertical;

        #endregion

        #region Methods

        /// <summary>
        /// Calculate of the y values start position
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
            var (valueLabelSizes, labelSizes) = MeasureLabelSizes();

            switch (Orientation)
            {
                case ChartOrientation.Horizontal:
                    DrawHorizontal(canvas, width, height, labelSizes, valueLabelSizes);
                    break;
                case ChartOrientation.Vertical:
                    DrawVertical(canvas, width, height, labelSizes, valueLabelSizes);
                    break;
            }
        }

        private void DrawHorizontal(SKCanvas canvas, int width, int height, SKRect[] labelSizes, SKRect[] valueLabelSizes)
        {
            var footerHeight = CalculateFooterHeight();
            var headerHeight = CalculateHeaderHeight(valueLabelSizes);
            var itemSize = CalculateItemSize(width, height, footerHeight, headerHeight);
            var origin = CalculateYOrigin(itemSize.Height, headerHeight);
            var points = CalculatePointPositions(itemSize, headerHeight);

            DrawPointAreas(canvas, points, origin);
            DrawPoints(canvas, points);
            DrawFooter(canvas, points, itemSize, height, footerHeight, labelSizes);

            DrawValueLabels(canvas, points, itemSize, valueLabelSizes);
        }

        private void DrawVertical(SKCanvas canvas, int width, int height, SKRect[] labelSizes, SKRect[] valueLabelSizes)
        {
            var footerHeight = CalculateFooterHeight();
            var headerHeight = CalculateHeaderHeight(valueLabelSizes);
            var itemSize = CalculateItemSize(width, height, footerHeight, headerHeight);
            var origin = CalculateYOrigin(itemSize.Height, headerHeight);
            var points = CalculatePointPositions(itemSize, headerHeight);

            DrawPointAreas(canvas, points, origin);
            DrawPoints(canvas, points);
            DrawFooter(canvas, points, itemSize, height, footerHeight, labelSizes);

            DrawValueLabels(canvas, points, itemSize, valueLabelSizes);
        }

        /// <summary>
        /// Calculate chart item size (per point bounds)
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
        /// <param name="headerHeight"></param>
        /// <returns></returns>
        protected SKPoint[] CalculatePointPositions(SKSize itemSize, float headerHeight)
        {
            var result = new SKPoint[Entries.Count];
            var halfBodyWidthPlusMargin = (itemSize.Width / 2f) + Margin;
            var bodyWidthPlusMargin = itemSize.Width + Margin;

            for (var i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];

                var x = halfBodyWidthPlusMargin + (i * bodyWidthPlusMargin);
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
        /// <param name="labelSizes"></param>
        protected void DrawFooter(SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight, SKRect[] labelSizes)
        {
            DrawLabels(canvas, points, itemSize, height, footerHeight, labelSizes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="height"></param>
        /// <param name="footerHeight"></param>
        /// <param name="labelSizes"></param>
        protected void DrawLabels(
            SKCanvas canvas,
            SKPoint[] points,
            SKSize itemSize,
            int height,
            float footerHeight,
            SKRect[] labelSizes)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];
                ref var bounds = ref labelSizes[i];

                if (bounds == SKRect.Empty)
                {
                    continue;
                }

                using (var paint = new SKPaint
                {
                    TextSize = LabelTextSize,
                    TextAlign = SKTextAlign.Center,
                    IsAntialias = true,
                    Color = entry.TextColor,
                    IsStroke = false
                })
                {
                    var posX = point.X;
                    var posY = height - (Margin + (LabelTextSize / 2f));

                    // draw outline
                    if (entry.LabelStrokeWidth > 0f && entry.LabelStrokeColor != SKColor.Empty)
                    {
                        using (var strokePaint = new SKPaint
                        {
                            Style = SKPaintStyle.StrokeAndFill,
                            StrokeWidth = entry.LabelStrokeWidth,
                            TextSize = LabelTextSize,
                            TextAlign = SKTextAlign.Center,
                            FakeBoldText = true,
                            IsAntialias = true,
                            Color = entry.LabelStrokeColor,
                            IsStroke = true
                        })
                            canvas.DrawText(entry.Label, posX, posY, strokePaint);
                    }

                    canvas.DrawText(entry.Label, posX, posY, paint);
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
            if (points.Length == 0 || PointMode == PointMode.None || PointSize == 0f)
                return;

            for (int i = 0; i < points.Length; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];
                canvas.DrawPoint(point, entry.Color, PointSize, PointMode);
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
            if (points.Length == 0 || PointAreaAlpha == 0)
            {
                return;
            }

            for (int i = 0; i < points.Length; i++)
            {
                var entry = Entries[i];
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

        /// <summary>
        /// Draw value labels
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="valueLabelSizes"></param>
        protected void DrawValueLabels(SKCanvas canvas, SKPoint[] points, SKSize itemSize, SKRect[] valueLabelSizes)
        {
            if (points.Length == 0)
            {
                return;
            }

            for (int i = 0; i < points.Length; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];

                ref var bounds = ref valueLabelSizes[i];
                if (bounds == SKRect.Empty)
                {
                    continue;
                }

                using (new SKAutoCanvasRestore(canvas))
                using (var paint = new SKPaint
                {
                    TextSize = LabelTextSize,
                    TextAlign = SKTextAlign.Center,
                    FakeBoldText = false,
                    IsAntialias = true,
                    Color = entry.Color,
                    IsStroke = false
                })
                {
                    var posX = point.X;
                    var posY = IsValueLabelNearValuePoints ? point.Y - 20f : Margin;

                    // draw outline
                    if (entry.ValueStrokeWidth > 0f && entry.ValueStrokeColor != SKColor.Empty)
                    {
                        using (var strokePaint = new SKPaint
                        {
                            Style = SKPaintStyle.StrokeAndFill,
                            StrokeWidth = entry.ValueStrokeWidth,
                            TextSize = LabelTextSize,
                            TextAlign = SKTextAlign.Center,
                            FakeBoldText = true,
                            IsAntialias = true,
                            Color = entry.ValueStrokeColor,
                            IsStroke = true
                        })
                            canvas.DrawText(entry.ValueLabel, posX, posY, strokePaint);
                    }

                    canvas.DrawText(entry.ValueLabel, posX, posY, paint);
                }
            }
        }

        /// <summary>
        /// Calculate chart footer height
        /// </summary>
        /// <returns></returns>
        protected float CalculateFooterHeight()
        {
            var result = Margin;

            if (Entries.Any(e => !string.IsNullOrEmpty(e.Label)))
            {
                result += LabelTextSize + Margin;
            }

            return result;
        }

        /// <summary>
        /// Calculate chart left panel width
        /// </summary>
        /// <returns></returns>
        protected float CalculateLeftPanelWidth(SKRect[] labelSizes)
        {
            return labelSizes.Max(x => x.Width) + Margin * 2f;
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
        /// Measure label and value label bounds
        /// </summary>
        /// <returns></returns>
        protected (SKRect[], SKRect[]) MeasureLabelSizes()
        {
            using (var paint = new SKPaint
            {
                TextSize = LabelTextSize
            })
            {
                var labelSizes = new SKRect[Entries.Count];
                var valueLabelSizes = new SKRect[Entries.Count];

                for (var i = 0; i < Entries.Count; i++)
                {
                    var entry = Entries[i];
                    if (!string.IsNullOrEmpty(entry.Label))
                    {
                        paint.MeasureText(entry.Label, ref labelSizes[i]);
                    }

                    if (!string.IsNullOrEmpty(entry.ValueLabel))
                    {
                        paint.MeasureText(entry.ValueLabel, ref valueLabelSizes[i]);
                    }
                }

                return (labelSizes, valueLabelSizes);
            }
        }

        #endregion
    }
}
