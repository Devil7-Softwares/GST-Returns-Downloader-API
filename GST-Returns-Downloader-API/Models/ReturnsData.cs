using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using DevExpress.Mvvm;
using Devil7.Automation.GSTR.Downloader.Models;

namespace Devil7.Automation.GSTR.Downloader.Models
{
    public class ReturnOperation : ViewModelBase
    {
        #region Variables
        private string operationName;
        private bool value;
        public delegate CommandResult action(RestSharp.RestClient client, string monthValue);
        #endregion

        #region Properties
        public string OperationName
        {
            get => operationName;
            set => this.SetProperty(ref operationName, value, "OperationName");
        }
        public bool Value
        {
            get => value;
            set => this.SetProperty(ref this.value, value, "Value");
        }
        public action Action
        {
            get;
            set;
        }
        public bool Enabled
        {
            get => this.Action != null;
        }
        #endregion
    }
    public class FileType : ViewModelBase
    {
        #region Constructor
        public FileType()
        {
            this.PropertyChanged += FileType_PropertyChanged;
            this.CheckFiledStatus = false;
            this.SubmittedIsEnough = false;
        }
        #endregion

        #region Variables
        private string fileTypeName = "";
        private ObservableCollection<ReturnOperation> operations;
        #endregion

        #region Properties
        public string FileTypeName
        {
            get => fileTypeName;
            set => this.SetProperty(ref fileTypeName, value, "FileTypeName");
        }
        public ObservableCollection<ReturnOperation> Operations
        {
            get => operations;
            set
            {
                this.SetProperty(ref operations, value, "Operations");
            }
        }
        public bool CheckFiledStatus { get; set; }
        public bool SubmittedIsEnough { get; set; }
        #endregion

        #region Events
        private void FileType_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Operations))
            {
                foreach (ReturnOperation operation in this.Operations)
                {
                    operation.PropertyChanged += Operation_PropertyChanged;
                }
            }
        }

        private void Operation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReturnOperation returnOperation = sender as ReturnOperation;
            if (e.PropertyName == nameof(ReturnOperation.Value) && returnOperation != null)
            {
                if (sender is ReturnOperation && returnOperation.Value)
                {
                    foreach (ReturnOperation operation in this.Operations)
                    {
                        if (operation != returnOperation && operation.Value)
                            operation.Value = false;
                    }
                }
            }
        }
        #endregion
    }
    public class ReturnsData : ViewModelBase
    {
        #region Variables
        private string returnName = "";
        private ObservableCollection<FileType> fileTypes;
        #endregion

        #region Properties
        public string ReturnName
        {
            get => returnName;
            set => this.SetProperty(ref returnName, value, "ReturnName");
        }

        public ObservableCollection<FileType> FileTypes
        {
            get => fileTypes;
            set => this.SetProperty(ref fileTypes, value, "FileTypes");
        }
        #endregion
    }
}
