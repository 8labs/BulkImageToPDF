using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.eightlabs.WPFCommon.ViewModels;

namespace com.eightlabs.BulkImageToPdf.ViewModels
{
    /// <summary>
    /// Base view model for displaying ordered screens  (also works as a breadcrumb style thing)
    /// </summary>
    public abstract class ScreenBaseViewModel : ViewModelBase
    {
        public abstract string ScreenName { get; }

        public MainWindowViewModel Main { get; set; }

        public ScreenBaseViewModel(MainWindowViewModel mainvm)
        {
            this.Main = mainvm;
        }

    }
}
