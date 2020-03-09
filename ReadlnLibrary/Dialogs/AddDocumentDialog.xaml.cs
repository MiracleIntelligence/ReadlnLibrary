using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using ReadlnLibrary.Core.Models;
using ReadlnLibrary.Managers;
using ReadlnLibrary.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ReadlnLibrary.Dialogs
{
    public sealed partial class AddDocumentDialog : ContentDialog
    {
        private List<RdlnCategory> _categories;

        public string DocumentCategory
        {
            get { return (string)GetValue(DocumentCategoryProperty); }
            set { SetValue(DocumentCategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DocumentCategory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentCategoryProperty =
            DependencyProperty.Register("DocumentCategory", typeof(string), typeof(AddDocumentDialog), new PropertyMetadata(0));


        public ObservableCollection<Field> Fields { get; private set; }
        private Dictionary<string, string> _knownValues;

        public AddDocumentDialog()
        {
            Fields = new ObservableCollection<Field>();

            this.InitializeComponent();
        }

        internal void Init(RdlnDocument doc, List<RdlnCategory> categories)
        {
            if (String.IsNullOrEmpty(doc.RawFields))
            {
                _knownValues = new Dictionary<string, string>();
            }
            else
            {
                _knownValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(doc.RawFields);
            }

            _categories = categories;

            DocumentCategory = doc.Category;

            UpdateFields(DocumentCategory);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing,
            // otherwise assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //Set the ItemsSource to be your filtered dataset
                //sender.ItemsSource = dataset;
                var hints = _categories?.Where(c => c.Name != null && c.Name.Contains(sender.Text, StringComparison.InvariantCultureIgnoreCase)).Select(c => c.Name).ToList();
                sender.ItemsSource = hints;
            }

            var canAddProp = String.IsNullOrEmpty(DocumentCategory) || (_categories != null && !_categories.Any(c => c.Name.Equals(DocumentCategory, StringComparison.InvariantCultureIgnoreCase)));
            ButtonAddField.Visibility = canAddProp ? Visibility.Visible : Visibility.Collapsed;
        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            ASBCategory.Text = (args.SelectedItem as string);
            UpdateFields(ASBCategory.Text);
        }


        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else
            {
                // Use args.QueryText to determine what to do.
            }
        }

        private void UpdateFields(string categoryName)
        {
            List<Field> fields;
            if (!String.IsNullOrEmpty(categoryName))
            {
                var category = DatabaseManager.Connection.Get<RdlnCategory>(c => c.Name == categoryName);
                if (category != null)
                {
                    var rdlnfields = DatabaseManager.Connection.Table<RdlnField>().Where(f => f.Category == categoryName).ToList();
                    fields = rdlnfields.Select(r => new Field(r.Name, _knownValues.ContainsKey(r.Name) ? _knownValues[r.Name] : null)).ToList();

                    Fields.Clear();
                    foreach (var f in fields)
                    {
                        Fields.Add(f);
                    }

                    return;
                }
            }

            fields = new List<Field>()
            {
                new Field
                {
                    Label = "Title",
                    Value = _knownValues["Title"]
                },
                new Field
                {
                    Label = "Author",
                    Value = _knownValues["Author"]
                }
                };
            foreach (var f in fields)
            {
                Fields.Add(f);
            }
        }

        private void OnButtonAddFieldClick(object sender, RoutedEventArgs e)
        {
            Fields.Add(new Field());
        }
    }
}
