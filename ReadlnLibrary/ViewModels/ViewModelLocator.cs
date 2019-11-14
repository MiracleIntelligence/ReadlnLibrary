using System;

using GalaSoft.MvvmLight.Ioc;

using ReadlnLibrary.Services;
using ReadlnLibrary.Views;

namespace ReadlnLibrary.ViewModels
{
    [Windows.UI.Xaml.Data.Bindable]
    public class ViewModelLocator
    {
        private static ViewModelLocator _current;

        public static ViewModelLocator Current => _current ?? (_current = new ViewModelLocator());

        private ViewModelLocator()
        {
            if (SimpleIoc.Default.IsRegistered<NavigationServiceEx>())
            {
                return;
            }

            SimpleIoc.Default.Register(() => new NavigationServiceEx());
            SimpleIoc.Default.Register<DocumentService>();
            Register<MainViewModel, MainPage>();
            Register<SettingsViewModel, SettingsPage>();
            Register<ShareTargetViewModel, ShareTargetPage>();
            Register<SchemeActivationSampleViewModel, SchemeActivationSamplePage>();
        }

        public SchemeActivationSampleViewModel SchemeActivationSampleViewModel => SimpleIoc.Default.GetInstance<SchemeActivationSampleViewModel>();

        public ShareTargetViewModel ShareTargetViewModel => SimpleIoc.Default.GetInstance<ShareTargetViewModel>();

        public SettingsViewModel SettingsViewModel => SimpleIoc.Default.GetInstance<SettingsViewModel>();

        public MainViewModel MainViewModel => SimpleIoc.Default.GetInstance<MainViewModel>();

        public NavigationServiceEx NavigationService => SimpleIoc.Default.GetInstance<NavigationServiceEx>();

        public void Register<VM, V>()
            where VM : class
        {
            SimpleIoc.Default.Register<VM>();

            NavigationService.Configure(typeof(VM).FullName, typeof(V));
        }
    }
}
