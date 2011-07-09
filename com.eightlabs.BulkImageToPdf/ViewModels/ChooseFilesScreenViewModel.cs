using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.eightlabs.BulkImageToPdf.ViewModels
{
    public class ChooseFilesScreenViewModel : ScreenBaseViewModel
    {

        public override string ScreenName
        {
            get { return "Choose Files"; }
        }

        public ChooseFilesScreenViewModel(MainWindowViewModel vm) : base(vm) { }

    }
}
