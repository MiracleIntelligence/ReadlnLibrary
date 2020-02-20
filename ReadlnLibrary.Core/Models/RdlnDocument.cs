using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SQLite;

namespace ReadlnLibrary.Core.Models
{
    public class RdlnDocument
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Path { get; set; }
        public string Token { get; set; }
        public string Category { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string RawFields { get; set; }

        public string GetRawFieldValue(string order)
        {
            var fields = JsonConvert.DeserializeObject<Dictionary<string, string>>(RawFields);
            if (fields.ContainsKey(order))
            {
                return fields[order];
            }
            return null;
        }
    }
}
