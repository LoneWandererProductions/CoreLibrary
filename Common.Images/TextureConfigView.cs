/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        TextureConfigView.cs
 * PURPOSE:     The view for Texture Configuration
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Imaging;
using Imaging.Enums;
using ViewModel;

namespace Common.Images
{
    /// <inheritdoc />
    /// <summary>
    ///     Main View for texture Configuration
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public sealed class TextureConfigView : ViewModelBase
    {
        /// <summary>
        ///     The alpha
        /// </summary>
        private int _alpha;

        /// <summary>
        ///     The angle1
        /// </summary>
        private double _anglePrimary;

        /// <summary>
        ///     The angle2
        /// </summary>
        private double _angleSecondary;

        /// <summary>
        ///     The base color
        /// </summary>
        private Color _baseColor;

        /// <summary>
        ///     The size of the cell (Cellular texture).
        /// </summary>
        private int _cellSize;

        /// <summary>
        ///     The center color (Cellular texture).
        /// </summary>
        private Color _centerColor;

        /// <summary>
        ///     The parsed color ramp (ColorMapped texture) that round-trips to
        ///     <see cref="TextureConfiguration.ColorRamp" />. The editable surface is
        ///     <see cref="_colorRampText" />; this is kept in sync with it.
        /// </summary>
        private Color[]? _colorRamp;

        /// <summary>
        ///     The text form of <see cref="_colorRamp" /> shown in the window; see
        ///     <see cref="ColorRampText" />.
        /// </summary>
        private string? _colorRampText;

        /// <summary>
        ///     The edge color (Cellular texture).
        /// </summary>
        private Color _edgeColor;

        /// <summary>
        ///     The edge jaggedness limit
        /// </summary>
        private int _edgeJaggednessLimit;

        /// <summary>
        ///     The is monochrome
        /// </summary>
        private bool _isMonochrome;

        /// <summary>
        ///     The is tiled
        /// </summary>
        private bool _isTiled;

        /// <summary>
        ///     The jaggedness threshold
        /// </summary>
        private int _jaggednessThreshold;

        /// <summary>
        ///     The line color
        /// </summary>
        private Color _lineColor;

        /// <summary>
        ///     The line spacing
        /// </summary>
        private int _lineSpacing;

        /// <summary>
        ///     The line thickness
        /// </summary>
        private int _lineThickness;

        /// <summary>
        ///     The maximum value
        /// </summary>
        private int _maxValue;

        /// <summary>
        ///     The minimum value
        /// </summary>
        private int _minValue;

        /// <summary>
        ///     The randomization factor
        /// </summary>
        private double _randomizationFactor;

        /// <summary>
        ///     The reset command
        /// </summary>
        private ICommand? _resetCommand;

        /// <summary>
        ///     The save command
        /// </summary>
        private ICommand? _saveCommand;

        /// <summary>
        ///     The secondary color (currently unused by generation — see remarks on
        ///     <see cref="SecondaryColor" />).
        /// </summary>
        private Color _secondaryColor;

        /// <summary>
        ///     The selected texture
        /// </summary>
        private TextureType _selectedTexture;

        /// <summary>
        ///     The turbulence power
        /// </summary>
        private double _turbulencePower;

        /// <summary>
        ///     The turbulence size
        /// </summary>
        private double _turbulenceSize;

        /// <summary>
        ///     The use smooth noise
        /// </summary>
        private bool _useSmoothNoise;

        /// <summary>
        ///     The use turbulence
        /// </summary>
        private bool _useTurbulence;

        /// <summary>
        ///     The wave amplitude
        /// </summary>
        private double _waveAmplitude;

        /// <summary>
        ///     The wave frequency
        /// </summary>
        private double _waveFrequency;

        /// <summary>
        ///     The x period
        /// </summary>
        private double _xPeriod;

        /// <summary>
        ///     The xy period
        /// </summary>
        private double _xyPeriod;

        /// <summary>
        ///     The y period
        /// </summary>
        private double _yPeriod;

        /// <summary>
        /// The configuration folder path
        /// </summary>
        private readonly string _configFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");

        /// <summary>
        /// The configuration file path
        /// </summary>
        private readonly string _configFilePath;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextureConfigView" /> class.
        /// </summary>
        public TextureConfigView()
        {
            _configFilePath = Path.Combine(_configFolderPath, "TextureSettings.json");
            LoadSettingsFromFile();

            // Pull the specific settings for the current SelectedTexture instead of hard defaults
            var currentConfig = ImagingFacade.GetTextureSettings(SelectedTexture) ?? new TextureConfiguration();
            LoadFromConfig(currentConfig);

            UpdateActiveProperties();
        }

        /// <summary>
        ///     Gets the filter options.
        /// </summary>
        /// <value>
        ///     The filter options.
        /// </value>
        public IEnumerable<TextureType>? TextureOptions =>
            Enum.GetValues(typeof(TextureType)) as IEnumerable<TextureType>;

        /// <summary>
        ///     Gets or sets the selected texture.
        /// </summary>
        /// <value>
        ///     The selected texture.
        /// </value>
        public TextureType SelectedTexture
        {
            get => _selectedTexture;
            set
            {
                if (!SetProperty(ref _selectedTexture, value, nameof(SelectedTexture))) return;

                // Sync values from Facade for the new selection
                var config = ImagingFacade.GetTextureSettings(value);
                if (config != null) LoadFromConfig(config);

                UpdateActiveProperties();
            }
        }

        /// <summary>
        ///     Gets or sets the minimum value.
        /// </summary>
        /// <value>
        ///     The minimum value.
        /// </value>
        public int MinValue
        {
            get => _minValue;
            set => SetProperty(ref _minValue, value, nameof(MinValue));
        }

        /// <summary>
        ///     Gets or sets the maximum value.
        /// </summary>
        /// <value>
        ///     The maximum value.
        /// </value>
        public int MaxValue
        {
            get => _maxValue;
            set => SetProperty(ref _maxValue, value, nameof(MaxValue));
        }

        /// <summary>
        ///     Gets or sets the alpha.
        /// </summary>
        /// <value>
        ///     The alpha.
        /// </value>
        public int Alpha
        {
            get => _alpha;
            set => SetProperty(ref _alpha, value, nameof(Alpha));
        }

        /// <summary>
        ///     Gets or sets the x period.
        /// </summary>
        /// <value>
        ///     The x period.
        /// </value>
        public double XPeriod
        {
            get => _xPeriod;
            set => SetProperty(ref _xPeriod, value, nameof(XPeriod));
        }

        /// <summary>
        ///     Gets or sets the y period.
        /// </summary>
        /// <value>
        ///     The y period.
        /// </value>
        public double YPeriod
        {
            get => _yPeriod;
            set => SetProperty(ref _yPeriod, value, nameof(YPeriod));
        }

        /// <summary>
        ///     Gets or sets the turbulence power.
        /// </summary>
        /// <value>
        ///     The turbulence power.
        /// </value>
        public double TurbulencePower
        {
            get => _turbulencePower;
            set => SetProperty(ref _turbulencePower, value, nameof(TurbulencePower));
        }

        /// <summary>
        ///     Gets or sets the size of the turbulence.
        /// </summary>
        /// <value>
        ///     The size of the turbulence.
        /// </value>
        public double TurbulenceSize
        {
            get => _turbulenceSize;
            set => SetProperty(ref _turbulenceSize, value, nameof(TurbulenceSize));
        }

        /// <summary>
        ///     Gets or sets the color of the base.
        /// </summary>
        /// <value>
        ///     The color of the base.
        /// </value>
        public Color BaseColor
        {
            get => _baseColor;
            set => SetProperty(ref _baseColor, value, nameof(BaseColor));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is monochrome.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is monochrome; otherwise, <c>false</c>.
        /// </value>
        public bool IsMonochrome
        {
            get => _isMonochrome;
            set => SetProperty(ref _isMonochrome, value, nameof(IsMonochrome));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is tiled.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is tiled; otherwise, <c>false</c>.
        /// </value>
        public bool IsTiled
        {
            get => _isTiled;
            set => SetProperty(ref _isTiled, value, nameof(IsTiled));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [use smooth noise].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [use smooth noise]; otherwise, <c>false</c>.
        /// </value>
        public bool UseSmoothNoise
        {
            get => _useSmoothNoise;
            set => SetProperty(ref _useSmoothNoise, value, nameof(UseSmoothNoise));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [use turbulence].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [use turbulence]; otherwise, <c>false</c>.
        /// </value>
        public bool UseTurbulence
        {
            get => _useTurbulence;
            set => SetProperty(ref _useTurbulence, value, nameof(UseTurbulence));
        }

        /// <summary>
        ///     Gets or sets the xy period.
        /// </summary>
        /// <value>
        ///     The xy period.
        /// </value>
        public double XyPeriod
        {
            get => _xyPeriod;
            set => SetProperty(ref _xyPeriod, value, nameof(XyPeriod));
        }

        /// <summary>
        ///     Gets or sets the line spacing.
        /// </summary>
        /// <value>
        ///     The line spacing.
        /// </value>
        public int LineSpacing
        {
            get => _lineSpacing;
            set => SetProperty(ref _lineSpacing, value, nameof(LineSpacing));
        }

        /// <summary>
        ///     Gets or sets the color of the line.
        /// </summary>
        /// <value>
        ///     The color of the line.
        /// </value>
        public Color LineColor
        {
            get => _lineColor;
            set => SetProperty(ref _lineColor, value, nameof(LineColor));
        }

        /// <summary>
        ///     Gets or sets the line thickness.
        /// </summary>
        /// <value>
        ///     The line thickness.
        /// </value>
        public int LineThickness
        {
            get => _lineThickness;
            set => SetProperty(ref _lineThickness, value, nameof(LineThickness));
        }

        /// <summary>
        ///     Gets or sets the angle1.
        /// </summary>
        /// <value>
        ///     The angle1.
        /// </value>
        public double AnglePrimary
        {
            get => _anglePrimary;
            set => SetProperty(ref _anglePrimary, value, nameof(AnglePrimary));
        }

        /// <summary>
        ///     Gets or sets the angle2.
        /// </summary>
        /// <value>
        ///     The angle2.
        /// </value>
        public double AngleSecondary
        {
            get => _angleSecondary;
            set => SetProperty(ref _angleSecondary, value, nameof(AngleSecondary));
        }

        /// <summary>
        ///     Gets or sets the wave frequency.
        /// </summary>
        /// <value>
        ///     The wave frequency.
        /// </value>
        public double WaveFrequency
        {
            get => _waveFrequency;
            set => SetProperty(ref _waveFrequency, value, nameof(WaveFrequency));
        }

        /// <summary>
        ///     Gets or sets the wave amplitude.
        /// </summary>
        /// <value>
        ///     The wave amplitude.
        /// </value>
        public double WaveAmplitude
        {
            get => _waveAmplitude;
            set => SetProperty(ref _waveAmplitude, value, nameof(WaveAmplitude));
        }

        /// <summary>
        ///     Gets or sets the randomization factor.
        /// </summary>
        /// <value>
        ///     The randomization factor.
        /// </value>
        public double RandomizationFactor
        {
            get => _randomizationFactor;
            set => SetProperty(ref _randomizationFactor, value, nameof(RandomizationFactor));
        }

        /// <summary>
        ///     Gets or sets the edge jaggedness limit.
        /// </summary>
        /// <value>
        ///     The edge jaggedness limit.
        /// </value>
        public int EdgeJaggednessLimit
        {
            get => _edgeJaggednessLimit;
            set => SetProperty(ref _edgeJaggednessLimit, value, nameof(EdgeJaggednessLimit));
        }

        /// <summary>
        ///     Gets or sets the jaggedness threshold.
        /// </summary>
        /// <value>
        ///     The jaggedness threshold.
        /// </value>
        public int JaggednessThreshold
        {
            get => _jaggednessThreshold;
            set => SetProperty(ref _jaggednessThreshold, value, nameof(JaggednessThreshold));
        }

        /// <summary>
        ///     Gets or sets the size of the cell. Used by the Cellular texture.
        /// </summary>
        /// <value>
        ///     The size of the cell.
        /// </value>
        public int CellSize
        {
            get => _cellSize;
            set => SetProperty(ref _cellSize, value, nameof(CellSize));
        }

        /// <summary>
        ///     Gets or sets the color of the center. Used by the Cellular texture.
        /// </summary>
        /// <value>
        ///     The color of the center.
        /// </value>
        public Color CenterColor
        {
            get => _centerColor;
            set => SetProperty(ref _centerColor, value, nameof(CenterColor));
        }

        /// <summary>
        ///     Gets or sets the color of the edge. Used by the Cellular texture.
        /// </summary>
        /// <value>
        ///     The color of the edge.
        /// </value>
        public Color EdgeColor
        {
            get => _edgeColor;
            set => SetProperty(ref _edgeColor, value, nameof(EdgeColor));
        }

        /// <summary>
        ///     Gets or sets the color ramp as a comma-separated list of #AARRGGBB tokens (e.g.
        ///     "#FFFF4400, #FFFFCC00"). Used by the ColorMapped texture. A plain array doesn't bind
        ///     cleanly to a single text field, so this is the editable surface; <see cref="_colorRamp" />
        ///     holds the parsed <see cref="Color" /> array that actually round-trips through
        ///     <see cref="TextureConfiguration.ColorRamp" />.
        /// </summary>
        /// <value>
        ///     The color ramp, as text.
        /// </value>
        public string ColorRampText
        {
            get => _colorRampText ?? string.Empty;
            set
            {
                if (!SetProperty(ref _colorRampText, value, nameof(ColorRampText))) return;

                _colorRamp = ParseColorRamp(value);
            }
        }

        /// <summary>
        ///     Formats a <see cref="Color" /> array as the comma-separated #AARRGGBB text
        ///     <see cref="ColorRampText" /> expects. Inverse of <see cref="ParseColorRamp" />.
        /// </summary>
        /// <param name="ramp">The colors to format.</param>
        /// <returns>The formatted text, or an empty string if <paramref name="ramp" /> is <c>null</c>.</returns>
        private static string FormatColorRamp(Color[]? ramp) =>
            ramp == null
                ? string.Empty
                : string.Join(", ", ramp.Select(c => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}"));

        /// <summary>
        ///     Gets or sets the secondary color.
        /// </summary>
        /// <remarks>
        ///     Currently has no effect: <c>TextureAreas.GenerateTexture</c> only reads it (as
        ///     <c>BaseColor</c>/<c>SecondaryColor</c>) inside <c>TexturePresets.GenerateCobblestone</c>/
        ///     <c>GenerateDragonScales</c>, and those are invoked as parameterless preset calls that never
        ///     receive the settings object this window edits. Exposed here for completeness and so it
        ///     round-trips through save/load without being silently dropped; wire it up on the generation
        ///     side if you want it to actually influence output.
        /// </remarks>
        /// <value>
        ///     The color of the secondary.
        /// </value>
        public Color SecondaryColor
        {
            get => _secondaryColor;
            set => SetProperty(ref _secondaryColor, value, nameof(SecondaryColor));
        }

        /// <summary>
        ///     Parses a comma-separated list of #AARRGGBB tokens back into a <see cref="Color" /> array.
        ///     Invalid or empty input yields <c>null</c> rather than throwing, so a bad paste can't crash
        ///     the window.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>The parsed colors, or <c>null</c> if <paramref name="text" /> was empty or invalid.</returns>
        private static Color[]? ParseColorRamp(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            try
            {
                var tokens = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (tokens.Length == 0) return null;

                var colors = new Color[tokens.Length];
                for (var i = 0; i < tokens.Length; i++)
                {
                    var token = tokens[i].TrimStart('#');
                    var argb = int.Parse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    colors[i] = Color.FromArgb(argb);
                }

                return colors;
            }
            catch (Exception ex) when (ex is FormatException or OverflowException)
            {
                // Leave the ramp unchanged rather than propagating a parse error into the UI thread.
                return null;
            }
        }

        // --- Active properties ---

        /// <summary>
        /// The is minimum value active
        /// </summary>
        private bool _isMinValueActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is minimum value active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is minimum value active; otherwise, <c>false</c>.
        /// </value>
        public bool IsMinValueActive
        {
            get => _isMinValueActive;
            set => SetProperty(ref _isMinValueActive, value, nameof(IsMinValueActive));
        }

        /// <summary>
        /// The is maximum value active
        /// </summary>
        private bool _isMaxValueActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is maximum value active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is maximum value active; otherwise, <c>false</c>.
        /// </value>
        public bool IsMaxValueActive
        {
            get => _isMaxValueActive;
            set => SetProperty(ref _isMaxValueActive, value, nameof(IsMaxValueActive));
        }

        private bool _isAlphaActive;

        public bool IsAlphaActive
        {
            get => _isAlphaActive;
            set => SetProperty(ref _isAlphaActive, value, nameof(IsAlphaActive));
        }

        /// <summary>
        /// The is x period active
        /// </summary>
        private bool _isXPeriodActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is x period active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is x period active; otherwise, <c>false</c>.
        /// </value>
        public bool IsXPeriodActive
        {
            get => _isXPeriodActive;
            set => SetProperty(ref _isXPeriodActive, value, nameof(IsXPeriodActive));
        }

        /// <summary>
        /// The is y period active
        /// </summary>
        private bool _isYPeriodActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is y period active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is y period active; otherwise, <c>false</c>.
        /// </value>
        public bool IsYPeriodActive
        {
            get => _isYPeriodActive;
            set => SetProperty(ref _isYPeriodActive, value, nameof(IsYPeriodActive));
        }

        /// <summary>
        /// The is turbulence power active
        /// </summary>
        private bool _isTurbulencePowerActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is turbulence power active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is turbulence power active; otherwise, <c>false</c>.
        /// </value>
        public bool IsTurbulencePowerActive
        {
            get => _isTurbulencePowerActive;
            set => SetProperty(ref _isTurbulencePowerActive, value, nameof(IsTurbulencePowerActive));
        }

        private bool _isTurbulenceSizeActive;

        public bool IsTurbulenceSizeActive
        {
            get => _isTurbulenceSizeActive;
            set => SetProperty(ref _isTurbulenceSizeActive, value, nameof(IsTurbulenceSizeActive));
        }

        /// <summary>
        /// The is base color active
        /// </summary>
        private bool _isBaseColorActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is base color active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is base color active; otherwise, <c>false</c>.
        /// </value>
        public bool IsBaseColorActive
        {
            get => _isBaseColorActive;
            set => SetProperty(ref _isBaseColorActive, value, nameof(IsBaseColorActive));
        }

        /// <summary>
        /// The is monochrome active
        /// </summary>
        private bool _isMonochromeActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is monochrome active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is monochrome active; otherwise, <c>false</c>.
        /// </value>
        public bool IsMonochromeActive
        {
            get => _isMonochromeActive;
            set => SetProperty(ref _isMonochromeActive, value, nameof(IsMonochromeActive));
        }

        /// <summary>
        /// The is tiled active
        /// </summary>
        private bool _isTiledActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tiled active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is tiled active; otherwise, <c>false</c>.
        /// </value>
        public bool IsTiledActive
        {
            get => _isTiledActive;
            set => SetProperty(ref _isTiledActive, value, nameof(IsTiledActive));
        }

        /// <summary>
        /// The is use smooth noise active
        /// </summary>
        private bool _isUseSmoothNoiseActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is use smooth noise active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is use smooth noise active; otherwise, <c>false</c>.
        /// </value>
        public bool IsUseSmoothNoiseActive
        {
            get => _isUseSmoothNoiseActive;
            set => SetProperty(ref _isUseSmoothNoiseActive, value, nameof(IsUseSmoothNoiseActive));
        }

        /// <summary>
        /// The is use turbulence active
        /// </summary>
        private bool _isUseTurbulenceActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is use turbulence active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is use turbulence active; otherwise, <c>false</c>.
        /// </value>
        public bool IsUseTurbulenceActive
        {
            get => _isUseTurbulenceActive;
            set => SetProperty(ref _isUseTurbulenceActive, value, nameof(IsUseTurbulenceActive));
        }

        /// <summary>
        /// The is xy period active
        /// </summary>
        private bool _isXyPeriodActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is xy period active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is xy period active; otherwise, <c>false</c>.
        /// </value>
        public bool IsXyPeriodActive
        {
            get => _isXyPeriodActive;
            set => SetProperty(ref _isXyPeriodActive, value, nameof(IsXyPeriodActive));
        }

        /// <summary>
        /// The is randomization factor active
        /// </summary>
        private bool _isRandomizationFactorActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is randomization factor active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is randomization factor active; otherwise, <c>false</c>.
        /// </value>
        public bool IsRandomizationFactorActive
        {
            get => _isRandomizationFactorActive;
            set => SetProperty(ref _isRandomizationFactorActive, value, nameof(IsRandomizationFactorActive));
        }

        /// <summary>
        /// The is edge jaggedness limit active
        /// </summary>
        private bool _isEdgeJaggednessLimitActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is edge jaggedness limit active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is edge jaggedness limit active; otherwise, <c>false</c>.
        /// </value>
        public bool IsEdgeJaggednessLimitActive
        {
            get => _isEdgeJaggednessLimitActive;
            set => SetProperty(ref _isEdgeJaggednessLimitActive, value, nameof(IsEdgeJaggednessLimitActive));
        }

        /// <summary>
        /// The is jaggedness threshold active
        /// </summary>
        private bool _isJaggednessThresholdActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is jaggedness threshold active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is jaggedness threshold active; otherwise, <c>false</c>.
        /// </value>
        public bool IsJaggednessThresholdActive
        {
            get => _isJaggednessThresholdActive;
            set => SetProperty(ref _isJaggednessThresholdActive, value, nameof(IsJaggednessThresholdActive));
        }

        /// <summary>
        /// The is wave frequency active
        /// </summary>
        private bool _isWaveFrequencyActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is wave frequency active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is wave frequency active; otherwise, <c>false</c>.
        /// </value>
        public bool IsWaveFrequencyActive
        {
            get => _isWaveFrequencyActive;
            set => SetProperty(ref _isWaveFrequencyActive, value, nameof(IsWaveFrequencyActive));
        }

        /// <summary>
        /// The is wave amplitude active
        /// </summary>
        private bool _isWaveAmplitudeActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is wave amplitude active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is wave amplitude active; otherwise, <c>false</c>.
        /// </value>
        public bool IsWaveAmplitudeActive
        {
            get => _isWaveAmplitudeActive;
            set => SetProperty(ref _isWaveAmplitudeActive, value, nameof(IsWaveAmplitudeActive));
        }

        /// <summary>
        /// The is line spacing active
        /// </summary>
        private bool _isLineSpacingActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is line spacing active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is line spacing active; otherwise, <c>false</c>.
        /// </value>
        public bool IsLineSpacingActive
        {
            get => _isLineSpacingActive;
            set => SetProperty(ref _isLineSpacingActive, value, nameof(IsLineSpacingActive));
        }

        /// <summary>
        /// The is line color active
        /// </summary>
        private bool _isLineColorActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is line color active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is line color active; otherwise, <c>false</c>.
        /// </value>
        public bool IsLineColorActive
        {
            get => _isLineColorActive;
            set => SetProperty(ref _isLineColorActive, value, nameof(IsLineColorActive));
        }

        /// <summary>
        /// The is line thickness active
        /// </summary>
        private bool _isLineThicknessActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is line thickness active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is line thickness active; otherwise, <c>false</c>.
        /// </value>
        public bool IsLineThicknessActive
        {
            get => _isLineThicknessActive;
            set => SetProperty(ref _isLineThicknessActive, value, nameof(IsLineThicknessActive));
        }

        /// <summary>
        /// The is angle primary active
        /// </summary>
        private bool _isAnglePrimaryActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is angle primary active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is angle primary active; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnglePrimaryActive
        {
            get => _isAnglePrimaryActive;
            set => SetProperty(ref _isAnglePrimaryActive, value, nameof(IsAnglePrimaryActive));
        }

        /// <summary>
        /// The is angle secondary active
        /// </summary>
        private bool _isAngleSecondaryActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is angle secondary active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is angle secondary active; otherwise, <c>false</c>.
        /// </value>
        public bool IsAngleSecondaryActive
        {
            get => _isAngleSecondaryActive;
            set => SetProperty(ref _isAngleSecondaryActive, value, nameof(IsAngleSecondaryActive));
        }

        /// <summary>
        /// The is cell size active
        /// </summary>
        private bool _isCellSizeActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is cell size active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is cell size active; otherwise, <c>false</c>.
        /// </value>
        public bool IsCellSizeActive
        {
            get => _isCellSizeActive;
            set => SetProperty(ref _isCellSizeActive, value, nameof(IsCellSizeActive));
        }

        /// <summary>
        /// The is center color active
        /// </summary>
        private bool _isCenterColorActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is center color active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is center color active; otherwise, <c>false</c>.
        /// </value>
        public bool IsCenterColorActive
        {
            get => _isCenterColorActive;
            set => SetProperty(ref _isCenterColorActive, value, nameof(IsCenterColorActive));
        }

        /// <summary>
        /// The is edge color active
        /// </summary>
        private bool _isEdgeColorActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is edge color active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is edge color active; otherwise, <c>false</c>.
        /// </value>
        public bool IsEdgeColorActive
        {
            get => _isEdgeColorActive;
            set => SetProperty(ref _isEdgeColorActive, value, nameof(IsEdgeColorActive));
        }

        /// <summary>
        /// The is color ramp active
        /// </summary>
        private bool _isColorRampActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is color ramp active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is color ramp active; otherwise, <c>false</c>.
        /// </value>
        public bool IsColorRampActive
        {
            get => _isColorRampActive;
            set => SetProperty(ref _isColorRampActive, value, nameof(IsColorRampActive));
        }

        /// <summary>
        /// The is secondary color active
        /// </summary>
        private bool _isSecondaryColorActive;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is secondary color active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is secondary color active; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecondaryColorActive
        {
            get => _isSecondaryColorActive;
            set => SetProperty(ref _isSecondaryColorActive, value, nameof(IsSecondaryColorActive));
        }

        // --- Command area ---

        /// <summary>
        ///     Gets the save command.
        /// </summary>
        /// <value>
        ///     The save command.
        /// </value>
        public ICommand SaveCommand => GetCommand(ref _saveCommand, SaveAction);

        /// <summary>
        ///     Gets the reset command.
        /// </summary>
        /// <value>
        ///     The reset command.
        /// </value>
        public ICommand ResetCommand => GetCommand(ref _resetCommand, ResetAction);

        /// <summary>
        ///     Gets the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="execute">The execute.</param>
        /// <returns>The selected Command</returns>
        private ICommand GetCommand(ref ICommand? command, Action<object> execute)
        {
            return command ??= new DelegateCommand<object>(execute, CanExecute);
        }

        /// <summary>
        ///     Saves the action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void SaveAction(object obj)
        {
            SaveSettings();
        }

        /// <summary>
        ///     Resets the action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void ResetAction(object obj)
        {
            var config = new TextureConfiguration();

            ImagingFacade.SetTextureSettings(SelectedTexture, config);
            LoadFromConfig(config);

            //  Persist the reset state to the local folder
            PersistSettingsToDisk();
            UpdateActiveProperties();
        }

        /// <summary>
        ///     Updates the active properties.
        /// </summary>
        private void UpdateActiveProperties()
        {
            var usedProperties = ImagingFacade.GetTextureProperties(SelectedTexture);

            IsMinValueActive = usedProperties.Contains(nameof(MinValue));
            IsMaxValueActive = usedProperties.Contains(nameof(MaxValue));
            IsAlphaActive = usedProperties.Contains(nameof(Alpha));
            IsXPeriodActive = usedProperties.Contains(nameof(XPeriod));
            IsYPeriodActive = usedProperties.Contains(nameof(YPeriod));
            IsTurbulencePowerActive = usedProperties.Contains(nameof(TurbulencePower));
            IsTurbulenceSizeActive = usedProperties.Contains(nameof(TurbulenceSize));
            IsBaseColorActive = usedProperties.Contains(nameof(BaseColor));
            IsMonochromeActive = usedProperties.Contains(nameof(IsMonochrome));
            IsTiledActive = usedProperties.Contains(nameof(IsTiled));
            IsUseSmoothNoiseActive = usedProperties.Contains(nameof(UseSmoothNoise));
            IsUseTurbulenceActive = usedProperties.Contains(nameof(UseTurbulence));
            IsXyPeriodActive = usedProperties.Contains(nameof(XyPeriod));
            IsRandomizationFactorActive = usedProperties.Contains(nameof(RandomizationFactor));
            IsEdgeJaggednessLimitActive = usedProperties.Contains(nameof(EdgeJaggednessLimit));
            IsJaggednessThresholdActive = usedProperties.Contains(nameof(JaggednessThreshold));
            IsWaveFrequencyActive = usedProperties.Contains(nameof(WaveFrequency));
            IsWaveAmplitudeActive = usedProperties.Contains(nameof(WaveAmplitude));
            IsLineSpacingActive = usedProperties.Contains(nameof(LineSpacing));
            IsLineColorActive = usedProperties.Contains(nameof(LineColor));
            IsLineThicknessActive = usedProperties.Contains(nameof(LineThickness));
            IsAnglePrimaryActive = usedProperties.Contains(nameof(AnglePrimary));
            IsAngleSecondaryActive = usedProperties.Contains(nameof(AngleSecondary));
            IsCellSizeActive = usedProperties.Contains(nameof(CellSize));
            IsCenterColorActive = usedProperties.Contains(nameof(CenterColor));
            IsEdgeColorActive = usedProperties.Contains(nameof(EdgeColor));
            IsColorRampActive = usedProperties.Contains(nameof(TextureConfiguration.ColorRamp));
            IsSecondaryColorActive = usedProperties.Contains(nameof(SecondaryColor));
        }

        /// <summary>
        ///     Saves the settings.
        /// </summary>
        private void SaveSettings()
        {
            // Gather the current properties into a TextureConfig object
            var config = new TextureConfiguration
            {
                MinValue = MinValue,
                MaxValue = MaxValue,
                Alpha = Alpha,
                XPeriod = XPeriod,
                YPeriod = YPeriod,
                TurbulencePower = TurbulencePower,
                TurbulenceSize = TurbulenceSize,
                BaseColor = BaseColor,
                IsMonochrome = IsMonochrome,
                IsTiled = IsTiled,
                UseSmoothNoise = UseSmoothNoise,
                UseTurbulence = UseTurbulence,
                XyPeriod = XyPeriod,
                LineSpacing = LineSpacing,
                LineColor = LineColor,
                LineThickness = LineThickness,
                AnglePrimary = AnglePrimary,
                AngleSecondary = AngleSecondary,
                WaveFrequency = WaveFrequency,
                WaveAmplitude = WaveAmplitude,
                RandomizationFactor = RandomizationFactor,
                EdgeJaggednessLimit = EdgeJaggednessLimit,
                JaggednessThreshold = JaggednessThreshold,
                CellSize = CellSize,
                CenterColor = CenterColor,
                EdgeColor = EdgeColor,
                ColorRamp = _colorRamp,
                SecondaryColor = SecondaryColor
            };

            // Update the settings in the backend registry
            ImagingFacade.SetTextureSettings(SelectedTexture, config);

            // Persist the configuration to the local folder
            PersistSettingsToDisk();

            // Provide feedback if needed or just update the UI state
            UpdateActiveProperties();
        }

        /// <summary>
        /// Maps a configuration object's values to the ViewModel properties.
        /// </summary>
        /// <param name="config">The configuration to load.</param>
        private void LoadFromConfig(TextureConfiguration? config)
        {
            if (config == null) return;

            BaseColor = config.BaseColor;
            LineColor = config.LineColor;
            MinValue = config.MinValue;
            MaxValue = config.MaxValue;
            Alpha = config.Alpha;
            XPeriod = config.XPeriod;
            YPeriod = config.YPeriod;
            TurbulencePower = config.TurbulencePower;
            TurbulenceSize = config.TurbulenceSize;
            IsMonochrome = config.IsMonochrome;
            IsTiled = config.IsTiled;
            UseSmoothNoise = config.UseSmoothNoise;
            UseTurbulence = config.UseTurbulence;
            XyPeriod = config.XyPeriod;
            LineSpacing = config.LineSpacing;
            LineThickness = config.LineThickness;
            AnglePrimary = config.AnglePrimary;
            AngleSecondary = config.AngleSecondary;
            WaveFrequency = config.WaveFrequency;
            WaveAmplitude = config.WaveAmplitude;
            RandomizationFactor = config.RandomizationFactor;
            EdgeJaggednessLimit = config.EdgeJaggednessLimit;
            JaggednessThreshold = config.JaggednessThreshold;
            CellSize = config.CellSize;
            CenterColor = config.CenterColor;
            EdgeColor = config.EdgeColor;
            ColorRampText = FormatColorRamp(config.ColorRamp);
            SecondaryColor = config.SecondaryColor;
        }

        /// <summary>
        /// Saves the current settings from the Facade to a local JSON file.
        /// </summary>
        private void PersistSettingsToDisk()
        {
            try
            {
                // Ensure the Config directory exists
                if (!Directory.Exists(_configFolderPath))
                {
                    Directory.CreateDirectory(_configFolderPath);
                }

                // Get JSON from Facade and write to disk
                var jsonConfig = ImagingFacade.GetSettingsAsJson();
                File.WriteAllText(_configFilePath, jsonConfig);
            }
            catch (Exception ex)
            {
                // Push errors to your Facade's logger
                ImagingFacade.LogError(ex);
            }
        }

        /// <summary>
        /// Loads previously saved settings from the local JSON file.
        /// </summary>
        private void LoadSettingsFromFile()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var jsonConfig = File.ReadAllText(_configFilePath);
                    ImagingFacade.LoadSettingsFromJson(jsonConfig);
                }
            }
            catch (Exception ex)
            {
                ImagingFacade.LogError(ex);
            }
        }
    }
}
