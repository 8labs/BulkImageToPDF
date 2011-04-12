using System;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace com.eightlabs.WPFCommon.Converters
{
    /// <summary>
    /// Default visibility converter (Supports false and true values)
    /// ex: <local:BoolToVisibilityConverter x:Key="InverseVisibility" TrueValue="Collapsed"  FalseValue="Visible" />
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : BoolToValueConverter<Visibility> { }

    /// <summary>
    /// Default ScrollBarVisibility converter (Supports false and true values)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(System.Windows.Controls.ScrollBarVisibility))]
    public class BoolToScrollBarVisibilityConverter : BoolToValueConverter<System.Windows.Controls.ScrollBarVisibility> { }

    /// <summary>
    /// Default bool string convertor (Supports false and true values)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToStringConverter : BoolToValueConverter<string> { }

    /// <summary>
    /// Inverts the bool value 
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolToInverseConverter : BoolToValueConverter<bool>
    {
        public BoolToInverseConverter()
        {
            this.FalseValue = true;
            this.TrueValue = false;
        }

    }

    /// <summary>
    /// Generic Bool to value convertor 
    /// Override to use in XAML
    /// </summary>
    /// <typeparam name="T">Type to convert the boolean value to</typeparam>
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }


}
