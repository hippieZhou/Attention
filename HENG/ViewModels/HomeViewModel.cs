﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HENG.Models;
using HENG.Services;
using Microsoft.Toolkit.Collections;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.UI.Animations;
using PixabaySharp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace HENG.ViewModels
{
    public class HomeViewModel : PixViewModel<PhotoItemSource, ImageItem>
    {
        private ICommand _loadedCommand;
        public ICommand LoadedCommand
        {
            get
            {
                if (_loadedCommand == null)
                {
                    _loadedCommand = new RelayCommand(async () =>
                    {
                        await HeaderDown();
                    });
                }
                return _loadedCommand;
            }
        }

        private ICommand _queryCommand;
        public ICommand QueryCommand
        {
            get
            {
                if (_queryCommand == null)
                {
                    _queryCommand = new RelayCommand<AutoSuggestBoxQuerySubmittedEventArgs>(args =>
                    {
                        ViewModelLocator.Current.PxService.QueryText = args.QueryText;
                        RefreshCommand.Execute(null);
                    });
                }
                return _queryCommand;
            }
        }

        private ICommand _headerUpCommand;
        public ICommand HeaderUpCommand
        {
            get
            {
                if (_headerUpCommand == null)
                {
                    _headerUpCommand = new RelayCommand(async () => 
                    {
                        if (_headerMask.Visibility == Visibility.Collapsed)
                        {
                            await HeaderDown();
                            await HeaderUp();
                        }
                    });
                }
                return _headerUpCommand;
            }
        }

        private ICommand _headerDownCommand;
        public ICommand HeaderDownCommand
        {
            get
            {
                if (_headerDownCommand == null)
                {
                    _headerDownCommand = new RelayCommand<TappedRoutedEventArgs>(async (obj) =>
                    {
                        if (obj == null)
                            return;
                        if (obj.OriginalSource.GetType() == typeof(Grid))
                        {
                            var anim = _headerGrid.Offset(0, -(float)Math.Max(152, _headerGrid.ActualHeight));
                            anim.Completed += (sender, e) =>
                            {
                                _headerMask.Visibility = Visibility.Collapsed;
                            };
                            await anim.StartAsync();
                        }
                        obj.Handled = true;
                    });
                }
                return _headerDownCommand;
            }
        }

        private async Task HeaderUp()
        {
            _headerMask.Visibility = Visibility.Visible;
            await _headerGrid.Offset(0).StartAsync();
        }
        private async Task HeaderDown()
        {
            _headerMask.Visibility = Visibility.Collapsed;
            await _headerGrid.Offset(0, -(float)Math.Max(152, _headerGrid.ActualHeight), 0).StartAsync();
        }

        public override ICommand ItemClickCommand => new RelayCommand<ImageItem>(item => 
        {
            base.ItemClickCommand.Execute(item);

            ConnectedAnimation animation = null;
            if (StoredItem != null)
            {
                animation = _listView.PrepareConnectedAnimation("forwardAnimation", StoredItem, "connectedElement");
                animation.Completed += (sender, e) =>
                {
                };
            }
            ViewModelLocator.Current.Shell.ShowDetail(StoredItem, animation);
        });

        public async Task HideDetailAsync(ImageItem item, ConnectedAnimation animation)
        {
            _listView.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
            _listView.UpdateLayout();
            await _listView.TryStartConnectedAnimationAsync(animation, item, "connectedElement");
        }
    }

    public class PhotoItemSource : IIncrementalSource<ImageItem>
    {
        public async Task<IEnumerable<ImageItem>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var result = await ViewModelLocator.Current.PxService.QueryImagesAsync(page: ++pageIndex, per_page: pageSize);
            return result?.Images;
        }
    }

    public class PixViewModel<TSource, IType> : ViewModelBase where TSource : IIncrementalSource<IType>
    {
        protected ListViewBase _listView;
        protected Grid _headerMask;
        protected Grid _headerGrid;

        private IType _storedItem;
        public IType StoredItem
        {
            get { return _storedItem; }
            private set { Set(ref _storedItem, value); }
        }

        private IncrementalLoadingCollection<TSource, IType> _items;
        public IncrementalLoadingCollection<TSource, IType> Items
        {
            get { return _items; }
            set { Set(ref _items, value); }
        }

        private Visibility _loadingVisibility = Visibility.Collapsed;
        public Visibility LoadingVisibility
        {
            get { return _loadingVisibility; }
            set { Set(ref _loadingVisibility, value); }
        }

        private Visibility _errorVisibility = Visibility.Collapsed;
        public Visibility ErrorVisibility
        {
            get { return _errorVisibility; }
            set { Set(ref _errorVisibility, value); }
        }

        public virtual void Initialize(ListViewBase listView, Grid headerMask, int itemsPerPage = 20)
        {
            _listView = listView;
            _headerMask = headerMask;
            _headerGrid = _headerMask.FindName("HeaderGrid") as Grid;

            if (Items == null)
            {
                Items = new IncrementalLoadingCollection<TSource, IType>(itemsPerPage,
                    () =>
                    {
                        LoadingVisibility = Visibility.Visible;
                        ErrorVisibility = Visibility.Collapsed;
                    },
                    () =>
                    {
                        LoadingVisibility = Visibility.Collapsed;
                    },
                    ex =>
                    {
                        ErrorVisibility = Visibility.Visible;
                    });
            };
        }

        private ICommand _itemClickCommand;
        public virtual ICommand ItemClickCommand
        {
            get
            {
                if (_itemClickCommand == null)
                {
                    _itemClickCommand = new RelayCommand<IType>(item =>
                    {
                        StoredItem = item;
                    });
                }
                return _itemClickCommand;
            }
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand(async () =>
                    {
                        await Items.RefreshAsync();
                    });
                }
                return _refreshCommand;
            }
        }

        private ICommand _downloadCommand;
        public ICommand DownloadCommand
        {
            get
            {
                if (_downloadCommand == null)
                {
                    _downloadCommand = new RelayCommand<IType>(async item =>
                    {
                        if (item is ImageItem val)
                        {
                            var download = new DownloadItem(val);
                            await DownloadService.DownloadAsync(download);
                        }
                    });
                }
                return _downloadCommand;
            }
        }
    }
}
