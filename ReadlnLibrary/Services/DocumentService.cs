﻿using Newtonsoft.Json;

using ReadlnLibrary.Core.Models;
using ReadlnLibrary.Dialogs;
using ReadlnLibrary.Helpers;
using ReadlnLibrary.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReadlnLibrary.Services
{
    public class DocumentService
    {
        public async Task<bool> FillDocumentDataAsync(RdlnDocument doc)
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
                doc.Title = dialog.Fields.FirstOrDefault(f => f.Label == "Title")?.Value;
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


        public bool FillDocumentDataByPattern(RdlnDocument doc, string pattern, string delimiter)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var fillResult = false;
            var categories = DatabaseManager.Connection.Table<RdlnCategory>().ToList();

            var name = Path.GetFileNameWithoutExtension(doc.Name);

            var fields = NameHelper.GetFieldsByPattern(name, pattern, delimiter);

            if (fields != null)
            {
                if (!fields.ContainsKey(Constants.GroupCategories.TITLE))
                {
                    fields.Add(Constants.GroupCategories.TITLE, doc.Title);
                }
                if (!fields.ContainsKey(Constants.GroupCategories.AUTHOR))
                {
                    fields.Add(Constants.GroupCategories.AUTHOR, doc.Author);
                }
            }

            doc.RawFields = JsonConvert.SerializeObject(fields);

            if (!fields.ContainsKey("Category"))
            {
                doc.Category = "Default";
            }
            else
            {
                doc.Category = fields["Category"];
            }

            if (fields.TryGetValue("Title", out string title))
            {
                doc.Title = title;
            }

            if (fields.TryGetValue("Author", out string author))
            {
                doc.Author = author;
            }

            if (!categories.Any(c => c.Name == doc.Category))
            {
                var newCategory = new RdlnCategory
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = doc.Category
                };

                DatabaseManager.Connection.Insert(newCategory);


                foreach (var field in fields)
                {
                    var rdlnField = new RdlnField
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = field.Key,
                        Category = newCategory.Name,
                        Type = Constants.FieldType.String
                    };

                    DatabaseManager.Connection.Insert(rdlnField);
                }

                fillResult = true;
            }
            return fillResult;
        }

        public IEnumerable<RdlnField> GetFields()
        {
            var rdlnfields = DatabaseManager.Connection.Table<RdlnField>().ToList();
            var fields = rdlnfields.GroupBy(f => f.Name).Select(list => list.FirstOrDefault());

            return fields;
        }
    }
}
