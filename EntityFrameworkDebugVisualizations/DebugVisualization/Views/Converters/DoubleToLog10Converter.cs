﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace EntityFramework.Debug.DebugVisualization.Views.Converters
{
    public class DoubleToLog10Converter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double) value;
            return Math.Log10(val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double) value;
            return Math.Pow(10, val);
        }

        #endregion
    }
}
