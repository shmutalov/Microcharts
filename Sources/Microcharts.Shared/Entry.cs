// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
	using SkiaSharp;
    using System;

    /// <summary>
    /// A data entry for a chart.
    /// </summary>
    public class Entry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microcharts.Entry"/> class.
        /// </summary>
        /// <param name="value">The entry value.</param>
        public Entry(float value)
        {
            Value = value;
        }

        #endregion

        #region Properties

        private float _value;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public float Value
        {
            get => _value;
            set
            {
                _value = value;
                AbsValue = Math.Abs(value);
            }
        }

        /// <summary>
        /// Gets the absolute value
        /// </summary>
        public float AbsValue { get; private set; }

        /// <summary>
        /// Gets or sets the caption label.
        /// </summary>
        /// <value>The label.</value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the label associated to the value.
        /// </summary>
        /// <value>The value label.</value>
        public string ValueLabel { get; set; }

        /// <summary>
        /// Gets or sets the color of the fill.
        /// </summary>
        /// <value>The color of the fill.</value>
        public SKColor Color { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the color of the text (for the caption label).
        /// </summary>
        /// <value>The color of the text.</value>
        public SKColor TextColor { get; set; } = SKColors.Gray;

        /// <summary>
        /// Gets or sets the outline color of the text (for the caption label).
        /// </summary>
        /// <value>The outline color of the text.</value>
        public SKColor LabelStrokeColor { get; set; } = SKColors.Empty;

        /// <summary>
        /// Gets or sets the outline width of the text (for the caption label).
        /// </summary>
        /// <value>The outline width of the text.</value>
        public float LabelStrokeWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the outline color of the text (for the value).
        /// </summary>
        /// <value>The outline color of the text.</value>
        public SKColor ValueStrokeColor { get; set; } = SKColors.Empty;

        /// <summary>
        /// Gets or sets the outline width of the text (for the value).
        /// </summary>
        /// <value>The outline width of the text.</value>
        public float ValueStrokeWidth { get; set; } = 2f;

        #endregion
    }
}
