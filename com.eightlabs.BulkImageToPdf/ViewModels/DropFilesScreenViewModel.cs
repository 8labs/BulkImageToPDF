using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.eightlabs.BulkImageToPdf.ViewModels
{
    public class DropFilesScreenViewModel : ScreenBaseViewModel
    {

        public override string ScreenName
        {
            get { return "Drop Files"; }
        }

        public DropFilesScreenViewModel(MainWindowViewModel vm) : base(vm) { }
    }
}
