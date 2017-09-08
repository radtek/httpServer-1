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

namespace com.superscene.ui {
	/// <summary>
	/// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
	///
	/// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:supersceneDeploy.sdk.superscene.TextBoxTip"
	///
	///
	/// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:supersceneDeploy.sdk.superscene.TextBoxTip;assembly=supersceneDeploy.sdk.superscene.TextBoxTip"
	///
	/// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
	/// 并重新生成以避免编译错误: 
	///
	///     在解决方案资源管理器中右击目标项目，然后依次单击
	///     “添加引用”->“项目”->[浏览查找并选择此项目]
	///
	///
	/// 步骤 2)
	/// 继续操作并在 XAML 文件中使用控件。
	///
	///     <MyNamespace:TextBoxTip/>
	///
	/// </summary>
	public class TextBoxTip : TextBox {
		static TextBoxTip() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxTip), new FrameworkPropertyMetadata(typeof(TextBoxTip)));
		}

		//tip
		public static readonly DependencyProperty TipProperty = DependencyProperty.Register("Tip", typeof(string), typeof(TextBoxTip), new PropertyMetadata(""));
		public string Tip {
			get { return (string)GetValue(TipProperty); }
			set { SetValue(TipProperty, value); }
		}

		//tip color
		public static readonly DependencyProperty TipColorProperty = DependencyProperty.Register("TipColor", typeof(Brush), typeof(TextBoxTip), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8F8F8F"))));
		public Brush TipColor {
			get { return (Brush)GetValue(TipColorProperty); }
			set { SetValue(TipColorProperty, value); }
		}

		//is show tip
		public static readonly DependencyProperty _ShowTipProperty = DependencyProperty.Register("_ShowTip", typeof(Visibility), typeof(TextBoxTip), new PropertyMetadata(Visibility.Hidden));
		public Visibility _ShowTip {
			get { return (Visibility)GetValue(_ShowTipProperty); }
			set { SetValue(_ShowTipProperty, value); }
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
