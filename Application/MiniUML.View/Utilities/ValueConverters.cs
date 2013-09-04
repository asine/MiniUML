using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MiniUML.Framework;
using MiniUML.View.Controls;
using MiniUML.View.Views;

namespace MiniUML.View.Utilities
{
    // Converts a shape id into a shape control.
    public class ShapeIdToControlConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty ReferenceControlProperty = 
            DependencyProperty.Register("ReferenceControl", typeof(UIElement), typeof(ShapeIdToControlConverter), new FrameworkPropertyMetadata(null));

        public UIElement ReferenceControl
        {
            get { return (UIElement)GetValue(ReferenceControlProperty); }
            set { SetValue(ReferenceControlProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object o = convert(value, targetType, parameter, culture);
            if (o == null) o = AnchorPoint.InvalidSnapTarget;
            return o;
        }

        private object convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String shape = value as String;
            if (shape == null || shape == "" || ReferenceControl == null) return null;

            CanvasView cv = CanvasView.GetCanvasView(ReferenceControl);
            if (cv == null) return null;
            return cv.ControlFromElement(cv._CanvasViewModel._DocumentViewModel.dm_DocumentDataModel.GetShapeById(shape));
        }

        public object ConvertBack(object value, Type type, object parameter, CultureInfo culture)
        {
            UIElement control = value as UIElement;
            if (control == null) return "";

            CanvasView cv = CanvasView.GetCanvasView(control);
            return cv.ElementFromControl(control).GetStringAttribute("Id");
        }
    }

    // Converts between double-precision device independent uits and centimeters.
    [ValueConversion(typeof(double), typeof(String))]
    public class DiuToCentimetersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value / 96 * 2.54).ToString("F", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Double.Parse((String)value, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint, culture) / 2.54 * 96;
        }
    }
}
