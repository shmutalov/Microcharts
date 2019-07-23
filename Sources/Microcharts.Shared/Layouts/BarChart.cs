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

        /// <summary>
        /// Draws the content of the chart onto the specified canvas.
        /// </summary>
        /// <param name="canvas">The output canvas.</param>
        /// <param name="width">The width of the chart.</param>
        /// <param name="height">The height of the chart.</param>
        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var (labelSizes, valueLabelSizes) = MeasureLabelSizes();
            var footerHeight = CalculateFooterHeight();
            var headerHeight = CalculateHeaderHeight(valueLabelSizes);
            var itemSize = CalculateItemSizeVertical(width, height, footerHeight, headerHeight);
            var origin = CalculateYOrigin(itemSize.Height, headerHeight);
            var points = CalculatePointPositionsVertical(itemSize, headerHeight);

            DrawBarAreas(canvas, points, itemSize, headerHeight);
            DrawBars(canvas, points, itemSize, origin, headerHeight);
            DrawPoints(canvas, points);
            DrawFooter(canvas, points, itemSize, height, footerHeight, labelSizes);
            DrawValueLabelsVertical(canvas, points, itemSize, valueLabelSizes);
        }

        /// <summary>
        /// Draws the value bars.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="points">The points.</param>
        /// <param name="bodySize">The item size.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="headerHeight">The Header height.</param>
        protected void DrawBars(SKCanvas canvas, SKPoint[] points, SKSize bodySize, float origin, float headerHeight)
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
                    var x = point.X - (bodySize.Width / 2);
                    var y = Math.Min(origin, point.Y);
                    var height = Math.Max(MinBarHeight, Math.Abs(origin - point.Y));
                    if (height < MinBarHeight)
                    {
                        height = MinBarHeight;
                        if (y + height > Margin + bodySize.Height)
                        {
                            y = headerHeight + bodySize.Height - height;
                        }
                    }

                    var rect = SKRect.Create(x, y, bodySize.Width, height);
                    canvas.DrawRect(rect, paint);
                }
            }
        }

        /// <summary>
        /// Draws the bar background areas.
        /// </summary>
        /// <param name="canvas">The output canvas.</param>
        /// <param name="points">The entry points.</param>
        /// <param name="itemSize">The item size.</param>
        /// <param name="headerHeight">The header height.</param>
        protected void DrawBarAreas(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float headerHeight)
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

        #endregion
    }
}
