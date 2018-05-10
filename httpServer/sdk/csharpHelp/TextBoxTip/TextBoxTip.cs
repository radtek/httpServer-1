using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace csharpHelp.ui {
	/// <summary></summary>
	public class TextBoxTip : TextBox {
		static TextBoxTip() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxTip), new FrameworkPropertyMetadata(typeof(TextBoxTip)));
		}

		public TextBoxTip() : base() {
			TextChanged += (object sender, TextChangedEventArgs e) => {
				if (e.OriginalSource != this) {
					e.Handled = true;
				}
			};
		}

		//tip
		public static readonly DependencyProperty TipProperty = DependencyProperty.Register("Tip", typeof(string), typeof(TextBoxTip), new PropertyMetadata(""));
		public string Tip {
			get { return (string)GetValue(TipProperty); }
			set { SetCurrentValue(TipProperty, value); }
		}

		//tip color
		public static readonly DependencyProperty TipColorProperty = DependencyProperty.Register("TipColor", typeof(Brush), typeof(TextBoxTip), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8F8F8F"))));
		public Brush TipColor {
			get { return (Brush)GetValue(TipColorProperty); }
			set { SetCurrentValue(TipColorProperty, value); }
		}

		//is show tip
		public static readonly DependencyProperty _ShowTipProperty = DependencyProperty.Register("_ShowTip", typeof(Visibility), typeof(TextBoxTip), new PropertyMetadata(Visibility.Hidden));
		public Visibility _ShowTip {
			get { return (Visibility)GetValue(_ShowTipProperty); }
			set { SetCurrentValue(_ShowTipProperty, value); }
		}

		//public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
		//	return (string)value == "True";
		//}

		//public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
		//	//bool? nb = (bool)value;
		//	return (bool)value == true ? "True" : "False";
		//}

	}
}
