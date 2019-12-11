using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadlnLibrary.Services;
using ReadlnLibrary.ViewModels;
using Windows.ApplicationModel.Activation;

namespace ReadlnLibrary.Activation
{
    internal class FileActivationHandler : ActivationHandler<FileActivatedEventArgs>
    {
        public NavigationServiceEx NavigationService => ViewModelLocator.Current.NavigationService;

        protected override async Task HandleInternalAsync(FileActivatedEventArgs args)
        {
            // Create data from activation Uri in ProtocolActivatedEventArgs
            //var dic = new Dictionary<string, string>();
            //for (int i = 0; i < args.Files.Count; ++i)
            //{
            //    var file = args.Files[i];
            //    dic.Add("file" + i, args.Files[i].Path);
            //    var data = new SchemeActivationData()
            //}
            if (args.Files.Count > 0)
            {
                NavigationService.Navigate(typeof(MainViewModel).FullName, args.Files);
            }

            await Task.CompletedTask;
        }
    }
}
