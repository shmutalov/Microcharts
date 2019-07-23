// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using System.Linq;

    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Bar.png)
    /// 
    /// A bar chart.
    /// </summary>
    public class BarChart : PointChart
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public BarChart()
        {
            PointSize = 14f;
            PointMode = PointMode.None;
            IsValueLabelNearValuePoints = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the bar background area alpha.
        /// </summary>
        /// <value>The bar area alpha.</value>
        public byte BarAreaAlpha { get; set; } = 32;

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void DrawHorizontal(SKCanvas canvas, int width, int height, SKRect[] labelSizes, SKRect[] valueLabelSizes)
        {
            var leftPanelWidth = CalculateLeftPanelWidth(labelSizes);
            var rigthPanelWidth = CalculateRightPanelWidth(valueLabelSizes);

            var itemSize = CalculateItemSizeHorizontal(width, height, leftPanelWidth, rigthPanelWidth);
            var origin = CalculateXOrigin(itemSize.Width, leftPanelWidth);
            var points = CalculatePointPositionsHorizontal(itemSize, leftPanelWidth);

            DrawOriginLineHorizontal(canvas, height, origin);

            DrawBarAreasHorizontal(canvas, points, itemSize, leftPanelWidth);
            DrawBarsHorizontal(canvas, points, itemSize, origin, leftPanelWidth);

            DrawPoints(canvas, points);
            DrawLeftPanel(canvas, points, itemSize, leftPanelWidth, labelSizes);

            DrawValueLabelsHorizontal(canvas, points, itemSize, valueLabelSizes, width, rigthPanelWidth);
        }


        /// <inheritdoc />
        protected override void DrawVertical(SKCanvas canvas, int width, int height, SKRect[] labelSizes, SKRect[] valueLabelSizes)
        {
            var footerHeight = CalculateFooterHeight();
            var headerHeight = CalculateHeaderHeight(valueLabelSizes);
            var itemSize = CalculateItemSizeVertical(width, height, footerHeight, headerHeight);
            var origin = CalculateYOrigin(itemSize.Height, headerHeight);
            var points = CalculatePointPositionsVertical(itemSize, headerHeight);

            DrawOriginLineVertical(canvas, width, origin);

            DrawBarAreasVertical(canvas, points, itemSize, headerHeight);
            DrawBarsVertical(canvas, points, itemSize, origin, headerHeight);

            DrawPoints(canvas, points);
            DrawFooter(canvas, points, itemSize, height, footerHeight, labelSizes);

            DrawValueLabelsVertical(canvas, points, itemSize, valueLabelSizes);
        }

        /// <summary>
        /// Draws the value bars (vertical)
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="points">The points.</param>
        /// <param name="itemSize">The item size.</param>
        /// <param name="yOrigin">The origin.</param>
        /// <param name="headerHeight">The Header height.</param>
        protected void DrawBarsVertical(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float yOrigin, float headerHeight)
        {
            const float MinBarHeight = 4;
            if (points.Length == 0)
            {
                return;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];

                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = entry.Color,
                })
                {
                    var x = point.X - (itemSize.Width / 2);
                    var y = Math.Min(yOrigin, point.Y);
                    var height = Math.Max(MinBarHeight, Math.Abs(yOrigin - point.Y));
                    if (height < MinBarHeight)
                    {
                        height = MinBarHeight;
                        if (y + height > Margin + itemSize.Height)
                        {
                            y = headerHeight + itemSize.Height - height;
                        }
                    }

                    var rect = SKRect.Create(x, y, itemSize.Width, height);
                    canvas.DrawRect(rect, paint);
                }
            }
        }

        /// <summary>
        /// Draws the bar background areas (vertical)
        /// </summary>
        /// <param name="canvas">The output canvas.</param>
        /// <param name="points">The entry points.</param>
        /// <param name="itemSize">The item size.</param>
        /// <param name="headerHeight">The header height.</param>
        protected void DrawBarAreasVertical(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float headerHeight)
        {
            if (points.Length == 0 || PointAreaAlpha == 0)
            {
                return;
            }

            for (int i = 0; i < points.Length; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];

                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = entry.Color.WithAlpha(BarAreaAlpha),
                })
                {
                    var max = entry.Value > 0 ? headerHeight : headerHeight + itemSize.Height;
                    var height = Math.Abs(max - point.Y);
                    var y = Math.Min(max, point.Y);
                    canvas.DrawRect(SKRect.Create(point.X - (itemSize.Width / 2), y, itemSize.Width, height), paint);
                }
            }
        }

        /// <summary>
        /// Draws the value bars (vertical)
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="points">The points.</param>
        /// <param name="itemSize">The item size.</param>
        /// <param name="xOrigin">The origin.</param>
        /// <param name="leftPanelWidth">The Header height.</param>
        protected void DrawBarsHorizontal(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float xOrigin, float leftPanelWidth)
        {
            const float MinBarWidth = 4f;
            if (points.Length == 0)
            {
                return;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];

                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = entry.Color,
                })
                {
                    var x = Math.Min(xOrigin, point.X);
                    var y = point.Y - (itemSize.Height / 2f);

                    var width = Math.Max(MinBarWidth, Math.Abs(xOrigin - point.X));
                    if (width < MinBarWidth)
                    {
                        width = MinBarWidth;
                        if (x + width > Margin + itemSize.Width)
                        {
                            x = leftPanelWidth + itemSize.Width - width;
                        }
                    }

                    var rect = SKRect.Create(x, y, width, itemSize.Height);
                    canvas.DrawRect(rect, paint);
                }
            }
        }

        /// <summary>
        /// Draws the bar background areas (vertical)
        /// </summary>
        /// <param name="canvas">The output canvas.</param>
        /// <param name="points">The entry points.</param>
        /// <param name="itemSize">The item size.</param>
        /// <param name="leftPanelWidth">The header height.</param>
        protected void DrawBarAreasHorizontal(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float leftPanelWidth)
        {
            if (points.Length == 0 || PointAreaAlpha == 0)
            {
                return;
            }

            for (var i = 0; i < points.Length; i++)
            {
                var entry = Entries[i];
                ref var point = ref points[i];

                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = entry.Color.WithAlpha(BarAreaAlpha),
                })
                {
                    var max = entry.Value > 0 
                        ? leftPanelWidth + PointSize + itemSize.Width
                        : leftPanelWidth + PointSize;

                    var width = Math.Abs(max - point.X);
                    var x = Math.Min(max, point.X);
                    canvas.DrawRect(SKRect.Create(x, point.Y - (itemSize.Height / 2f), width, itemSize.Height), paint);
                }
            }
        }

        #endregion
    }
}
