using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadlnLibrary.Helpers
{
    public class NameHelper
    {
        public const string PATTERN = @"Title,Author";
        public const string DELIMITER = @"_";

        public static Dictionary<string, string> GetFieldsByPattern(string value, string pattern)
        {
            var result = new Dictionary<string, string>();
            var values = value.Split(DELIMITER);
            var properties = pattern.Split(DELIMITER);
            for (int i = 0; i < properties.Length; ++i)
            {
                if (value.Length > i)
                {
                    result.Add(properties[i], values[i]);
                }
            }

            return result;
        }
    }
}
