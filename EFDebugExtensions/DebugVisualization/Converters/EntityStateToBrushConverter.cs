using System;
using System.Data.Entity;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EntityFramework.Debug.DebugVisualization.Converters
{
    [ValueConversion(typeof(EntityState), typeof(Color))]
    public class EntityStateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is EntityState))
                throw new ArgumentException("value should be of type EntityState");

            var state = (EntityState)value;
            switch (state)
            {
                case EntityState.Unchanged:
                    return Brushes.LightGray;
                case EntityState.Added:
                    return Brushes.PaleGreen;
                case EntityState.Deleted:
                    return Brushes.LightCoral;
                case EntityState.Modified:
                    return Brushes.LightGoldenrodYellow;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}