using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadlnLibrary.Core.Models;
using ReadlnLibrary.Dialogs;

namespace ReadlnLibrary.Services
{
    public class DocumentService
    {
        public async Task<RdlnDocument> FillDocumentData(RdlnDocument doc)
        {
            var dialog = new AddDocumentDialog();
            dialog.Init(doc);
            var result = await dialog.ShowAsync();
            if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                doc.Title = dialog.DocumentTitle;
                doc.Author = dialog.DocumentAuthor;
            }
            return doc;
        }
    }
}
