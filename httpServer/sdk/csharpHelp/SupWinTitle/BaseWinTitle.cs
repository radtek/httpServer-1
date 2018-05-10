using csharpHelp.util;
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
using System.Windows.Shell;

namespace csharpHelp.ui {
	/// <summary>
	/// SupWinTitle.xaml 的交互逻辑
	/// </summary>
	public class BaseWinTitle : UserControl {
		//static BaseWinTitle() {
		//	DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseWinTitle),
		//		new FrameworkPropertyMetadata(typeof(BaseWinTitle)));
		//}
		private Window win = null;
		private bool isMouseMove = false;
		private object clickEle = null;
		private bool isMax = false;
		private Rect rcNormal = new Rect();
		//private bool isFirstRect = true;
		private Thickness oldTickness = new Thickness(5);

		//public BaseWinTitle() : base() {
		//	//InitializeComponent();
		//	Background = Brushes.Transparent;

		//	//Loaded += BaseWinTitle_Loaded;
		//}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			//throw new NotImplementedException();
			//Debug.WriteLine("aa");
			win = findWin();
			if (win == null) {
				//base.OnVisualParentChanged(oldParent);
				return;
			}

			win.MouseLeftButtonDown += Win_MouseLeftButtonDown;
			win.MouseLeftButtonUp += Win_MouseLeftButtonUp;

			WindowChrome chrome = WindowChrome.GetWindowChrome(win);
			if (chrome == null) {
				chrome = new WindowChrome();
				chrome.CaptionHeight = 0;
				chrome.CornerRadius = new CornerRadius(0);
				chrome.GlassFrameThickness = new Thickness(1);
				chrome.NonClientFrameEdges = NonClientFrameEdges.None;
				//base.OnVisualParentChanged(oldParent);
				//return;
				WindowChrome.SetWindowChrome(win, chrome);
			}
			chrome.ResizeBorderThickness = oldTickness;

			// Call base class to perform standard event handling. 
			//base.OnVisualParentChanged(oldParent);
		}

		private void Win_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			clickEle = e.OriginalSource;
		}

		private void Win_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			clickEle = null;
		}

		//protected override void OnVisualParentChanged(DependencyObject oldParent) {
		//	win = findWin();
		//	if (win == null) {
		//		base.OnVisualParentChanged(oldParent);
		//		return;
		//	}

		//	WindowChrome chrome = WindowChrome.GetWindowChrome(win);
		//	if (chrome == null) {
		//		chrome = new WindowChrome();
		//		chrome.CaptionHeight = 0;
		//		//base.OnVisualParentChanged(oldParent);
		//		//return;
		//		WindowChrome.SetWindowChrome(win, chrome);
		//	}
		//	chrome.ResizeBorderThickness = oldTickness;

		//	// Call base class to perform standard event handling. 
		//	base.OnVisualParentChanged(oldParent);
		//}

		protected Window findWin() {
			Window winTmp = null;
			try {
				object parent = Parent;
				while (((FrameworkElement)parent).Parent != null) {
					parent = ((FrameworkElement)parent).Parent;
				}

				winTmp = (Window)parent;
			} catch (Exception) {

			}

			return winTmp;
		}

		/// <summary>
		/// 窗口移动事件
		/// </summary>
		public void UserControl_MouseMove(object sender, MouseEventArgs e) {
			if (win == null) { return; }

			isMouseMove = true;
			//Debug.WriteLine(e.OriginalSource);
			//Debug.WriteLine(e.Source);
			//Debug.WriteLine(clickEle);
			if (isMax) {
				return;
			}
			if (clickEle != e.OriginalSource) {
				return;
			}
			if (e.LeftButton != MouseButtonState.Pressed) {
				return;
			}
			//Grid back = (Grid)FindName("grdBack");
			//if (e.OriginalSource != back) {
			//	return;
			//}

			//Window win = findWin();
			win.DragMove();
		}

		//private bool isWinMax() {
		//	if (win == null) { return false; }

		//	//Window win = findWin();
		//	Rect rc = SystemParameters.WorkArea;//获取工作区大小
		//	if (win.Width == rc.Width && win.Height == rc.Height) {
		//		return true;
		//	}

		//	return false;
		//}

		public void updateWinSize() {
			if (win == null) { return; }

			//Window win = findWin();
			WindowChrome chrome = WindowChrome.GetWindowChrome(win);

			//if (isFirstRect) {

			//}
			
			//Grid grdNormal = (Grid)FindName("btnNormal");
			//Grid grdMax = (Grid)FindName("btnMax");
			if (isMax) {
				chrome.ResizeBorderThickness = oldTickness;

				win.Left = rcNormal.Left;
				win.Top = rcNormal.Top;
				win.Width = rcNormal.Width;
				win.Height = rcNormal.Height;

				//grdNormal.Visibility = Visibility.Visible;
				//grdMax.Visibility = Visibility.Collapsed;
			} else {
				oldTickness = chrome.ResizeBorderThickness;
				chrome.ResizeBorderThickness = new Thickness(0);

				rcNormal = new Rect(win.Left, win.Top, win.Width, win.Height);//保存下当前位置与大小
				
				win.Left = 0;//设置位置
				win.Top = 0;
				Rect rc = SystemParameters.WorkArea;//获取工作区大小
				win.Width = rc.Width;
				win.Height = rc.Height;

				//grdNormal.Visibility = Visibility.Collapsed;
				//grdMax.Visibility = Visibility.Visible;
			}
			isMax = !isMax;
			IsMaximized = isMax;
			//Debug.WriteLine(IsMaximized);
			//if (win.WindowState == WindowState.Maximized) {
			//	win.WindowState = WindowState.Normal; //设置窗口还原
			//} else {
			//	win.WindowState = WindowState.Maximized; //设置窗口最大化
			//}

			//Grid grdNormal = (Grid)FindName("btnNormal");
			//Grid grdMax = (Grid)FindName("btnMax");

			//grdNormal.Visibility = win.WindowState == WindowState.Maximized ? Visibility.Collapsed : Visibility.Visible;
			//grdMax.Visibility = win.WindowState == WindowState.Maximized ? Visibility.Visible : Visibility.Collapsed;
		}

		/// <summary>
		/// 标题栏单击/双击事件
		/// </summary>
		public void UserControl_MouseDown(object sender, MouseButtonEventArgs e) {
			//clickEle = e.OriginalSource;

			if (isMouseMove) {
				isMouseMove = false;
				return;
			}
			isMouseMove = false;
			//clickEle = e.OriginalSource;

			//Window win = findWin();
			if (e.ClickCount != 2) {
				return;
			}

			//if (win.WindowState == WindowState.Maximized) {
			//	win.WindowState = WindowState.Normal; //设置窗口还原
			//} else {
			//	win.WindowState = WindowState.Maximized; //设置窗口最大化
			//}
			updateWinSize();
		}

		/// <summary>
		/// 窗口最小化
		/// </summary>
		public void btnMin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if (win == null) { return; }

			if (clickEle != e.Source) {
				return;
			}
			//if (isMouseMove) {
			//	return;
			//}

			//Window win = ((Grid)this.Parent).Parent as Window;

			win.WindowState = WindowState.Minimized; //设置窗口最小化
		}

		/// <summary>
		/// 窗口最大化与还原
		/// </summary>
		public void btnMax_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if (clickEle != e.Source) {
				return;
			}
			//if (isMouseMove) {
			//	return;
			//}

			//Window win = ((Grid)this.Parent).Parent as Window;

			//if (win.WindowState == WindowState.Maximized) {
			//	win.WindowState = WindowState.Normal; //设置窗口还原
			//} else {
			//	win.WindowState = WindowState.Maximized; //设置窗口最大化
			//}
			updateWinSize();
		}

		/// <summary>
		/// 窗口关闭
		/// </summary>
		public void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if (win == null) { return; }

			if (clickEle != e.Source) {
				return;
			}
			//if (isMouseMove) {
			//	return;
			//}

			//Window win = ((Grid)this.Parent).Parent as Window;

			win.Close();
		}

		//public void rightButton_MouseEnter(object sender, MouseEventArgs e) {
		//	Grid grd = (Grid)sender;
		//	grd.Background = ComUtil.createBrush("#e5e5e5");
		//}

		//public void btnClose_MouseEnter(object sender, MouseEventArgs e) {
		//	Grid grd = (Grid)sender;
		//	grd.Background = ComUtil.createBrush("#e81123");
		//	try {
		//		Path path = (Path)FindName("pathClose");
		//		path.Stroke = ComUtil.createBrush("#fff");
		//	} catch (Exception) { }
		//	//pathClose.Stroke = ComUtil.createBrush("#fff");
		//}

		//public void rightButton_MouseLeave(object sender, MouseEventArgs e) {
		//	Grid grd = (Grid)sender;
		//	grd.Background = Brushes.Transparent;
		//}

		//public void btnClose_MouseLeave(object sender, MouseEventArgs e) {
		//	Grid grd = (Grid)sender;
		//	grd.Background = Brushes.Transparent;
		//	try {
		//		Path path = (Path)FindName("pathClose");
		//		path.Stroke = ComUtil.createBrush("#000");
		//	} catch (Exception) { }
		//	//pathClose.Stroke = ComUtil.createBrush("#000");
		//}

		public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register("IsMaximized", typeof(bool), typeof(BaseWinTitle), new PropertyMetadata(false));
		public bool IsMaximized {
			get { return (bool)GetValue(IsMaximizedProperty); }
			set { SetCurrentValue(IsMaximizedProperty, value); }
		}

	}
}
