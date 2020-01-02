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

namespace httpServer.view.util {
	/// <summary>
	/// ListServer.xaml 的交互逻辑
	/// </summary>
	public partial class ListServer : UserControl {
		public ListServer() {
			InitializeComponent();
		}

		private void ImgDrag_MouseDown(object sender, MouseButtonEventArgs e) {
			Image img = sender as Image;
			if(img == null) {
				return;
			}

			Debug.WriteLine("aaa:" + img.Tag);

			DragMode = true;
		}

		//DragMode
		public static readonly DependencyProperty DragModeProperty = DependencyProperty.Register("DragMode", typeof(bool), typeof(ListServer), new PropertyMetadata(false));
		public bool DragMode {
			get { return (bool)GetValue(DragModeProperty); }
			set { SetCurrentValue(DragModeProperty, value); }
		}

		public System.Collections.IEnumerable ItemsSource { get { return lst.ItemsSource; } set { lst.ItemsSource = value; } }
		public int SelectedIndex { get { return lst.SelectedIndex; } set { lst.SelectedIndex = value; } }
		public ItemCollection Items { get { return lst.Items; } }

		public void ScrollIntoView(object item) {
			lst.ScrollIntoView(item);
		}

		//OnSelectionChanged
		//public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(EventHandler<SelectionChangedEventArgs>), typeof(ListServer));
		//public event SelectionChangedEventHandler SelectionChanged {
		//	//将路由事件添加路由事件处理程序
		//	add { Debug.WriteLine("bbb"); lst.SelectionChanged += value; }
		//	//从路由事件处理程序中移除路由事件
		//	remove { lst.SelectionChanged -= value; }
		//}


	}
}
