using System;
using System.Threading.Tasks;

using ReadlnLibrary.Core.Models;
using ReadlnLibrary.Dialogs;

namespace ReadlnLibrary.Services
{
    public class DocumentService
    {
        public async Task<bool> FillDocumentData(RdlnDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var fillResult = false;

            var dialog = new AddDocumentDialog();
            dialog.Init(doc);
            var result = await dialog.ShowAsync();
            if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                doc.Title = dialog.DocumentTitle;
                doc.Author = dialog.DocumentAuthor;
                doc.Category = dialog.DocumentCategory;

                fillResult = true;
            }
            return fillResult;
        }
    }
}
