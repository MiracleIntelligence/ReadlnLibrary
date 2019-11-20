using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using ReadlnLibrary.Core.Collections;
using ReadlnLibrary.Core.Models;
using ReadlnLibrary.Managers;
using ReadlnLibrary.Services;
using ReadlnLibrary.Helpers;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReadlnLibrary.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private DocumentService _documentService;

        public ICommand AddFileCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }
        public ICommand RemoveDocCommand { get; private set; }
        public ICommand EditDocCommand { get; private set; }
        public ICommand SetGroupCommand { get; private set; }

        public GroupedObservableCollection<string, RdlnDocument> GroupedDocuments { get; private set; }
        public bool LibraryIsEmpty => !(GroupedDocuments?.Count > 0);
        public MainViewModel(DocumentService documentService)
        {
            _documentService = documentService;

            AddFileCommand = new RelayCommand(AddFile);
            OpenFileCommand = new RelayCommand<RdlnDocument>(OpenFile);
            RemoveDocCommand = new RelayCommand<RdlnDocument>(RemoveDoc);
            EditDocCommand = new RelayCommand<RdlnDocument>(EditDoc);
            SetGroupCommand = new RelayCommand<string>(SetGroup);
        }

        private async void SetGroup(string order)
        {
            var currentOrder = await ApplicationData.Current.LocalFolder.ReadAsync<string>(Constants.Settings.ORDER).ConfigureAwait(true);
            if (currentOrder != order)
            {
                var documents = GroupedDocuments.EnumerateItems();
                GroupedDocuments = GetGrouped(documents, order);

                RaisePropertyChanged(nameof(GroupedDocuments));

                await ApplicationData.Current.LocalFolder.SaveAsync(Constants.Settings.ORDER, order).ConfigureAwait(false);
            }
        }

        private GroupedObservableCollection<string, RdlnDocument> GetGrouped(IEnumerable<RdlnDocument> documents, string order)
        {
            GroupedObservableCollection<string, RdlnDocument> groups = null;
            switch (order)
            {
                case Constants.GroupCategories.AUTHOR:
                    groups = new GroupedObservableCollection<string, RdlnDocument>(d => { return d.Author == null ? string.Empty : d.Author; }, documents);
                    break;
                case Constants.GroupCategories.CATEGORY:
                    groups = new GroupedObservableCollection<string, RdlnDocument>(d => { return d.Category == null ? string.Empty : d.Category; }, documents);
                    break;
                default:
                    groups = new GroupedObservableCollection<string, RdlnDocument>(d => { return d.Title == null ? String.Empty : d.Title[0].ToString(CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture); }, documents);
                    break;
            }
            return groups;
        }

        private void RemoveDoc(RdlnDocument doc)
        {
            if (doc != null)
            {
                try
                {
                    var result = DatabaseManager.Connection.Delete(doc);
                    if (result != 0)
                    {
                        GroupedDocuments.Remove(doc);
                    }

                    RaisePropertyChanged(nameof(LibraryIsEmpty));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
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
                    Debug.WriteLine(ex);
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
                    Id = Guid.NewGuid().ToString(),
                    Path = file.Path,
                    Name = file.Name,
                    Title = file.DisplayName,
                    Token = faToken
                };

                var doumentWasChangef = await _documentService.FillDocumentData(document).ConfigureAwait(true);

                var count = DatabaseManager.Connection.Insert(document);

                if (count > 0)
                {
                    GroupedDocuments.Add(document);
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

        private async void EditDoc(RdlnDocument document)
        {
            if (document != null)
            {
                try
                {
                    var documentWasChanged = await _documentService.FillDocumentData(document).ConfigureAwait(true);

                    if (documentWasChanged)
                    {
                        GroupedDocuments.Remove(document);
                        DatabaseManager.Connection.Update(document);
                        GroupedDocuments.Add(document);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        internal async void Initialize()
        {
            DatabaseManager.InitConnection();

            var docs = DatabaseManager.Connection.Table<RdlnDocument>().ToList();
            //GroupedDocuments = new GroupedObservableCollection<string, RdlnDocument>(d => { return String.IsNullOrEmpty(d.Title) ? String.Empty : d.Title[0].ToString(CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture); }, docs);

            var group = await ApplicationData.Current.LocalFolder.ReadAsync<string>(Constants.Settings.ORDER).ConfigureAwait(true);
            GroupedDocuments = GetGrouped(docs, group);

            RaisePropertyChanged(nameof(GroupedDocuments));
            RaisePropertyChanged(nameof(LibraryIsEmpty));
        }
    }
}
