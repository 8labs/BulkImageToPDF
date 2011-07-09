using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.eightlabs.BulkImageToPdf.ViewModels
{
    public class ConversionOptionsScreenViewModel : ScreenBaseViewModel
    {

        public override string ScreenName
        {
            get { return "Conversion Options"; }
        }

        public ConversionOptionsScreenViewModel(MainWindowViewModel vm) : base(vm) { }

    }
}
