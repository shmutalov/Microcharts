// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Line.png)
    /// 
    /// Line chart.
    /// </summary>
    public class LineChart : PointChart
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public LineChart()
        {
            PointSize = 10;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the size of the line.
        /// </summary>
        /// <value>The size of the line.</value>
        public float LineSize { get; set; } = 3;

        /// <summary>
        /// Gets or sets the line mode.
        /// </summary>
        /// <value>The line mode.</value>
        public LineMode LineMode { get; set; } = LineMode.Spline;

        /// <summary>
        /// Gets or sets the alpha of the line area.
        /// </summary>
        /// <value>The line area alpha.</value>
        public byte LineAreaAlpha { get; set; } = 32;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var (labelSizes, valueLabelSizes) = MeasureLabelSizes();
            var footerHeight = CalculateFooterHeight();
            var headerHeight = CalculateHeaderHeight(valueLabelSizes);
            var itemSize = CalculateItemSize(width, height, footerHeight, headerHeight);
            var origin = CalculateYOrigin(itemSize.Height, headerHeight);
            var points = CalculatePointPositions(itemSize, headerHeight);

            DrawArea(canvas, points, itemSize, origin);
            DrawLine(canvas, points, itemSize);
            DrawPoints(canvas, points);
            DrawFooter(canvas, points, itemSize, height, footerHeight, labelSizes);
            DrawValueLabels(canvas, points, itemSize, valueLabelSizes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        protected void DrawLine(SKCanvas canvas, SKPoint[] points, SKSize itemSize)
        {
            if (points.Length < 2 || LineMode == LineMode.None)
            {
                return;
            }

            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White,
                StrokeWidth = LineSize,
                IsAntialias = true,
            })
            using (var shader = CreateGradient(points))
            {
                paint.Shader = shader;

                var path = new SKPath();

                path.MoveTo(points[0]);

                var last = (LineMode == LineMode.Spline) ? points.Length - 1 : points.Length;
                for (int i = 0; i < last; i++)
                {
                    if (LineMode == LineMode.Spline)
                    {
                        var entry = Entries[i];
                        var nextEntry = Entries[i + 1];
                        var cubicInfo = CalculateCubicInfo(points, i, itemSize);
                        path.CubicTo(cubicInfo.control, cubicInfo.nextControl, cubicInfo.nextPoint);
                    }
                    else if (LineMode == LineMode.Straight)
                    {
                        path.LineTo(points[i]);
                    }
                }

                canvas.DrawPath(path, paint);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="points"></param>
        /// <param name="itemSize"></param>
        /// <param name="origin"></param>
        protected void DrawArea(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float origin)
        {
            if (LineAreaAlpha == 0 || points.Length < 2)
            {
                return;
            }

            ref var firstPoint = ref points[0];
            ref var lastPoint = ref points[points.Length - 1];

            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.White,
                IsAntialias = true,
            })
            using (var shader = CreateGradient(points, LineAreaAlpha))
            {
                paint.Shader = shader;

                var path = new SKPath();
                path.MoveTo(firstPoint.X, origin);
                path.LineTo(firstPoint);

                var last = (LineMode == LineMode.Spline) ? points.Length - 1 : points.Length;
                for (int i = 0; i < last; i++)
                {
                    if (LineMode == LineMode.Spline)
                    {
                        var entry = Entries[i];
                        var nextEntry = Entries[i + 1];
                        var cubicInfo = CalculateCubicInfo(points, i, itemSize);
                        path.CubicTo(cubicInfo.control, cubicInfo.nextControl, cubicInfo.nextPoint);
                    }
                    else if (LineMode == LineMode.Straight)
                    {
                        path.LineTo(points[i]);
                    }
                }

                path.LineTo(lastPoint.X, origin);
                path.Close();

                canvas.DrawPath(path, paint);
            }
        }

        private (SKPoint point, SKPoint control, SKPoint nextPoint, SKPoint nextControl) CalculateCubicInfo(SKPoint[] points, int i, SKSize itemSize)
        {
            var point = points[i];
            var nextPoint = points[i + 1];
            var controlOffset = new SKPoint(itemSize.Width * 0.8f, 0);
            var currentControl = point + controlOffset;
            var nextControl = nextPoint - controlOffset;
            return (point, currentControl, nextPoint, nextControl);
        }

        private SKShader CreateGradient(SKPoint[] points, byte alpha = 255)
        {
            var startX = points[0].X;
            var endX = points[points.Length - 1].X;
            var rangeX = endX - startX;

            return SKShader.CreateLinearGradient(
                new SKPoint(startX, 0),
                new SKPoint(endX, 0),
                Entries.Select(x => x.Color.WithAlpha(alpha)).ToArray(),
                null,
                SKShaderTileMode.Clamp);
        }

        #endregion
    }
}