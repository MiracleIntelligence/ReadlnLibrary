using System.Collections.Generic;

using Windows.ApplicationModel.DataTransfer;

namespace ReadlnLibrary.Models
{
    public class DragDropStartingData
    {
        public DataPackage Data { get; set; }

        public IList<object> Items { get; set; }
    }
}
