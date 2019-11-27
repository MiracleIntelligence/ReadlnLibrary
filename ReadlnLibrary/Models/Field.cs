using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadlnLibrary.Models
{
    public class Field
    {
        public Field()
        {

        }
        public Field(string label, string value = null)
        {
            Label = label;
            Value = value;
        }
        public string Label { get; set; }
        public string Value { get; set; }
    }
}
