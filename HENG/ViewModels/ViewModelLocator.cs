﻿using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using CommonServiceLocator;
using HENG.Views;

namespace HENG.ViewModels
{
    [Windows.UI.Xaml.Data.Bindable]
    public class ViewModelLocator
    {
        private static ViewModelLocator _current;

        public static ViewModelLocator Current => _current ?? (_current = new ViewModelLocator());

        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var nav = new NavigationService();

            nav.Configure(typeof(HomeViewModel).FullName, typeof(HomeView));
            nav.Configure(typeof(PicsumViewModel).FullName, typeof(PicsumView));
            nav.Configure(typeof(NewestViewModel).FullName, typeof(NewestView));
            nav.Configure(typeof(HottestViewModel).FullName, typeof(HottestView));
            nav.Configure(typeof(GirlsViewModel).FullName, typeof(GirlsView));
            nav.Configure(typeof(SkylandViewModel).FullName, typeof(SkylandView));
            nav.Configure(typeof(LocalViewModel).FullName, typeof(LocalView));
            nav.Configure(typeof(SettingsViewModel).FullName, typeof(SettingsView));

            SimpleIoc.Default.Register<INavigationService>(() => nav);
            SimpleIoc.Default.Register<IDialogService, DialogService>();

            SimpleIoc.Default.Register<ShellViewModel>();
            SimpleIoc.Default.Register<DetailViewModel>();

            SimpleIoc.Default.Register<HomeViewModel>();
            SimpleIoc.Default.Register<PicsumViewModel>();
            SimpleIoc.Default.Register<NewestViewModel>();
            SimpleIoc.Default.Register<HottestViewModel>();
            SimpleIoc.Default.Register<GirlsViewModel>();
            SimpleIoc.Default.Register<SkylandViewModel>();
            SimpleIoc.Default.Register<LocalViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        public ShellViewModel Shell => ServiceLocator.Current.GetInstance<ShellViewModel>();
        public DetailViewModel Detail => ServiceLocator.Current.GetInstance<DetailViewModel>();

        public HomeViewModel Home => ServiceLocator.Current.GetInstance<HomeViewModel>();
        public PicsumViewModel Picsum => ServiceLocator.Current.GetInstance<PicsumViewModel>();
        public NewestViewModel Newest => ServiceLocator.Current.GetInstance<NewestViewModel>();
        public HottestViewModel Hottest => ServiceLocator.Current.GetInstance<HottestViewModel>();
        public GirlsViewModel Girls => ServiceLocator.Current.GetInstance<GirlsViewModel>();
        public SkylandViewModel Skyland => ServiceLocator.Current.GetInstance<SkylandViewModel>();
        public LocalViewModel Local => ServiceLocator.Current.GetInstance<LocalViewModel>();
        public SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();
    }
}
