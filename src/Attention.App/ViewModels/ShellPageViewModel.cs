﻿using Prism.Commands;
using Prism.Windows.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;
using Attention.App.Views;
using System.Linq;
using Prism.Windows.AppModel;

namespace Attention.App.ViewModels
{
    public class ShellPageViewModel: ViewModelBase
    {
        private muxc.NavigationView _shellNav;
        private Frame _shellFrame;
        private readonly IResourceLoader _resourceLoader;

        public ShellPageViewModel(IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader ?? throw new ArgumentNullException(nameof(resourceLoader));
        }

        private bool _isBackEnabled;
        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }
            set { SetProperty(ref _isBackEnabled, value); }
        }

        private object _header;
        public object Header
        {
            get { return _header; }
            set { SetProperty(ref _header, value); }
        }

        private ObservableCollection<muxc.NavigationViewItemBase> _primaryItems;
        public ObservableCollection<muxc.NavigationViewItemBase> PrimaryItems
        {
            get { return _primaryItems ?? (_primaryItems = new ObservableCollection<muxc.NavigationViewItemBase>()); }
            set { SetProperty(ref _primaryItems, value); }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        private ICommand _loadCommand;
        public ICommand LoadCommand
        {
            get
            {
                if (_loadCommand == null)
                {
                    _loadCommand = new DelegateCommand(() => 
                    {
                        PrimaryItems.Clear();
                        
                        PrimaryItems.Add(new muxc.NavigationViewItemSeparator());
                        PrimaryItems.Add(new muxc.NavigationViewItemHeader() { Content = _resourceLoader.GetString("shellNav_menu") });
                        PrimaryItems.Add(new muxc.NavigationViewItem() { Content = _resourceLoader.GetString("shellNav_home"), Icon = new SymbolIcon(Symbol.Home), Tag = typeof(HomePage) });
                        PrimaryItems.Add(new muxc.NavigationViewItem() { Content = _resourceLoader.GetString("shellNav_download"), Icon = new SymbolIcon(Symbol.Download), Tag = typeof(DownloadPage) });

                        var first = PrimaryItems.OfType<muxc.NavigationViewItem>().FirstOrDefault();
                        _shellFrame.Navigate(Type.GetType(first.Tag.ToString()));
                    });
                }
                return _loadCommand;
            }
        }

        private ICommand _itemInvokedCommand;
        public ICommand ItemInvokedCommand
        {
            get
            {
                if (_itemInvokedCommand == null)
                {
                    _itemInvokedCommand = new DelegateCommand<muxc.NavigationViewItemInvokedEventArgs>(args =>
                    {
                        var pageType = args.IsSettingsInvoked ? typeof(SettingsPage) : Type.GetType(args.InvokedItemContainer.Tag.ToString());
                        if (pageType != null && SelectedItem != args.InvokedItemContainer)
                        {
                            _shellFrame.Navigate(pageType);
                        }
                    });
                }
                return _itemInvokedCommand;
            }
        }

        private ICommand _backRequestedCommand;
        public ICommand BackRequestedCommand
        {
            get
            {
                if (_backRequestedCommand == null)
                {
                    _backRequestedCommand = new DelegateCommand(() =>
                    {
                        _shellFrame.GoBack();
                    });
                }
                return _backRequestedCommand;
            }
        }

        public void Initialize(muxc.NavigationView shellNav, Frame frame)
        {
            _shellNav = shellNav ?? throw new ArgumentNullException(nameof(shellNav));
            _shellFrame = frame ?? throw new ArgumentNullException(nameof(frame));
            _shellFrame.Navigated += (sender, e) =>
            {
                IsBackEnabled = _shellFrame.CanGoBack;
                SelectedItem = e?.SourcePageType == typeof(SettingsPage)
                    ? _shellNav.SettingsItem
                    : PrimaryItems.OfType<muxc.NavigationViewItem>().FirstOrDefault(x => x.Tag.ToString() == e?.SourcePageType.ToString());
                Header = SelectedItem is muxc.NavigationViewItem navItem ? navItem.Content : (default);
            };
        }
    }
}