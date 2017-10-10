using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	///     xmlns:MyNamespace="clr-namespace:httpServer.sdk.superscene.LabelCombox"
	///
	///
	/// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:httpServer.sdk.superscene.LabelCombox;assembly=httpServer.sdk.superscene.LabelCombox"
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
	///     <MyNamespace:LabelCombox/>
	///
	/// </summary>
	public class ComboBoxLabel : ComboBox {
		//ComboBox cbx = null;

		////[Category("Custom Control Settings")]
		//public static readonly DependencyProperty ListItemsProperty = DependencyProperty.Register("ListItems", typeof(List<object>), typeof(ComboBoxLabel), null);
		//public List<object> ListItems { get { return (List<object>)GetValue(ListItemsProperty); } set { SetValue(ListItemsProperty, value); } }


		static ComboBoxLabel() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxLabel), new FrameworkPropertyMetadata(typeof(ComboBoxLabel)));
		}
		
		public ComboBoxLabel() {
			//Loaded += ComboBoxLabel_Loaded;
			//CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewExecutedMethod));
		}

		//public override void OnApplyTemplate() {
		//	base.OnApplyTemplate();
		//	try {
		//		Debug.WriteLine("aa");
		//		Grid grd = (Grid)(Template.FindName("grd", this));
		//		if(grd == null) {
		//			return;
		//		}
		//		Debug.WriteLine("bb");

		//		SetValue(Grid.ColumnProperty, 1);
		//		Padding = new Thickness(0, 0, 0, 3);
		//		grd.Children.Add(grd);
		//	} catch(Exception) { }
		//}

		//private void ComboBoxLabel_Loaded(object sender, RoutedEventArgs e) {
		//	try {
		//		cbx = (ComboBox)(Template.FindName("cbx", this));
		//		if(cbx == null) {
		//			return;
		//		}

		//		List<string> data = new List<string>();
		//		data.Add("aaa");
		//		data.Add("bbb");
		//		cbx.ItemsSource = ItemsSource;

		//	} catch(Exception) { }
		//}

		//Content
		public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(ComboBoxLabel), new PropertyMetadata(""));
		public string Content {
			get { return (string)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		//Content color
		public static readonly DependencyProperty ContentColorProperty = DependencyProperty.Register("ContentColor", typeof(Brush), typeof(ComboBoxLabel), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"))));
		public Brush ContentColor {
			get { return (Brush)GetValue(ContentColorProperty); }
			set { SetValue(ContentColorProperty, value); }
		}

		//Left Width
		public static readonly DependencyProperty LeftWidthProperty = DependencyProperty.Register("LeftWidth", typeof(Double), typeof(ComboBoxLabel), new PropertyMetadata(Double.NaN));
		public Double LeftWidth {
			get { return (Double)GetValue(LeftWidthProperty); }
			set { SetValue(LeftWidthProperty, value); }
		}

		//Left Gap
		public static readonly DependencyProperty _LabelPaddingProperty = DependencyProperty.Register("_LabelPadding", typeof(Thickness), typeof(ComboBoxLabel), new PropertyMetadata(new Thickness(5, 0, 8, 3)));
		public Thickness _LabelPadding {
			get { return (Thickness)GetValue(_LabelPaddingProperty); }
			set { SetValue(_LabelPaddingProperty, value); }
		}

		//Right Width
		public static readonly DependencyProperty RightWidthProperty = DependencyProperty.Register("RightWidth", typeof(Double), typeof(ComboBoxLabel), new PropertyMetadata(Double.NaN));
		public Double RightWidth {
			get { return (Double)GetValue(RightWidthProperty); }
			//get { return ActualWidth - LeftWidth; }
			set { SetValue(RightWidthProperty, value); }
		}

	}
}
