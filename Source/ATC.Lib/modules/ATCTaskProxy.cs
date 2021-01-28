using MSHC.Util.Helper;
using MSHC.WPF.MVVM;
using System;

namespace ATC.Lib
{
    public enum ProxyState
    {
        Waiting,
        Running,
        Success,
        Errored,
    }

    public class ATCTaskProxy: ObservableObject
    {
        private DateTimeOffset _startTime = DateTimeOffset.MinValue;
        private DateTimeOffset _endTime   = DateTimeOffset.MinValue;

        private string _timeCache = null;
        public string Time 
        {
            get
            {
                if (_startTime == DateTimeOffset.MinValue)
                {
                    return "--:--";
                }
                else
                {
                    if (_endTime == DateTimeOffset.MinValue)
                    {
                        return (DateTimeOffset.Now - _startTime).ToString(@"mm\:ss");
                    }
                    else
                    {
                        return (_endTime - _startTime).ToString(@"mm\:ss");
                    }
                }
            }
        }

        private ProxyState _state = ProxyState.Waiting;
        public ProxyState State { get { return _state; } set { _state = value; OnPropertyChanged(); } }

        private string _title= "";
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(); } }

        private readonly object _logLock = new object();

        private string _log = "";
        public string Log { get { return _log; } set { _log = value; OnPropertyChanged(); } }

        public readonly string Rootcat;
        public readonly string Subcat;

        public ATCTaskProxy(string title, string rootcat, Guid guid)
        {
            _title  = title;
            Rootcat = rootcat;
            Subcat  = guid.ToString("N");

            ATCLogger.AddListener(Rootcat + "::" + Subcat, this);
        }

        public void RegisterRoot()
        {
            ATCLogger.AddListener(string.Empty, this);
        }

        public ATCTaskProxy(string title, string logcat)
        {
            _title  = title;
            Rootcat = logcat;
            Subcat  = null;

            ATCLogger.AddListener(Rootcat, this);
        }

        public void AddLog(string text)
        {
            lock (_logLock) { _log = _log + text + "\n"; }
            DispatcherHelper.SmartInvoke(() => { OnExplicitPropertyChanged(nameof(Log)); });
        }

        public void TriggerTimeChanged()
        {
            if (_timeCache != Time) { _timeCache = Time; OnExplicitPropertyChanged(nameof(Time)); }
        }

        public void Start()
        {
            DispatcherHelper.SmartInvoke(() =>
            {
                if (State != ProxyState.Waiting) return;

                _startTime = DateTime.Now;
                State = ProxyState.Running;
            });
        }

        public void SetErrored()
        {
            DispatcherHelper.SmartInvoke(() =>
            {
                _endTime = DateTime.Now;
                State = ProxyState.Errored;
            });
        }

        public void FinishSuccess()
        {
            DispatcherHelper.SmartInvoke(() =>
            {
                if (State == ProxyState.Errored) return;

                _endTime = DateTime.Now;
                State = ProxyState.Success;
            });
        }
    }
}
