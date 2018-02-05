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

namespace httpServer.assembly.util {
	class RelayCommand : ICommand {
		private Action<object> _action;
		public RelayCommand(Action<object> action) {
			_action = action;
		}

		public bool CanExecute(object parameter) {
			return true;
		}

		public event EventHandler CanExecuteChanged {
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter) {
			_action?.Invoke(parameter);
		}
	}

	public class UpDownEvent : RoutedEventArgs {
		public object tag = 0;
		
		public UpDownEvent(RoutedEvent routedEvent, object source, object _tag) : base(routedEvent, source) {
			tag = _tag;
		}
	}

	/// <summary>
	/// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
	///
	/// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:httpServer.assembly.util"
	///
	///
	/// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:httpServer.assembly.util;assembly=httpServer.assembly.util"
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
	///     <MyNamespace:ListBoxServer/>
	///
	/// </summary>
	public class ListBoxServer : ListBox {
		//public enum EDataType{
		//	Run, Stop
		//};

		static ListBoxServer() {
			//Visibility = Visibility.Collapsed
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ListBoxServer), new FrameworkPropertyMetadata(typeof(ListBoxServer)));
		}

		public ListBoxServer() {
			CmdUpClick = new RelayCommand(new Action<object>(onUpClick));
			CmdDownClick = new RelayCommand(new Action<object>(onDownClick));
		}

		public ICommand CmdUpClick { get; set; }
		public ICommand CmdDownClick { get; set; }
		//public IEnumerable<string> Items { get; private set; }

		private void onUpClick(object tag) {
			//Debug.WriteLine(sender.GetType());
			UpDownEvent arg = new UpDownEvent(UpClickProperty, this, tag);
			RaiseEvent(arg);
		}

		private void onDownClick(object tag) {
			UpDownEvent arg = new UpDownEvent(DownClickProperty, this, tag);
			RaiseEvent(arg);
		}

		//UpClick
		public static readonly RoutedEvent UpClickProperty = EventManager.RegisterRoutedEvent("UpClick", RoutingStrategy.Bubble, typeof(EventHandler<UpDownEvent>), typeof(ListBoxServer));
		public event RoutedEventHandler UpClick {
			//将路由事件添加路由事件处理程序
			add { AddHandler(UpClickProperty, value); }
			//从路由事件处理程序中移除路由事件
			remove { RemoveHandler(UpClickProperty, value); }
		}

		//DownClick
		public static readonly RoutedEvent DownClickProperty = EventManager.RegisterRoutedEvent("DownClick", RoutingStrategy.Bubble, typeof(EventHandler<UpDownEvent>), typeof(ListBoxServer));
		public event RoutedEventHandler DownClick {
			//将路由事件添加路由事件处理程序
			add { AddHandler(DownClickProperty, value); }
			//从路由事件处理程序中移除路由事件
			remove { RemoveHandler(DownClickProperty, value); }
		}

		//public static readonly DependencyProperty DataTypeProperty = DependencyProperty.Register("DataType", typeof(EDataType), typeof(ListBoxServer), new UIPropertyMetadata(null));
		//public EDataType DataType {
		//	get { return (EDataType)GetValue(DataTypeProperty); }
		//	set { SetValue(DataTypeProperty, value); }
		//}

		//private static readonly DependencyProperty ImgResProperty = DependencyProperty.Register("ImgRes", typeof(string), typeof(ListBoxServer), new UIPropertyMetadata(null));
		//private string ImgRes {
		//	get { return "/httpServer;component/resource/status_run.pn"; }
		//	set { SetValue(DataTypeProperty, value); }
		//}

	}
}
