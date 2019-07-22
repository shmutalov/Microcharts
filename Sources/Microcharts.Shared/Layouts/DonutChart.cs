// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Donut.png)
    /// 
    /// A donut chart.
    /// </summary>
    public class DonutChart : Chart
    {
        #region Properties

        /// <summary>
        /// Gets or sets the radius of the hole in the center of the chart.
        /// </summary>
        /// <value>The hole radius.</value>
        public float HoleRadius { get; set; } = 0;

        private List<Entry> _entries;
        private float _sum;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            _entries = Entries.ToList();
            _sum = _entries.Sum(x => x.AbsValue);

            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.Translate(width / 2f, height / 2f);
                
                var radius = (Math.Min(width, height) - (2f * Margin)) / 2f;

                var start = 0.0f;
                foreach (var entry in _entries)
                {
                    var end = start + (entry.AbsValue / _sum);

                    // Sector
                    var path = RadialHelpers.CreateSectorPath(start, end, radius, radius * HoleRadius);
                    using (var paint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = entry.Color,
                        IsAntialias = true,
                    })
                    {
                        canvas.DrawPath(path, paint);
                    }

                    start = end;
                }
            }

            DrawCaption(canvas, width, height);
        }

        /// <summary>
        /// Draw chart legend
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void DrawCaption(SKCanvas canvas, int width, int height)
        {
            var rightValues = new List<Entry>();
            var leftValues = new List<Entry>();

            for (var i = 0; i < _entries.Count / 2; i++)
            {
                rightValues.Add(_entries[i]);
            }

            for (var i = _entries.Count - 1; i >= _entries.Count / 2; i--)
            {
                leftValues.Add(_entries[i]);
            }

            DrawCaptionElementsHorizontal(canvas, width, height, rightValues, false);
            DrawCaptionElementsHorizontal(canvas, width, height, leftValues, true);
        }

        #endregion
    }
}