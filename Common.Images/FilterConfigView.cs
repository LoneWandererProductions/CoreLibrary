/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        FilterConfigView.cs
 * PURPOSE:     View Model for Filter Configuration
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
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
    ///     Set Input Fields active or Inactive based on the used Filter
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public sealed class FilterConfigView : ViewModelBase
    {
        /// <summary>
        ///     The current configuration
        /// </summary>
        private readonly FilterConfiguration? _currentConfig;

        /// <summary>
        ///     The base window size
        /// </summary>
        private int _baseWindowSize;

        /// <summary>
        ///     The bias
        /// </summary>
        private double _bias;

        /// <summary>
        ///     The factor
        /// </summary>
        private double _factor;

        /// <summary>
        ///     The reset command
        /// </summary>
        private ICommand? _resetCommand;

        /// <summary>
        ///     The save command
        /// </summary>
        private ICommand? _saveCommand;

        /// <summary>
        ///     The scale
        /// </summary>
        private int _scale;

        /// <summary>
        ///     The selected filter
        /// </summary>
        private FiltersType _selectedFilter;

        /// <summary>
        ///     The sigma
        /// </summary>
        private double _sigma;

        /// <summary>
        /// The configuration folder path
        /// </summary>
        private readonly string _configFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");

        /// <summary>
        /// The configuration file path
        /// </summary>
        private readonly string _configFilePath;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterConfigView" /> class.
        /// </summary>
        public FilterConfigView()
        {
            _configFilePath = Path.Combine(_configFolderPath, "FilterSettings.json");

            // 1. Load from disk into the Facade
            LoadSettingsFromFile();

            // 2. Set the default selected filter (e.g., the first one)
            SelectedFilter = FilterOptions.FirstOrDefault();

            // 3. UpdateActiveProperties will now pull the LOADED settings from the Facade
            // because it calls ImagingFacade.GetFilterSettings(SelectedFilter)
        }

        /// <summary>
        ///     Gets or sets the factor.
        /// </summary>
        public double Factor
        {
            get => _factor;
            set => SetProperty(ref _factor, value, nameof(Factor));
        }

        /// <summary>
        ///     Gets or sets the bias.
        /// </summary>
        public double Bias
        {
            get => _bias;
            set => SetProperty(ref _bias, value, nameof(Bias));
        }

        /// <summary>
        ///     Gets or sets the sigma.
        /// </summary>
        public double Sigma
        {
            get => _sigma;
            set => SetProperty(ref _sigma, value, nameof(Sigma));
        }

        /// <summary>
        ///     Gets or sets the base window size.
        /// </summary>
        public int BaseWindowSize
        {
            get => _baseWindowSize;
            set => SetProperty(ref _baseWindowSize, value, nameof(BaseWindowSize));
        }

        /// <summary>
        ///     Gets or sets the scale.
        /// </summary>
        public int Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value, nameof(Scale));
        }

        /// <summary>
        ///     Gets or sets the selected filter.
        /// </summary>
        /// <value>
        ///     The selected filter.
        /// </value>
        public FiltersType SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (_selectedFilter == value) return;

                _selectedFilter = value;
                OnPropertyChanged(nameof(SelectedFilter));
                UpdateActiveProperties();
            }
        }

        /// <summary>
        ///     Gets the filter options.
        /// </summary>
        /// <value>
        ///     The filter options.
        /// </value>
        public IEnumerable<FiltersType> FilterOptions =>
            Enum.GetValues(typeof(FiltersType)) as IEnumerable<FiltersType>;

        /// <summary>
        ///     Gets or sets the current configuration.
        /// </summary>
        /// <value>
        ///     The current configuration.
        /// </value>
        public FilterConfiguration? CurrentConfig
        {
            get => _currentConfig;
            init
            {
                _currentConfig = value;
                OnPropertyChanged(nameof(CurrentConfig));
            }
        }

        // Local properties to control UI element states

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is factor active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is factor active; otherwise, <c>false</c>.
        /// </value>
        public bool IsFactorActive { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is bias active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is bias active; otherwise, <c>false</c>.
        /// </value>
        public bool IsBiasActive { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is sigma active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is sigma active; otherwise, <c>false</c>.
        /// </value>
        public bool IsSigmaActive { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is base window size active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is base window size active; otherwise, <c>false</c>.
        /// </value>
        public bool IsBaseWindowSizeActive { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is scale active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is scale active; otherwise, <c>false</c>.
        /// </value>
        public bool IsScaleActive { get; set; }

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
        ///     Updates the active properties.
        /// </summary>
        private void UpdateActiveProperties()
        {
            // Get the used properties for the selected filter
            var usedProperties = ImagingFacade.GetFilterProperties(SelectedFilter);

            // Update active state for each property based on the selected filter
            IsFactorActive = usedProperties.Contains(nameof(Factor));
            IsBiasActive = usedProperties.Contains(nameof(Bias));
            IsSigmaActive = usedProperties.Contains(nameof(Sigma));
            IsBaseWindowSizeActive = usedProperties.Contains(nameof(BaseWindowSize));
            IsScaleActive = usedProperties.Contains(nameof(Scale));

            // SWITCHED TO FACADE: Retrieve the saved settings for the selected filter
            var savedSettings = ImagingFacade.GetFilterSettings(SelectedFilter);

            // Set the properties from the saved settings, if the property is active
            if (IsFactorActive) Factor = savedSettings.Factor;
            if (IsBiasActive) Bias = savedSettings.Bias;
            if (IsSigmaActive) Sigma = savedSettings.Sigma;
            if (IsBaseWindowSizeActive) BaseWindowSize = savedSettings.BaseWindowSize;
            if (IsScaleActive) Scale = savedSettings.Scale;

            // Notify UI about changes
            OnPropertyChanged(nameof(IsFactorActive));
            OnPropertyChanged(nameof(IsBiasActive));
            OnPropertyChanged(nameof(IsSigmaActive));
            OnPropertyChanged(nameof(IsBaseWindowSizeActive));
            OnPropertyChanged(nameof(IsScaleActive));
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
        ///     Saves the current configuration to the image processor.
        /// </summary>
        private void SaveSettings()
        {
            var config = new FilterConfiguration
            {
                Factor = Factor,
                Bias = Bias,
                Sigma = Sigma,
                BaseWindowSize = BaseWindowSize,
                Scale = Scale
            };

            // Update the settings in the backend registry
            ImagingFacade.SetFilterSettings(SelectedFilter, config);

            // Persist the configuration to the local folder
            PersistSettingsToDisk();

            // Provide feedback if needed or just update the UI state
            UpdateActiveProperties();
        }

        /// <summary>
        ///     Resets the selected filter to default values.
        /// </summary>
        private void ResetAction(object obj)
        {
            var defaultConfig = new FilterConfiguration(); // Pure defaults

            ImagingFacade.SetFilterSettings(SelectedFilter, defaultConfig);

            //  Persist the reset state to the local folder
            PersistSettingsToDisk();

            // Load the now-reset values back into the ViewModel fields
            Factor = defaultConfig.Factor;
            Bias = defaultConfig.Bias;
            Sigma = defaultConfig.Sigma;
            BaseWindowSize = defaultConfig.BaseWindowSize;
            Scale = defaultConfig.Scale;

            UpdateActiveProperties();
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
