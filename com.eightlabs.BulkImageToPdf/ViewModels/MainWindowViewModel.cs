using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.eightlabs.WPFCommon.ViewModels;
using System.Collections.ObjectModel;
using System.IO;

namespace com.eightlabs.BulkImageToPdf.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            FilesList = new ObservableCollection<FileInfo>();
        }

        #region Private Variables

        #endregion

        #region Public Variables

        public ObservableCollection<FileInfo> FilesList { get; set; }

        #endregion

        #region Private Methods

        #endregion

        #region Public Methods

        #endregion

        #region Commands

        #endregion
    }
}
