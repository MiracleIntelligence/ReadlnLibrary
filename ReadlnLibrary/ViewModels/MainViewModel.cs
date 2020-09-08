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
using ReadlnLibrary.Views;
using System.IO;
using Windows.UI.Popups;
using ReadlnLibrary.Dialogs;
using System.Collections.ObjectModel;
using System.Linq;

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
        public ICommand SettingsCommand { get; private set; }
        public ObservableCollection<string> Tokens { get; set; }
        public ObservableCollection<string> Categories { get; set; }
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
            SettingsCommand = new RelayCommand(GoToSettings);

            Tokens = new ObservableCollection<string>();

            Categories = new ObservableCollection<string>();
        }

        public event EventHandler<RdlnDocument> DocumentAdded;

        private void GoToSettings()
        {
            ViewModelLocator.Current.NavigationService.Navigate(typeof(SettingsViewModel).FullName);
        }

        public async Task UpdateFilters()
        {
            await UpdateDocuments(Tokens).ConfigureAwait(false);
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

            if (String.IsNullOrEmpty(order))
            {
                groups = new GroupedObservableCollection<string, RdlnDocument>(d => { return d.Category == null ? string.Empty : d.Category; }, documents);
            }
            else
            {
                groups = new GroupedObservableCollection<string, RdlnDocument>(d => { return (d.GetRawFieldValue(order) ?? string.Empty).ToUpperInvariant(); }, documents);
            }
            //switch (order)
            //{
            //    case Constants.GroupCategories.AUTHOR:
            //        groups = new GroupedObservableCollection<string, RdlnDocument>(d => { return d.Author == null ? string.Empty : d.Author; }, documents);
            //        break;
            //    case Constants.GroupCategories.CATEGORY:
            //        groups = new GroupedObservableCollection<string, RdlnDocument>(d => { return d.Category == null ? string.Empty : d.Category; }, documents);
            //        break;
            //    default:
            //        groups = new GroupedObservableCollection<string, RdlnDocument>(d => { return d.Title == null ? String.Empty : d.Title[0].ToString(CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture); }, documents);
            //        break;
            //}
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
                catch (FileNotFoundException ex)
                {
                    var dialog = new MessageDialog("File cannot be opened. It was moved or deleted.", "File not found");
                    await dialog.ShowAsync();
                    Debug.WriteLine(ex);
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


            await AddFileAsync(file);
        }

        private async void EditDoc(RdlnDocument document)
        {
            if (document != null)
            {
                try
                {
                    var documentWasChanged = await _documentService.FillDocumentDataAsync(document).ConfigureAwait(true);

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

        internal async Task Initialize()
        {
            DatabaseManager.InitConnection();

            await UpdateDocuments().ConfigureAwait(false);
        }

        internal async Task UpdateDocuments(IEnumerable<string> filters = null)
        {
            var docs = DatabaseManager.Connection.Table<RdlnDocument>().ToList();

            if (filters != null && filters.Any())
            {
                docs = docs.Where(d => filters.Contains(d.Category)).ToList();
            }

            var group = Constants.GroupCategories.CATEGORY;
            try
            {
                group = await ApplicationData.Current.LocalFolder.ReadAsync<string>(Constants.Settings.ORDER).ConfigureAwait(true);

                Categories.Clear();
                var categories = DatabaseManager.Connection.Table<RdlnCategory>().ToList();
                foreach (var category in categories)
                {
                    Categories.Add(category.Name);
                }

            }
            finally
            {
                GroupedDocuments = GetGrouped(docs, group);

                RaisePropertyChanged(nameof(GroupedDocuments));
                RaisePropertyChanged(nameof(LibraryIsEmpty));
            }
        }

        internal async Task AddFiles(IReadOnlyList<IStorageItem> files)
        {
            try
            {
                if (files.Count > 0)
                {
                    string pattern = null;
                    string delimiter = null;

                    var dialog = new ContentDialogPattern();
                    var dialogResult = await dialog.ShowAsync();
                    if (dialogResult == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                    {
                        pattern = dialog.Pattern;
                        delimiter = dialog.Delimiter;
                    }

                    foreach (var file in files)
                    {
                        if (String.IsNullOrEmpty(pattern))
                        {
                            await AddFileAsync(file).ConfigureAwait(false);
                        }
                        else
                        {
                            AddFileByPattern(file, pattern, delimiter);
                        }
                    }
                }
            }
            catch
            {

            }
        }


        internal async Task AddFileAsync(IStorageItem file)
        {
            if (file != null)
            {
                string faToken = StorageApplicationPermissions.FutureAccessList.Add(file);
                var document = new RdlnDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Path = file.Path,
                    Name = file.Name,
                    Title = file.Name,
                    Token = faToken
                };

                var documentWasChanged = await _documentService.FillDocumentDataAsync(document).ConfigureAwait(true);

                if (documentWasChanged)
                {
                    var count = DatabaseManager.Connection.Insert(document);

                    if (count > 0)
                    {
                        GroupedDocuments.Add(document);

                        DocumentAdded?.Invoke(this, document);

                        RaisePropertyChanged(nameof(LibraryIsEmpty));
                    }
                }
            }
        }

        internal void AddFileByPattern(IStorageItem file, string pattern, string delimiter)
        {
            if (file != null)
            {
                string faToken = StorageApplicationPermissions.FutureAccessList.Add(file);
                var document = new RdlnDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Path = file.Path,
                    Name = file.Name,
                    Title = file.Name,
                    Token = faToken
                };
                try
                {
                    //var pattern = await ApplicationData.Current.LocalFolder.ReadAsync<string>(Constants.Settings.PATTERN).ConfigureAwait(true);


                    var documentWasChanged = _documentService.FillDocumentDataByPattern(document, pattern, delimiter);

                    //if (documentWasChanged)
                    {
                        var count = DatabaseManager.Connection.Insert(document);

                        if (count > 0)
                        {
                            GroupedDocuments.Add(document);

                            DocumentAdded?.Invoke(this, document);

                            RaisePropertyChanged(nameof(LibraryIsEmpty));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

            }
        }
    }
}
