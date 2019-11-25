using System;
using System.Linq;
using System.Threading.Tasks;

using ReadlnLibrary.Core.Models;
using ReadlnLibrary.Dialogs;
using ReadlnLibrary.Managers;

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

            var categories = DatabaseManager.Connection.Table<RdlnCategory>().ToList();


            dialog.Init(doc, categories);

            var result = await dialog.ShowAsync();
            if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                doc.Title = dialog.DocumentTitle;
                doc.Author = dialog.DocumentAuthor;
                doc.Category = dialog.DocumentCategory;

                if (!categories.Any(c => c.Name == doc.Category))
                {
                    var newCategory = new RdlnCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = doc.Category
                    };

                    DatabaseManager.Connection.Insert(newCategory);
                }

                fillResult = true;
            }
            return fillResult;
        }
    }
}
