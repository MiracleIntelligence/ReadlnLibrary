using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ReadlnLibrary.Core.Models;
using ReadlnLibrary.Managers;
using ReadlnLibrary.Services;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace ReadlnLibrary.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private DocumentService _documentService;

        public ICommand AddFileCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }

        public ObservableCollection<RdlnDocument> Documents { get; private set; }
        public bool LibraryIsEmpty => !(Documents?.Count > 0);
        public MainViewModel(DocumentService documentService)
        {
            _documentService = documentService;
            AddFileCommand = new RelayCommand(AddFile);
            OpenFileCommand = new RelayCommand<RdlnDocument>(OpenFile);
        }

        private async void OpenFile(RdlnDocument doc)
        {
            if (doc != null && !String.IsNullOrEmpty(doc.Path))
            {
                try
                {
                    var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(doc.Token);
                    var success = await Windows.System.Launcher.LaunchFileAsync(file);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async void AddFile()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.CommitButtonText = "Open";
            openPicker.FileTypeFilter.Add("*");
            var file = await openPicker.PickSingleFileAsync();


            if (file != null)
            {
                string faToken = StorageApplicationPermissions.FutureAccessList.Add(file);
                var document = new RdlnDocument
                {
                    DocumentId = Guid.NewGuid().ToString(),
                    Path = file.Path,
                    Name = file.Name,
                    Title = file.DisplayName,
                    Token = faToken
                };

                var doc = await _documentService.FillDocumentData(document);

                var count = DatabaseManager.Connection.Insert(doc);

                if (count > 0)
                {
                    Documents.Add(document);
                    RaisePropertyChanged(nameof(LibraryIsEmpty));
                }


                // Launch the retrieved file
                //var success = await Windows.System.Launcher.LaunchFileAsync(file);

                //if (success)
                //{
                //    // File launched
                //}
                //else
                //{
                //    // File launch failed
                //}
            }
            else
            {
                // Could not find file
            }
        }

        internal void Initialize()
        {
            DatabaseManager.InitConnection();

            var docs = DatabaseManager.Connection.Table<RdlnDocument>().ToList();
            Documents = new ObservableCollection<RdlnDocument>(docs);

            RaisePropertyChanged(nameof(Documents));
            RaisePropertyChanged(nameof(LibraryIsEmpty));
        }
    }
}
