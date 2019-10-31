﻿using Attention.UWP.Helpers;
using Attention.UWP.Models;
using Attention.UWP.Models.Repositories;
using Attention.UWP.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MetroLog;
using PixabaySharp.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Attention.UWP.ViewModels
{
    public class PhotoItemViewModel : ViewModelBase
    {
        private UIElement _destinationElement;

        private Visibility _visibility = Visibility.Collapsed;
        public Visibility Visibility
        {
            get { return _visibility; }
            set { Set(ref _visibility, value); }
        }

        private ImageItem _item;
        public ImageItem Item
        {
            get { return _item; }
            set { Set(ref _item, value); }
        }

        protected ICommand _loadedCommand;
        public ICommand LoadedCommand
        {
            get
            {
                if (_loadedCommand == null)
                {
                    _loadedCommand = new RelayCommand<UIElement>(destinationElement =>
                    {
                        _destinationElement = destinationElement;
                        Visibility = Visibility.Collapsed;
                    });
                }
                return _loadedCommand;
            }
        }

        private ICommand _backCommand;
        public ICommand BackCommand
        {
            get
            {
                if (_backCommand == null)
                {
                    _backCommand = new RelayCommand<TappedRoutedEventArgs>(async args =>
                    {
                        if (args.OriginalSource is FrameworkElement root && root.Name == "OverlayPopup")
                        {
                            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", _destinationElement);
                            animation.Configuration = new DirectConnectedAnimationConfiguration();
                            animation.IsScaleAnimationEnabled = true;
                            var done = await ViewModelLocator.Current.Main.PhotoGridViewModel.TryStart(Item, animation);
                            if (done)
                            {
                                Visibility = Visibility.Collapsed;
                            }
                        }
                    });
                }
                return _backCommand;
            }
        }

        private ICommand _downloadCommand;
        public ICommand DownloadCommand
        {
            get
            {
                if (_downloadCommand == null)
                {
                    _downloadCommand = new RelayCommand(async () =>
                    {
                        StorageFolder folder = await App.Settings.GetSavingFolderAsync();
                        DownloadItemResult state = await new DownloadItem(Item, folder).DownloadAsync();
                        Debug.WriteLine(state);
                    });
                }
                return _downloadCommand;
            }
        }

        private ICommand _shareCommand;
        public ICommand ShareCommand
        {
            get
            {
                if (_shareCommand == null)
                {
                    _shareCommand = new RelayCommand(() =>
                    {
                        ShareHelper.ShareData(Item);
                    });
                }
                return _shareCommand;
            }
        }

        private ICommand _browseCommand;
        public ICommand BrowseCommand
        {
            get
            {
                if (_browseCommand == null)
                {
                    _browseCommand = new RelayCommand(async () =>
                    {
                        await Launcher.LaunchUriAsync(new Uri(Item.PageURL));
                    });
                }
                return _browseCommand;
            }
        }

        public bool TryStart(object selected, ConnectedAnimation animation)
        {
            if (selected is ImageItem item)
            {
                Item = item;
                Visibility = Visibility.Visible;
                return animation.TryStart(_destinationElement);
            }
            else
            {
                return false;
            }
        }
    }
}
