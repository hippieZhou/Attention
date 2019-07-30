﻿using GalaSoft.MvvmLight;
using Microsoft.Toolkit.Uwp.Extensions;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace HENG.App.Models
{
    /// <summary>
    /// https://edi.wang/post/2017/9/8/uwp-read-write-settings
    /// https://edi.wang/post/2016/2/18/windows-10-uwp-async-await-ui-thread
    /// https://edi.wang/post/2017/9/9/windows-10-uwp-switching-language
    /// https://edi.wang/post/2015/12/30/windows-10-uwp-report-error-page
    /// </summary>
    public class AppSettings : ObservableObject
    {
        static private AppSettings _current = null;
        static public AppSettings Current => _current ?? (Application.Current.Resources["AppSettings"] as AppSettings);

        public AppSettings()
        {
            if (SystemInformation.IsFirstRun)
            {
                ThemeMode = (int)ElementTheme.Default;
                Language = Windows.Globalization.Language.CurrentInputMethodLanguageTag == "zh-Hans-CN" ? 0 : 1;
            }
        }

        public ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

        public string DbPath => Path.Combine(ApplicationData.Current.LocalFolder.Path, "Storage.sqlite");

        public int ThemeMode
        {
            get => GetSettingsValue(nameof(ThemeMode), (int)ElementTheme.Default);
            set
            {
                SetSettingsValue(nameof(ThemeMode), value);
                RaisePropertyChanged(() => ThemeMode);

                UpdateTheme();
            }
        }

        public int Language
        {
            get
            {
                return GetSettingsValue(nameof(Language), 0);
            }
            set
            {
                SetSettingsValue(nameof(Language), value);
                RaisePropertyChanged(() => Language);

                UpdateLanguage();
            }
        }

        public string AppName => "AppDisplayName".GetLocalized();

        public string AppVersion
        {
            get
            {
                PackageVersion version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }

        private string _downloadPath;
        public string DownloadPath
        {
            get { return _downloadPath; }
            set
            {
                _downloadPath = value;
                RaisePropertyChanged(() => DownloadPath);
            }
        }

        public async Task InitConfiguration()
        {
            UpdateTheme();
            UpdateLanguage();
            await UpdateDownloadPath();
        }

        #region 私有方法
        private void UpdateTheme()
        {
            var theme = (ElementTheme)ThemeMode;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = theme == ElementTheme.Default || theme == ElementTheme.Light ? Colors.Black : Colors.White;
            if (Window.Current.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = theme;
            }
        }

        private void UpdateLanguage()
        {
            ApplicationLanguages.PrimaryLanguageOverride = Language == 0 ? "zh-Hans-CN" : "en-US";
        }

        private async Task UpdateDownloadPath()
        {
            StorageFolder folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("HENG", CreationCollisionOption.OpenIfExists);
            DownloadPath = folder.Path;
        }

        private TResult GetSettingsValue<TResult>(string name, TResult defaultValue)
        {
            try
            {
                if (!LocalSettings.Values.ContainsKey(name))
                {
                    LocalSettings.Values[name] = defaultValue;
                }
                return (TResult)LocalSettings.Values[name];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return defaultValue;
            }
        }

        private void SetSettingsValue(string name, object value)
        {
            LocalSettings.Values[name] = value;
        }
        #endregion
    }
}