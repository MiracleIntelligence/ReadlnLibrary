using System;
using Windows.UI.Xaml.Controls;

namespace ReadlnLibrary.Dialogs
{
    public sealed partial class ContentDialogPattern : ContentDialog
    {
        public string Delimiter => TextBoxDelimiter.Text;
        public string Pattern => TextBoxPattern.Text;
        public ContentDialogPattern()
        {
            this.InitializeComponent();
            TextBoxPattern.TextChanged += OnTextBoxTextChanged;
            TextBoxDelimiter.TextChanged += OnTextBoxTextChanged;
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidatePattern();
        }

        private void ValidatePattern()
        {
            if (!String.IsNullOrEmpty(Delimiter) && !String.IsNullOrEmpty(Pattern))
            {
                PrimaryButtonText = "Confirm";
            }
            else
            {
                PrimaryButtonText = String.Empty;
            }
        }
    }
}
