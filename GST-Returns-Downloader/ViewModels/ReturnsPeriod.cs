using System.Collections.ObjectModel;
using ReactiveUI;

namespace Devil7.Automation.GSTR.Downloader.ViewModels
{
    public class MonthData : ViewModelBase
    {
        #region  Variables
        private string month = "";
        private string value = "";
        private bool isChecked = false;
        #endregion

        #region Constructor
        public MonthData(string month, string value)
        {
            this.Month = month;
            this.Value = value;
        }
        #endregion

        #region Properties
        public string Month { get => month; set => this.RaiseAndSetIfChanged(ref month, value); }
        public string Value { get => this.value; set => this.RaiseAndSetIfChanged(ref this.value, value); }
        public bool IsChecked { get => isChecked; set => this.RaiseAndSetIfChanged(ref isChecked, value); }
        #endregion
    }

    public class YearData : ViewModelBase
    {
        #region "Variables"
        private string year = "";
        private bool isChecked = false;
        private bool isChanging = false;
        private ObservableCollection<MonthData> months;
        #endregion

        #region Constructor
        public YearData(Models.Year yearData)
        {
            this.Year = yearData.year;
            this.Months = new ObservableCollection<MonthData>();

            foreach (Models.Month month in yearData.months)
            {
                MonthData monthData = new MonthData(month.month, month.value);
                monthData.PropertyChanged += this.Month_PropertyChanged;
                this.Months.Add(monthData);
            }
        }
        #endregion

        #region Properties
        public string Year { get => year; set => this.RaiseAndSetIfChanged(ref year, value); }
        public bool IsChecked
        {
            get
            {
                bool value = true;
                foreach (MonthData month in this.Months)
                    if (!month.IsChecked) value = false;
                return value;
            }
            set
            {
                if (this.isChecked != value)
                {
                    this.isChanging = true;
                    this.isChecked = value;
                    foreach (MonthData month in this.Months)
                        month.IsChecked = value;
                    this.RaisePropertyChanged("IsChecked");
                    this.isChanging = false;
                }
            }
        }
        public ObservableCollection<MonthData> Months { get => months; set => this.RaiseAndSetIfChanged(ref months, value); }
        #endregion

        #region Events
        private void Month_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!this.isChanging && e.PropertyName == nameof(MonthData.IsChecked))
            {
                bool value = true;
                foreach (MonthData month in this.Months)
                    if (!month.IsChecked) value = false;
                if (this.isChecked != value)
                {
                    this.isChecked = value;
                    this.RaisePropertyChanged("IsChecked");
                }
            }
        }
        #endregion
    }
}