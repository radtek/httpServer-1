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

namespace com.superscene.ui {
	/// <summary>
	/// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
	///
	/// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:supersceneDeploy.sdk.superscene.TextBoxCheckbox"
	///
	///
	/// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:supersceneDeploy.sdk.superscene.TextBoxCheckbox;assembly=supersceneDeploy.sdk.superscene.TextBoxCheckbox"
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
	///     <MyNamespace:TextBoxCheckbox/>
	///
	/// </summary>
	public class TextBoxCheckbox : TextBoxFile {
		static TextBoxCheckbox() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxCheckbox), new FrameworkPropertyMetadata(typeof(TextBoxCheckbox)));
		}

		public TextBoxCheckbox() {
			Loaded +=TextBoxCheckbox_Loaded; ;
		}

		private void TextBoxCheckbox_Loaded(object sender, RoutedEventArgs e) {
			try {
				CheckBoxBan chk = (CheckBoxBan)(Template.FindName("chk", this));
				if(chk == null) {
					return;
				}

				chk.Checked += Chk_Checked;
				chk.Unchecked += Chk_Unchecked;

			} catch(Exception ) { }
		}

		private void Chk_Checked(object sender, RoutedEventArgs e) {
			//Debug.WriteLine(sender.GetType() + "," + e.OriginalSource.GetType() + "," + e.Source.GetType());
			if(e.OriginalSource != sender) {
				return;
			}
			RoutedEventArgs arg = new RoutedEventArgs(CheckedProperty, this);
			RaiseEvent(arg);
		}

		private void Chk_Unchecked(object sender, RoutedEventArgs e) {
			if(e.OriginalSource != sender) {
				return;
			}
			RoutedEventArgs arg = new RoutedEventArgs(UncheckedProperty, this);
			RaiseEvent(arg);
		}

		////Content
		//public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(TextBoxCheckbox), new PropertyMetadata(""));
		//public string Content {
		//	get { return (string)GetValue(ContentProperty); }
		//	set { SetValue(ContentProperty, value); }
		//}

		////Content color
		//public static readonly DependencyProperty ContentColorProperty = DependencyProperty.Register("ContentColor", typeof(Brush), typeof(TextBoxCheckbox), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"))));
		//public Brush ContentColor {
		//	get { return (Brush)GetValue(ContentColorProperty); }
		//	set { SetValue(ContentColorProperty, value); }
		//}

		////Left Width
		//public static readonly DependencyProperty LeftWidthProperty = DependencyProperty.Register("LeftWidth", typeof(Double), typeof(TextBoxCheckbox), new PropertyMetadata(Double.NaN));
		//public Double LeftWidth {
		//	get { return (Double)GetValue(LeftWidthProperty); }
		//	set { SetValue(LeftWidthProperty, value); }
		//}

		////Left Gap
		//public static readonly DependencyProperty _LabelPaddingProperty = DependencyProperty.Register("_LabelPadding", typeof(Thickness), typeof(TextBoxCheckbox), new PropertyMetadata(new Thickness(5, 0, 8, 0)));
		//public Thickness _LabelPadding {
		//	get { return (Thickness)GetValue(_LabelPaddingProperty); }
		//	set { SetValue(_LabelPaddingProperty, value); }
		//}

		////Right Width
		//public static readonly DependencyProperty RightWidthProperty = DependencyProperty.Register("RightWidth", typeof(Double), typeof(TextBoxCheckbox), new PropertyMetadata(Double.NaN));
		//public Double RightWidth {
		//	get { return (Double)GetValue(RightWidthProperty); }
		//	//get { return ActualWidth - LeftWidth; }
		//	set { SetValue(RightWidthProperty, value); }
		//}

		//AllowChange
		public static readonly DependencyProperty AllowChangeProperty = DependencyProperty.Register("AllowChange", typeof(bool), typeof(TextBoxCheckbox), new PropertyMetadata(true));
		public bool AllowChange {
			get { return (bool)GetValue(AllowChangeProperty); }
			set { SetValue(AllowChangeProperty, value); }
		}

		//AllowChange
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool?), typeof(TextBoxCheckbox), new PropertyMetadata(false));
		public bool? IsChecked {
			get { return (bool?)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		//Checked
		public static readonly RoutedEvent CheckedProperty = EventManager.RegisterRoutedEvent("Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextBoxCheckbox));
		public event RoutedEventHandler Checked {
			//将路由事件添加路由事件处理程序
			add { AddHandler(CheckedProperty, value); }
			//从路由事件处理程序中移除路由事件
			remove { RemoveHandler(CheckedProperty, value); }
		}

		//Checked
		public static readonly RoutedEvent UncheckedProperty = EventManager.RegisterRoutedEvent("Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextBoxCheckbox));
		public event RoutedEventHandler Unchecked {
			//将路由事件添加路由事件处理程序
			add { AddHandler(UncheckedProperty, value); }
			//从路由事件处理程序中移除路由事件
			remove { RemoveHandler(UncheckedProperty, value); }
		}

	}
}
