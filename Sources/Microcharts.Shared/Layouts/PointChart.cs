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

        /// <summary>
        /// Calculate of the x values start position
        /// </summary>
        /// <param name="itemWidth"></param>
        /// <param name="leftPanelWidth"></param>
        /// <returns></returns>
        public float CalculateXOrigin(float itemWidth, float leftPanelWidth)
        {
            if (MaxValue <= 0)
            {
                return leftPanelWidth + itemWidth;
            }

            return leftPanelWidth + Margin;
        }

        /// <inheritdoc />
        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var (labelSizes, valueLabelSizes) = MeasureLabelSizes();

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
            var leftPanelWidth = CalculateLeftPanelWidth(labelSizes);
            var rigthPanelWidth = CalculateRightPanelWidth(valueLabelSizes);

            var itemSize = CalculateItemSizeHorizontal(width, height, leftPanelWidth, rigthPanelWidth);
            var origin = CalculateXOrigin(itemSize.Height, leftPanelWidth);
            var points = CalculatePointPositionsHorizontal(itemSize, leftPanelWidth);

            DrawPointAreasHorizontal(canvas, points, origin);
            DrawPoints(canvas, points);
            DrawLeftPanel(canvas, points, itemSize, leftPanelWidth, labelSizes);

            DrawValueLabelsHorizontal(canvas, points, itemSize, valueLabelSizes, width, rigthPanelWidth);
        }

        private void DrawVertical(SKCanvas canvas, int width, int height, SKRect[] labelSizes, SKRect[] valueLabelSizes)
        {
            var footerHeight = CalculateFooterHeight();
            var headerHeight = CalculateHeaderHeight(valueLabelSizes);
            var itemSize = CalculateItemSizeVertical(width, height, footerHeight, headerHeight);
            var origin = CalculateYOrigin(itemSize.Height, headerHeight);
            var points = CalculatePointPositionsVertical(itemSize, headerHeight);

            DrawPointAreasVertical(canvas, points, origin);
            DrawPoints(canvas, points);
            DrawFooter(canvas, points, itemSize, height, footerHeight, labelSizes);

            DrawValueLabelsVertical(canvas, points, itemSize, valueLabelSizes);
        }

        /// <summary>
        /// Calculate chart item size (per point bounds)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="footerHeight"></param>
        /// <param name="headerHeight"></param>
        /// <returns></returns>
        protected SKSize CalculateItemSizeVertical(int width, int height, float footerHeight, float headerHeight)
        {
            var w = (width - ((Entries.Count + 1) * Margin)) / Entries.Count;
            var h = height - Margin - footerHeight - headerHeight;
            return new SKSize(w, h);
        }

        /// <summary>
        /// Calculate chart item size (per point bounds)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="leftPanelWidth"></param>
        /// <param name="rightPanelWidth"></param>
        /// <returns></returns>
        protected SKSize CalculateItemSizeHorizontal(int width, int height, float leftPanelWidth, float rightPanelWidth)
        {
            var w = width - leftPanelWidth - rightPanelWidth;
            var h = (height - ((Entries.Count + 1) * Margin)) / Entries.Count;
            
            return new SKSize(w, h);
        }

        /// <summary>
        /// Calculate point positions in canvas (vertical)
        /// </summary>
        /// <param name="itemSize"></param>
        /// <param name="headerHeight"></param>
        /// <returns></returns>
        protected SKPoint[] CalculatePointPositionsVertical(SKSize itemSize, float headerHeight)
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
        /// Calculate point positions in canvas (horizontal)
        /// </summary>
        /// <param name="itemSize"></param>
        /// <param name="leftPanelWidth"></param>
        /// <returns></returns>
        protected SKPoint[] CalculatePointPositionsHorizontal(SKSize itemSize, float leftPanelWidth)
        {
            var result = new SKPoint[Entries.Count];
            var halfBodyHeightPlusMargin = (itemSize.Height / 2f) + Margin;
            var bodyHeightPlusMargin = itemSize.Height + Margin;

            for (var i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];

                var y = halfBodyHeightPlusMargin + (i * bodyHeightPlusMargin);
                var x = leftPanelWidth + (((entry.Value - MinValue) / ValueRange) * itemSize.Width);

                result[i] = new SKPoint(x, y);
            }

            return result;
        }

        /// <summary>
        /// Draw footer (labels)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="height"></param>
        /// <param name="footerHeight"></param>
        /// <param name="labelSizes"></param>
        protected void DrawFooter(SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight, SKRect[] labelSizes)
        {
            DrawLabelsVertical(canvas, points, itemSize, height, footerHeight, labelSizes);
        }

        /// <summary>
        /// Draw left panel (labels)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="width"></param>
        /// <param name="leftPanelWidth"></param>
        /// <param name="labelSizes"></param>
        protected void DrawLeftPanel(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float leftPanelWidth, SKRect[] labelSizes)
        {
            DrawLabelsHorizontal(canvas, points, itemSize, leftPanelWidth, labelSizes);
        }

        /// <summary>
        /// Draw labels (vertical)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="height"></param>
        /// <param name="footerHeight"></param>
        /// <param name="labelSizes"></param>
        protected void DrawLabelsVertical(
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
        /// Draw labels (horizontal)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="width"></param>
        /// <param name="leftPanelWidth"></param>
        /// <param name="labelSizes"></param>
        protected void DrawLabelsHorizontal(
            SKCanvas canvas,
            SKPoint[] points,
            SKSize itemSize,
            float leftPanelWidth,
            SKRect[] labelSizes)
        {
            for (var i = 0; i < Entries.Count; i++)
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
                    TextAlign = SKTextAlign.Right,
                    IsAntialias = true,
                    Color = entry.TextColor,
                    IsStroke = false
                })
                {
                    var posX = leftPanelWidth;
                    var posY = point.Y + bounds.Height / 2f;

                    // draw outline
                    if (entry.LabelStrokeWidth > 0f && entry.LabelStrokeColor != SKColor.Empty)
                    {
                        using (var strokePaint = new SKPaint
                        {
                            Style = SKPaintStyle.StrokeAndFill,
                            StrokeWidth = entry.LabelStrokeWidth,
                            TextSize = LabelTextSize,
                            TextAlign = SKTextAlign.Right,
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
        /// Draw points to the canvas
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
        /// Draw points background areas (vertical)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="yOrigin"></param>
        protected void DrawPointAreasVertical(SKCanvas canvas, SKPoint[] points, float yOrigin)
        {
            if (points.Length == 0 || PointAreaAlpha == 0)
            {
                return;
            }

            for (var i = 0; i < points.Length; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];
                var y = Math.Min(yOrigin, point.Y);

                using (var shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, yOrigin),
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
                    var height = Math.Max(2, Math.Abs(yOrigin - point.Y));
                    canvas.DrawRect(SKRect.Create(point.X - (PointSize / 2), y, PointSize, height), paint);
                }
            }
        }

        /// <summary>
        /// Draw points background areas (horizontal)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="xOrigin"></param>
        protected void DrawPointAreasHorizontal(SKCanvas canvas, SKPoint[] points, float xOrigin)
        {
            if (points.Length == 0 || PointAreaAlpha == 0)
            {
                return;
            }

            for (int i = 0; i < points.Length; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];
                var x = Math.Min(xOrigin, point.X);

                using (var shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, xOrigin),
                    new SKPoint(0, point.X),
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
                    var width = Math.Max(2f, Math.Abs(xOrigin - point.X));
                    canvas.DrawRect(SKRect.Create(x, point.Y - (PointSize / 2f), width, PointSize), paint);
                }
            }
        }

        /// <summary>
        /// Draw value labels (vertical)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="valueLabelSizes"></param>
        protected void DrawValueLabelsVertical(SKCanvas canvas, SKPoint[] points, SKSize itemSize, SKRect[] valueLabelSizes)
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
                    var posY = IsValueLabelNearValuePoints ? point.Y - PointSize : Margin;

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
        /// Draw value labels (horizontal)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="valueLabelSizes"></param>
        /// <param name="width"></param>
        /// <param name="rightPanelWidth"></param>
        protected void DrawValueLabelsHorizontal(
            SKCanvas canvas, 
            SKPoint[] points, 
            SKSize itemSize, 
            SKRect[] valueLabelSizes, 
            int width,
            float rightPanelWidth)
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
                    TextAlign = SKTextAlign.Left,
                    FakeBoldText = false,
                    IsAntialias = true,
                    Color = entry.Color,
                    IsStroke = false
                })
                {
                    var posX = IsValueLabelNearValuePoints ? point.X + PointSize + PointSize : width - rightPanelWidth + Margin;
                    var posY = point.Y + bounds.Height / 2f;

                    // draw outline
                    if (entry.ValueStrokeWidth > 0f && entry.ValueStrokeColor != SKColor.Empty)
                    {
                        using (var strokePaint = new SKPaint
                        {
                            Style = SKPaintStyle.StrokeAndFill,
                            StrokeWidth = entry.ValueStrokeWidth,
                            TextSize = LabelTextSize,
                            TextAlign = SKTextAlign.Left,
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
            return labelSizes.Max(x => x.Width) + Margin + Margin;
        }

        /// <summary>
        /// Calculate chart right panel width
        /// </summary>
        /// <returns></returns>
        protected float CalculateRightPanelWidth(SKRect[] valueLabelSizes)
        {
            return valueLabelSizes.Max(x => x.Width) + Margin + Margin;
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
