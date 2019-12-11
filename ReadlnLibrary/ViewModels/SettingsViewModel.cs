using System;
using System.Threading.Tasks;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using ReadlnLibrary.Helpers;
using ReadlnLibrary.Services;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;

namespace ReadlnLibrary.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    public class SettingsViewModel : ViewModelBase
    {
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        private ICommand _switchThemeCommand;

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            ElementTheme = param;
                            await ThemeSelectorService.SetThemeAsync(param);
                        });
                }

                return _switchThemeCommand;
            }
        }


        private string _settingPattern;
        public string SettingPattern
        {
            get { return _settingPattern; }
            set { Set(ref _settingPattern, value); }
        }

        private ICommand _savePatternCommand;

        public ICommand SavePatternCommand
        {
            get
            {
                if (_savePatternCommand == null)
                {
                    _savePatternCommand = new RelayCommand(
                        async () =>
                        {
                            await SavePattern(SettingPattern);
                        });
                }

                return _savePatternCommand;
            }
        }



        public SettingsViewModel()
        {
        }

        public async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            SettingPattern = await GetSettingPattern();
        }
        private async Task SavePattern(string settingPattern)
        {
            await ApplicationData.Current.LocalFolder.SaveAsync(Constants.Settings.PATTERN, settingPattern).ConfigureAwait(false);

        }
        private async Task<string> GetSettingPattern()
        {
            string pattern = null;
            try
            {
                pattern = await ApplicationData.Current.LocalFolder.ReadAsync<string>(Constants.Settings.PATTERN).ConfigureAwait(false);

            }
            catch (Exception ex)
            {

            }
            return pattern;

        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
