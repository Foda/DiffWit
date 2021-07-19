using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace DiffWit.Utils
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        private bool _isInverse = false;
        public bool IsInverse
        {
            get { return _isInverse; }
            set { _isInverse = value; }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool visibility = (bool)value;
            if (visibility != IsInverse)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
