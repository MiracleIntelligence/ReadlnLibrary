using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using ReadlnLibrary.ViewModels;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ReadlnLibrary.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel
        {
            get { return ViewModelLocator.Current.MainViewModel; }
        }

        public MainPage()
        {
            InitializeComponent();
            ContentArea.DataContext = ViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.Initialize();
            if (e.Parameter is System.Collections.Generic.IReadOnlyList<Windows.Storage.IStorageItem>)
            {
                await ViewModel.AddFiles(e.Parameter as IReadOnlyList<IStorageItem>);
            }
        }

        private void OnElementIndexChanged(ItemsRepeater sender, ItemsRepeaterElementIndexChangedEventArgs args)
        {
            //var selectable = args.Element as ISelectable;
            //if (selectable != null)
            //{
            //    // Sync the ID we use to notify the selection model when the item
            //    // we represent has changed location in the data source.
            //    selectable.SelectionIndex = args.NewIndex;
            //}
        }

        private void Grid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var button = sender as Button;

            if (ViewModel.OpenFileCommand != null && ViewModel.OpenFileCommand.CanExecute(button.DataContext))
            {
                ViewModel.OpenFileCommand.Execute(button.DataContext);
            };
        }

        private void DocFlyoutButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DocFlyout.Hide();
        }
    }
}
