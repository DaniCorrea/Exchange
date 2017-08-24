namespace Exchange.ViewModels
{
    using GalaSoft.MvvmLight.Command;
    using Models;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using System;
    using System.ComponentModel;
    using System.Net.Http;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using Xamarin.Forms;

    public class MainViewModel : INotifyPropertyChanged
    {

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Attributes
        bool _convIsEnabled;
        bool _cleanIsEnabled;
        bool _isRunning;
        string _amount;
        string _result;
        Rate _sourceRate;
        Rate _targetRate;
        ObservableCollection<Rate> _rates;
        #endregion

        #region Properties
        public string Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Amount)));
                }
            }
        }
        public ObservableCollection<Rate> Rates
        {
            get
            {
                return _rates;
            }
            set
            {
                if (_rates != value)
                {
                    _rates = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rates)));
                }
            }
        }
        public Rate SourceRate
        {
            get {
                return _sourceRate;
            }
            set {
                if (_sourceRate != value)
                {
                    _sourceRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceRate)));
                }
            }
        }
        public Rate TargetRate
        {
            get {
                return _targetRate;
            }
            set
            {

                if (_targetRate != value)
                {
                    _targetRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetRate)));
                }
            }
        }
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                if (_isRunning!=value)
                {
                    _isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                }
            }
        }
        public bool ConvIsEnabled
        {
            get
            {
                return _convIsEnabled;
            }
            set
            {
                if (_convIsEnabled != value)
                {
                    _convIsEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConvIsEnabled)));
                }
            }
        }
        public bool CleanIsEnabled
        {
            get
            {
                return _cleanIsEnabled;
            }
            set
            {
                if (_cleanIsEnabled != value)
                {
                    _cleanIsEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CleanIsEnabled)));
                }
            }
        }
        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                if (_result != value)
                {
                    _result = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Result)));
                }
            }
        }
        #endregion

        #region Commands
        public ICommand ConvertCommand
        {
            get {
                return new RelayCommand(Convert);
            }
        }

        async void Convert()
        {
            if (string.IsNullOrEmpty(Amount))
            {
                await Application.Current.MainPage.DisplayAlert("Error","Please enter an amount","Accept");
                return;
            }

            decimal amount;
            if (!decimal.TryParse(Amount,out amount))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter a numeric value in amount", "Accept");
                return;
            }

            if (SourceRate == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please select a source rate", "Accept");
                return;
            }
            if (TargetRate == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please select a target rate", "Accept");
                return;
            }
            if (SourceRate==TargetRate) {
                await Application.Current.MainPage.DisplayAlert("Error", "Please select a different target rate", "Accept");
                return;
            }
            var amountConverted = amount/(decimal)SourceRate.TaxRate*
                                         (decimal)TargetRate.TaxRate;

            Result = string.Format("{0:C2} {1} = \n{2:C2} {3}",
                amount,
                SourceRate.Name,
                amountConverted,
                TargetRate.Name);
                CleanIsEnabled = true;
        }

        public ICommand SwitchCommand
        {
            get
            {
                return new RelayCommand(_Switch);
            }
        }

        private void _Switch()
        {
            var aux = SourceRate;
            SourceRate = TargetRate;
            TargetRate = aux;
            Convert();
        }

        public ICommand CleanCommand
        {
            get
            {
                return new RelayCommand(Clean);
            }
        }

        private void Clean()
        {
            Result = "Ready to convert!";
            SourceRate = null;
            TargetRate = null;
            Amount = null;
            CleanIsEnabled = false;
        }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            LoadRates();
        }
        #endregion
        
        #region Methods
        async void LoadRates()
        {
            IsRunning = true;
            Result = "Loading Rates...";
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://apiexchangerates.azurewebsites.net");
                var controller = "/api/Rates";
                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    IsRunning = false;
                    Result = result;
                }

                var rates = JsonConvert.DeserializeObject<List<Rate>>(result);
                Rates = new ObservableCollection<Rate>(rates);
                ConvIsEnabled = true;
                IsRunning = false;
                Result = "Ready to convert!";
            }
            catch(Exception ex)
            {
                IsRunning = false;
                Result = ex.Message;
            }
        }
        #endregion
    }
}
