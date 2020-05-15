﻿using Attention.App.Businesss;
using Attention.App.Extensions;
using Attention.App.ViewModels.UcViewModels;
using Attention.Core.Dtos;
using Attention.Core.Events;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Prism.Commands;
using Prism.Events;
using Prism.Logging;
using Prism.Windows.Mvvm;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Attention.App.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private readonly ILoggerFacade _logger;
        private readonly IEventAggregator _eventAggregator;
        private AdaptiveGridView _adaptiveGV;

        private IncrementalLoadingCollection<WallpaperItemSource, WallpaperDto> _entities;
        public IncrementalLoadingCollection<WallpaperItemSource, WallpaperDto> Entities
        {
            get { return _entities; }
            set { SetProperty(ref _entities, value); }
        }

        public HomePageViewModel(ILoggerFacade logger, IEventAggregator eventAggregator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        private Visibility _notFound;
        public Visibility NotFound
        {
            get { return _notFound; }
            set { SetProperty(ref _notFound, value); }
        }

        private DetailCardViewModel _cardViewModel;
        public DetailCardViewModel CardViewModel
        {
            get { return _cardViewModel; }
            set { SetProperty(ref _cardViewModel, value); }
        }

        private ICommand _loadCommand;
        public ICommand LoadCommand
        {
            get
            {
                if (_loadCommand == null)
                {
                    _loadCommand = new DelegateCommand<AdaptiveGridView>(adaptiveGV =>
                    {
                        _adaptiveGV = adaptiveGV ?? throw new ArgumentNullException(nameof(adaptiveGV));
                        _adaptiveGV.SizeChanged += (sender, args) =>
                        {
                            if (args.NewSize != args.PreviousSize && _adaptiveGV.SelectedItem != null)
                            {
                                _adaptiveGV.ScrollIntoView(_adaptiveGV.SelectedItem, ScrollIntoViewAlignment.Default);
                            }
                        };

                        CardViewModel = new DetailCardViewModel();
                        CardViewModel.TryStartBackwardsAnimation += async (sender, args) =>
                        {
                            _adaptiveGV.ScrollIntoView(args.Item1);
                            _adaptiveGV.UpdateLayout();
                            if (_adaptiveGV.ContainerFromItem(args.Item1) is GridViewItem container)
                            {
                                container.Opacity = 1.0d;
                            }
                            await _adaptiveGV.TryStartConnectedAnimationAsync(args.Item2, args.Item1, "connectedElement");
                        };

                        Entities = new IncrementalLoadingCollection<WallpaperItemSource, WallpaperDto>(10, () =>
                         {
                         }, () =>
                         {
                         }, ex =>
                         {
                             _logger.Log(ex.ToString(), Category.Exception, Priority.High);
                             _eventAggregator.GetEvent<NotificationEvent>().Publish("THERE HAVE SOMETHING WRONG");
                         });

                        Entities.CollectionChanged += (sender, e) => 
                        {
                            NotFound = Entities?.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
                        };
                    });
                }
                return _loadCommand;
            }
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new DelegateCommand(async () =>
                    {
                        await Entities.RefreshAsync();
                    });
                }
                return _refreshCommand;
            }
        }

        protected ICommand _itemClickCommand;
        public ICommand ItemClickCommand
        {
            get
            {
                if (_itemClickCommand == null)
                {
                    _itemClickCommand = new DelegateCommand<WallpaperDto>(entity =>
                    {
                        if (_adaptiveGV.ContainerFromItem(entity) is GridViewItem container)
                        {
                            container.Opacity = 0.0d;
                            var animation = container.CreateForwardAnimation(_adaptiveGV, entity, () =>
                           {
                               CardViewModel.AvatarVisibility = Visibility.Visible;
                               CardViewModel.FooterVisibility = Visibility.Visible;
                           });
                            CardViewModel.TryStartForwardAnimation(entity, animation);
                        }
                    });
                }
                return _itemClickCommand;
            }
        }
    }
}
