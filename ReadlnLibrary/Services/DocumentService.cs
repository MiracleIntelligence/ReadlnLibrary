using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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


            if (doc.RawFields == null)
            {
                var rawFields = new Dictionary<string, string>();
                rawFields.Add("Title", doc.Title);
                rawFields.Add("Author", doc.Author);
                doc.RawFields = JsonConvert.SerializeObject(rawFields);
            }

            dialog.Init(doc, categories);

            var result = await dialog.ShowAsync();
            if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                doc.Title = dialog.Fields.FirstOrDefault(f=>f.Label == "Title")?.Value;
                doc.Author = dialog.Fields.FirstOrDefault(f => f.Label == "Author")?.Value;
                doc.Category = dialog.DocumentCategory;

                doc.RawFields = JsonConvert.SerializeObject(dialog.Fields.ToDictionary(f => f.Label, f => f.Value));

                if (!categories.Any(c => c.Name == doc.Category))
                {
                    var newCategory = new RdlnCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = doc.Category
                    };

                    DatabaseManager.Connection.Insert(newCategory);


                    foreach (var field in dialog.Fields)
                    {
                        var rdlnField = new RdlnField
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = field.Label,
                            Category = newCategory.Name,
                            Type = "string"
                        };

                        DatabaseManager.Connection.Insert(rdlnField);
                    }
                }

                fillResult = true;
            }
            return fillResult;
        }
    }
}
