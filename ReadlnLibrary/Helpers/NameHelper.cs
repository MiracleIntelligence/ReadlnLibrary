using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadlnLibrary.Helpers
{
    public static class NameHelper
    {
        //public const string PATTERN = @"Title,Author";
        //public const string DELIMITER = @"_";

        public static Dictionary<string, string> GetFieldsByPattern(string value, string pattern, string delimiter)
        {
            var result = new Dictionary<string, string>();
            var values = value.Split(delimiter);
            var properties = pattern.Split(delimiter);
            int i = 0;
            foreach (var property in properties)
            {
                if (value.Length > i)
                {
                    if (property.Contains("=", StringComparison.InvariantCulture))
                    {
                        var propPair = property.Split("=");
                        if (propPair.Length > 1)
                        {
                            result.Add(propPair[0], propPair[1]);
                            continue;
                        }
                    }
                    result.Add(property, values[i]);
                    i++;
                }
            }

            return result;
        }
    }
}
