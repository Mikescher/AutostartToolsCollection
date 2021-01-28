using ATC.UI;
using MSHC.WPF.MVVM;
using System;
using System.Windows.Threading;

namespace ATC
{
    public class MainWindowViewmodel: ObservableObject
    {
        private readonly DispatcherTimer _timer;

        public RangeEnabledObservableCollection<ATCTaskProxy> Tasks { get; } = new RangeEnabledObservableCollection<ATCTaskProxy>();

        private int _closeProgress = 0;
        public int CloseProgress { get { return _closeProgress; } set { _closeProgress = value; OnPropertyChanged(); } }

        private ATCTaskProxy _selectedTask = null;
        public ATCTaskProxy SelectedTask { get { return _selectedTask; } set { _selectedTask = value; OnPropertyChanged(); } }

        public MainWindowViewmodel()
        {
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(750), DispatcherPriority.Normal, TimerTick, App.Current.Dispatcher);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            foreach (var t in Tasks) t.TriggerTimeChanged();
        }
    }
}
