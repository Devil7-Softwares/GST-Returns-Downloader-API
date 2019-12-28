using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Devil7.Automation.GSTR.Downloader.Models;
using ReactiveUI;

namespace Devil7.Automation.GSTR.Downloader.ViewModels
{
    public class ReturnOperation : ReactiveObject
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
            set => this.RaiseAndSetIfChanged(ref operationName, value);
        }
        public bool Value
        {
            get => value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
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
    public class FileType : ReactiveObject
    {
        #region Constructor
        public FileType()
        {
            this.PropertyChanged += FileType_PropertyChanged;
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
            set => this.RaiseAndSetIfChanged(ref fileTypeName, value);
        }
        public ObservableCollection<ReturnOperation> Operations
        {
            get => operations;
            set
            {
                this.RaiseAndSetIfChanged(ref operations, value);
            }
        }
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
    public class ReturnsData : ReactiveObject
    {
        #region Variables
        private string returnName = "";
        private bool checkFiledStatus = false;
        private ObservableCollection<FileType> fileTypes;
        #endregion

        #region Properties
        public string ReturnName
        {
            get => returnName;
            set => this.RaiseAndSetIfChanged(ref returnName, value);
        }

        public bool CheckFiledStatus { get => checkFiledStatus; set => this.RaiseAndSetIfChanged(ref checkFiledStatus, value); }
        public ObservableCollection<FileType> FileTypes
        {
            get => fileTypes;
            set => this.RaiseAndSetIfChanged(ref fileTypes, value);
        }
        #endregion
    }
}
