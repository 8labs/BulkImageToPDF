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
            FilesList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(FilesList_CollectionChanged);
        }

        void FilesList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("FilesListIsEmpty");
        }

        #region Private Variables

        #endregion

        #region Public Variables

        public ObservableCollection<FileInfo> FilesList { get; set; }

        public Boolean FilesListIsEmpty {
            get
            {
                return FilesList.Count == 0;
            }
        }

        #endregion

        #region Private Methods

        #endregion

        #region Public Methods

        #endregion

        #region Commands

        #endregion
    }
}
