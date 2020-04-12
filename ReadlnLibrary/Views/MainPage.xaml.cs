using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.UI.Xaml.Controls;
using ReadlnLibrary.Core.Models;
using ReadlnLibrary.Services;
using ReadlnLibrary.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
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

            ViewModel.DocumentAdded += OnDocumentAdded;
        }

        private void OnDocumentAdded(object sender, RdlnDocument e)
        {
            SetGroupingButtons();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.Initialize();

            SetGroupingButtons();

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

        private void DefaultScreen_DragOver(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void DefaultScreen_Drop(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    //var storageFile = items[0] as StorageFile;
                    await ViewModel.AddFiles(items).ConfigureAwait(false);
                    //var bitmapImage = new BitmapImage();
                    //bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
                    //// Set the image on the main page to the dropped image
                    //Image.Source = bitmapImage;
                }
            }
        }

        private void SetGroupingButtons()
        {
            var categories = ViewModel.GroupedDocuments.EnumerateItems().Select(doc => doc.Category);
            var fields = SimpleIoc.Default.GetInstance<DocumentService>().GetFields();

            CommandBarTop.SecondaryCommands.Clear();

            CommandBarTop.SecondaryCommands.Add(new AppBarButton
            {
                Label = $"group by {Constants.GroupCategories.CATEGORY}",
                Command = ViewModel.SetGroupCommand
            });

            foreach (var field in fields)
            {
                if (field.Name.Equals(Constants.GroupCategories.CATEGORY, StringComparison.InvariantCulture))
                {
                    continue;
                }
                var button = new AppBarButton()
                {
                    Label = $"group by {field.Name}",
                    Command = ViewModel.SetGroupCommand,
                    CommandParameter = field.Name
                };

                CommandBarTop.SecondaryCommands.Add(button);
            }
        }

        private async void OnImageThumbnailLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var image = sender as Image;
            var doc = image.DataContext as RdlnDocument;
            if (doc != null)
            {
                try
                {
                    var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(doc.Token);
                    StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.DocumentsView, 30, ThumbnailOptions.UseCurrentScale);

                    var imageSource = new BitmapImage();
                    imageSource.SetSource(thumbnail);
                    image.Source = imageSource;
                }
                catch
                {
                    // We have placeholder image instead
                }
            }
        }
    }
}
