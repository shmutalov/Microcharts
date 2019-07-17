﻿// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/RadialGauge.png)
    /// 
    /// Radial gauge chart.
    /// </summary>
    public class RadialGaugeChart : Chart
    {
        #region Properties

        /// <summary>
        /// Gets or sets the size of each gauge. If negative, then its will be calculated from the available space.
        /// </summary>
        /// <value>The size of the line.</value>
        public float LineSize { get; set; } = -1;

        /// <summary>
        /// Gets or sets the gauge background area alpha.
        /// </summary>
        /// <value>The line area alpha.</value>
        public byte LineAreaAlpha { get; set; } = 52;

        /// <summary>
        /// Gets or sets the start angle.
        /// </summary>
        /// <value>The start angle.</value>
        public float StartAngle { get; set; } = -90;

        private float AbsoluteMinimum => Entries.Select(x => x.Value).Concat(new[] { MaxValue, MinValue, InternalMinValue ?? 0 }).Min(x => Math.Abs(x));

        private float AbsoluteMaximum => Entries.Select(x => x.Value).Concat(new[] { MaxValue, MinValue, InternalMinValue ?? 0 }).Max(x => Math.Abs(x));

        private float ValueRange => AbsoluteMaximum - AbsoluteMinimum;

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="entry"></param>
        /// <param name="radius"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="strokeWidth"></param>
        public void DrawGaugeArea(SKCanvas canvas, Entry entry, float radius, int cx, int cy, float strokeWidth)
        {
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                Color = entry.Color.WithAlpha(LineAreaAlpha),
                IsAntialias = true,
            })
            {
                canvas.DrawCircle(cx, cy, radius, paint);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="entry"></param>
        /// <param name="radius"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="strokeWidth"></param>
        public void DrawGauge(SKCanvas canvas, Entry entry, float radius, int cx, int cy, float strokeWidth)
        {
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                StrokeCap = SKStrokeCap.Round,
                Color = entry.Color,
                IsAntialias = true,
            })
            {
                using (SKPath path = new SKPath())
                {
                    var sweepAngle = 360 * (Math.Abs(entry.Value) - AbsoluteMinimum) / ValueRange;
                    path.AddArc(SKRect.Create(cx - radius, cy - radius, 2 * radius, 2 * radius), StartAngle, sweepAngle);
                    canvas.DrawPath(path, paint);
                }
            }
        }

        /// <inheritdoc />
        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            DrawCaption(canvas, width, height);

            var sumValue = Entries.Sum(x => Math.Abs(x.Value));
            var radius = (Math.Min(width, height) - (2 * Margin)) / 2;
            var cx = width / 2;
            var cy = height / 2;
            var lineWidth = (LineSize < 0) ? (radius / ((Entries.Count() + 1) * 2)) : LineSize;
            var radiusSpace = lineWidth * 2;

            for (int i = 0; i < Entries.Count(); i++)
            {
                var entry = Entries.ElementAt(i);
                var entryRadius = (i + 1) * radiusSpace;
                DrawGaugeArea(canvas, entry, entryRadius, cx, cy, lineWidth);
                DrawGauge(canvas, entry, entryRadius, cx, cy, lineWidth);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void DrawCaption(SKCanvas canvas, int width, int height)
        {
            var center = ValueRange / 2;
            var rightValues = new List<Entry>();
            var leftValues = new List<Entry>();

            foreach (var entry in Entries)
            {
                if (entry.AbsValue < center)
                {
                    rightValues.Add(entry);
                }
                else
                {
                    leftValues.Add(entry);
                }
            }

            leftValues.Reverse();

            DrawCaptionElements(canvas, width, height, rightValues, false);
            DrawCaptionElements(canvas, width, height, leftValues, true);
        }

        #endregion
    }
}