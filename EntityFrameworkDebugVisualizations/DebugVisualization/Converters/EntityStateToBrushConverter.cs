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

            // http://colorschemedesigner.com/#1t52KccK-w0w0

            var state = (EntityState)value;
            switch (state)
            {
                case EntityState.Unchanged:
                    return new BrushConverter().ConvertFrom("#EEEEEE");
                case EntityState.Added:
                    return new BrushConverter().ConvertFrom("#B4F3AE");
                case EntityState.Deleted:
                    return new BrushConverter().ConvertFrom("#FFC3B6");
                case EntityState.Modified:
                    return new BrushConverter().ConvertFrom("#FFF4B6");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(EntityState), typeof(Color))]
    public class RelationStateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is EntityState))
                throw new ArgumentException("value should be of type EntityState");

            var state = (EntityState)value;
            switch (state)
            {
                case EntityState.Unchanged:
                    return new BrushConverter().ConvertFrom("#777777");
                case EntityState.Added:
                    return new BrushConverter().ConvertFrom("#39972F");
                case EntityState.Deleted:
                    return new BrushConverter().ConvertFrom("#A64733");
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