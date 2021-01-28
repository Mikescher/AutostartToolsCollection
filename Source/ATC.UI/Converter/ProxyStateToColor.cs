using System;
using System.Windows;
using System.Windows.Media;
using ATC.Lib;
using MSHC.WPF.MVVM;

namespace ATC.UI.Converter
{
    public class ProxyStateToColor : OneWayConverter<ProxyState, Brush>
    {
        protected override Brush Convert(ProxyState value, object parameter)
        {
            switch (value)
            {
                case ProxyState.Waiting: return Brushes.LightGray;
                case ProxyState.Running: return Brushes.RoyalBlue;
                case ProxyState.Success: return Brushes.Chartreuse;
                case ProxyState.Errored: return Brushes.Crimson;
            }

            return Brushes.Magenta;
        }
    }
}
