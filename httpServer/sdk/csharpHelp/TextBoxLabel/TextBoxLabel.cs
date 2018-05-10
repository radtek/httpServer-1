using System;
using System.Collections.Generic;
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
	public class TextBoxLabel : TextBoxTip {
		static TextBoxLabel() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxLabel), new FrameworkPropertyMetadata(typeof(TextBoxLabel)));
		}

		//Content
		public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(TextBoxLabel), new PropertyMetadata(""));
		public string Content {
			get { return (string)GetValue(ContentProperty); }
			set { SetCurrentValue(ContentProperty, value); }
		}

		//Content color
		public static readonly DependencyProperty ContentColorProperty = DependencyProperty.Register("ContentColor", typeof(Brush), typeof(TextBoxLabel), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"))));
		public Brush ContentColor {
			get { return (Brush)GetValue(ContentColorProperty); }
			set { SetCurrentValue(ContentColorProperty, value); }
		}

		//Left Width
		public static readonly DependencyProperty LeftWidthProperty = DependencyProperty.Register("LeftWidth", typeof(Double), typeof(TextBoxLabel), new PropertyMetadata(Double.NaN));
		public Double LeftWidth {
			get { return (Double)GetValue(LeftWidthProperty); }
			set { SetCurrentValue(LeftWidthProperty, value); }
		}

		//Left Gap
		public static readonly DependencyProperty _LabelPaddingProperty = DependencyProperty.Register("_LabelPadding", typeof(Thickness), typeof(TextBoxLabel), new PropertyMetadata(new Thickness(5, 0, 8, 3)));
		public Thickness _LabelPadding {
			get { return (Thickness)GetValue(_LabelPaddingProperty); }
			set { SetCurrentValue(_LabelPaddingProperty, value); }
		}

		//Right Width
		public static readonly DependencyProperty RightWidthProperty = DependencyProperty.Register("RightWidth", typeof(Double), typeof(TextBoxLabel), new PropertyMetadata(Double.NaN));
		public Double RightWidth {
			get { return (Double)GetValue(RightWidthProperty); }
			//get { return ActualWidth - LeftWidth; }
			set { SetCurrentValue(RightWidthProperty, value); }
		}

		//is show tip
		//public static readonly DependencyProperty _ShowTipProperty = DependencyProperty.Register("_ShowTip", typeof(Visibility), typeof(TextBoxLabel), new PropertyMetadata(Visibility.Hidden));
		//public Visibility _ShowTip {
		//	get { return (Visibility)GetValue(_ShowTipProperty); }
		//	set { SetCurrentValue(_ShowTipProperty, value); }
		//}

	}
}
