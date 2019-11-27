using System;

using ReadlnLibrary.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ReadlnLibrary.TemplateSelectors
{
    public class FieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NewFieldTemplate { get; set; }
        public DataTemplate FieldTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var field = item as Field;
            if (String.IsNullOrEmpty(field?.Label))
            {
                return NewFieldTemplate;
            }

            return FieldTemplate;
        }
    }
}
