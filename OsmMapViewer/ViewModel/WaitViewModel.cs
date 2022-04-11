using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OsmMapViewer.Misc;

namespace OsmMapViewer.ViewModel
{
    public class WaitViewModel: ViewModelBase
    {

        private bool _WaitVisible = false;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public CancellationToken GetCancellationToken() {
            return cancellationTokenSource.Token;
        }
        public bool WaitVisible//видимая панель
        {
            get => _WaitVisible;
            set {
                if (!_WaitVisible && value)
                    cancellationTokenSource = new CancellationTokenSource();
                SetProperty(ref _WaitVisible, value);
            }
        }

        private string _WaitHeaderText = "Пожалуйста, подождите";
        public string WaitHeaderText
        {
            get => _WaitHeaderText;
            set => SetProperty(ref _WaitHeaderText, value);
        }
        private string _WaitText = "";
        public string WaitText
        {
            get => _WaitText;
            set => SetProperty(ref _WaitText, value);
        }


        private bool _WaitButtonCancelActivating = false;
        public bool WaitButtonCancelActivating//делаем неактивной кнопку отмена
        {
            get => _WaitButtonCancelActivating;
            set => SetProperty(ref _WaitButtonCancelActivating, value);
        }

        private bool _WaitButtonCancelVisible = false;
        public bool WaitButtonCancelVisible//делаем невидимой отмену
        {
            get => _WaitButtonCancelVisible;
            set => SetProperty(ref _WaitButtonCancelVisible, value);
        }

        public CancellationToken ShowWithCancel(string text ="",string title = "Пожалуйста, подождите")
        {
            WaitButtonCancelVisible = true;
            WaitButtonCancelActivating = false;
            WaitText = text;
            WaitHeaderText = title;
            WaitVisible = true;
            cancellationTokenSource = new CancellationTokenSource();
            return GetCancellationToken();
        }

        public void ShowWithoutCancel(string text = "", string title = "Пожалуйста, подождите") {
            WaitButtonCancelVisible = false;
            WaitButtonCancelActivating = false;
            WaitText = text;
            WaitHeaderText = title;
            WaitVisible = true;
        }

        private RelayCommand waitCancelButton;
        public RelayCommand WaitCancelButton {//Команда отмены
            get {
                return waitCancelButton ??
                       (waitCancelButton = new RelayCommand(obj =>
                       {
                           cancellationTokenSource.Cancel(false);
                           WaitButtonCancelActivating = true;
                       }));
            }
            set
            {
                waitCancelButton = value;
            }
        }
    }
}
