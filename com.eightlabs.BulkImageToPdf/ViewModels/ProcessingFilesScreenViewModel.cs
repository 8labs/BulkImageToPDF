using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.eightlabs.BulkImageToPdf.ViewModels
{
    public class ProcessingFilesScreenViewModel : ScreenBaseViewModel
    {

        public override string ScreenName
        {
            get { return "Processing Files"; }
        }

        public ProcessingFilesScreenViewModel(MainWindowViewModel vm) : base(vm) { }
    }
}
