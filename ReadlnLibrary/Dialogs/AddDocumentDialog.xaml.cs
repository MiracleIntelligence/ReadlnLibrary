using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ReadlnLibrary.Core.Models;
using ReadlnLibrary.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReadlnLibrary.Dialogs
{
    public sealed partial class AddDocumentDialog : ContentDialog
    {



        public string DocumentTitle
        {
            get { return (string)GetValue(DocumentTitleProperty); }
            set { SetValue(DocumentTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentTitleProperty =
            DependencyProperty.Register("DocumentTitle", typeof(string), typeof(AddDocumentDialog), new PropertyMetadata(0));
        private List<RdlnCategory> _categories;

        public string DocumentName
        {
            get { return (string)GetValue(DocumentNameProperty); }
            set { SetValue(DocumentNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DocumentName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentNameProperty =
            DependencyProperty.Register("DocumentName", typeof(string), typeof(AddDocumentDialog), new PropertyMetadata(0));




        public string DocumentAuthor
        {
            get { return (string)GetValue(DocumentAuthorProperty); }
            set { SetValue(DocumentAuthorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DocumentAuthor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentAuthorProperty =
            DependencyProperty.Register("DocumentAuthor", typeof(string), typeof(AddDocumentDialog), new PropertyMetadata(0));




        public string DocumentCategory
        {
            get { return (string)GetValue(DocumentCategoryProperty); }
            set { SetValue(DocumentCategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DocumentCategory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentCategoryProperty =
            DependencyProperty.Register("DocumentCategory", typeof(string), typeof(AddDocumentDialog), new PropertyMetadata(0));



        public AddDocumentDialog()
        {
            this.InitializeComponent();
        }

        internal void Init(RdlnDocument doc, List<RdlnCategory> categories)
        {
            _categories = categories;

            DocumentName = doc.Name;
            DocumentTitle = doc.Title;
            DocumentAuthor = doc.Author;
            DocumentCategory = doc.Category;


            ASBCategory.ItemsSource = _categories;
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
                sender.ItemsSource = _categories?.Where(c => c.Name.Contains(sender.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            ASBCategory.Text = (args.SelectedItem as RdlnCategory)?.Name;
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
    }
}
