using csharpHelp.util;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
	public class TextBoxFile : TextBoxLabel {
		//TextBox txt = null;
		//Label btn = null;
		//public ICommand CmdOpenFile;

		static TextBoxFile() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxFile), new FrameworkPropertyMetadata(typeof(TextBoxFile)));
		}

		public TextBoxFile() : base() {
			//Loaded += TextBoxFile_Loaded;
			CmdOpenFile = new SupRelayCommand(new Action(onOpenFile));

			//TextChanged += (object sender, TextChangedEventArgs e) => {
			//	if (e.OriginalSource != this) {
			//		e.Handled = true;
			//	}
			//};
		}

		public static readonly DependencyProperty CmdOpenFileProperty = DependencyProperty.Register("CmdOpenFile", typeof(ICommand), typeof(TextBoxFile));
		public ICommand CmdOpenFile {
			get { return (ICommand)GetValue(CmdOpenFileProperty); }
			set { SetCurrentValue(CmdOpenFileProperty, value); }
		}

		private void onOpenFile() {
			bool isSelected = false;
			if (IsSelectFolder) {
				isSelected = selectFolder();
			} else {
				isSelected = selectFile();
			}

			if (!isSelected) {
				return;
			}

			//发送事件
			RoutedEventArgs arg = new RoutedEventArgs(SelectedFileProperty, this);
			RaiseEvent(arg);
		}

		//private void TextBoxFile_Loaded(object sender, RoutedEventArgs e) {
		//	try {
		//		txt = (TextBox)(Template.FindName("txt", this));
		//		if(txt == null) {
		//			return;
		//		}

		//		btn = (Label)(Template.FindName("btn", this));
		//		if(btn == null) {
		//			return;
		//		}

		//		btn.MouseEnter += Btn_MouseEnter;
		//		btn.MouseLeave += Btn_MouseLeave;
		//		btn.MouseLeftButtonDown += Btn_MouseLeftButtonDown;
		//		//btn.MouseLeftButtonUp += Btn_MouseLeftButtonUp;

		//		txt.TextChanged += Txt_TextChanged;
		//		txt.PreviewDragOver += Txt_PreviewDragOver;
		//		txt.PreviewDrop += Txt_PreviewDrop;

		//	} catch(Exception) {}
		//	Debug.WriteLine("bb:");
		//}

		//private void Btn_MouseLeave(object sender, MouseEventArgs e) {
		//	_ButtonHeight = 5;
		//}

		//private void Btn_MouseEnter(object sender, MouseEventArgs e) {
		//	_ButtonHeight = 7;
		//}

		//private void Btn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
		//	_ButtonHeight = 3;
		//}

		//private void Btn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
		//	_ButtonHeight = 5;

		//	bool isSelected = false;
		//	if(IsSelectFolder) {
		//		isSelected = selectFolder();
		//	} else {
		//		isSelected = selectFile();
		//	}

		//	if(!isSelected) {
		//		return;
		//	}

		//	//发送事件
		//	RoutedEventArgs arg = new RoutedEventArgs(SelectedFileProperty, this);
		//	RaiseEvent(arg);
		//}

		//private void Txt_TextChanged(object sender, TextChangedEventArgs e) {
		//	e.Handled = true;
		//}

		//允许拖拽文件
		public static readonly DependencyProperty IsEnabledDragFileProperty = DependencyProperty.RegisterAttached("IsEnabledDragFile", typeof(bool), typeof(TextBoxFile), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsEnabledDragFileChanged)));
		public static void SetIsEnabledDragFile(UIElement element, bool value) {
			element.SetCurrentValue(IsEnabledDragFileProperty, value);
		}
		public static bool GetIsEnabledDragFile(UIElement element) {
			return (bool)element.GetValue(IsEnabledDragFileProperty);
		}

		private static void OnIsEnabledDragFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var ele = d as UIElement;
			if (ele != null && e.NewValue != null) {
				if ((bool)e.NewValue) {
					ele.PreviewDragOver += Txt_PreviewDragOver;
					ele.PreviewDrop += Txt_PreviewDrop;
				} else {
					ele.PreviewDragOver -= Txt_PreviewDragOver;
					ele.PreviewDrop -= Txt_PreviewDrop;
				}
			}
		}

		private static void Txt_PreviewDragOver(object sender, DragEventArgs e) {
			//拖拽文件
			//if(ButtonWidth == 0) {
			//	return;
			//}
			e.Effects = DragDropEffects.Copy;
			e.Handled = true;
		}

		private static void Txt_PreviewDrop(object sender, DragEventArgs e) {
			TextBox txt = sender as TextBox;
			if (txt == null) {
				return;
			}

			//拖拽文件
			try {
				string[] docPath = (string[])e.Data.GetData(DataFormats.FileDrop);
				if(docPath.Length > 0) {
					//txt.Text = docPath[0];
					txt.Text = docPath[0];
				}
			} catch(Exception) { }
		}

		//选择文件
		private bool selectFile() {
			//选择文件
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "所有文件(*.*)|*.*";
			//if(File.Exists(txt.Text)) {
			if(File.Exists(Text)) {
				//ofd.InitialDirectory = System.IO.Path.GetDirectoryName(txt.Text);
				ofd.InitialDirectory = System.IO.Path.GetDirectoryName(Text);
			}
			ofd.ValidateNames = true;
			ofd.CheckPathExists = true;
			ofd.CheckFileExists = true;
			if(ofd.ShowDialog() != true) {
				return true;
			}

			//txt.Text = ofd.FileName;
			Text = ofd.FileName;
			return false;
		}

		//选择文件夹
		private bool selectFolder() {
			//var dlg = new System.Windows.Forms.FolderBrowserDialog();
			//dlg.Description = "";

			////if(Directory.Exists(txt.Text)) {
			////	dlg.SelectedPath = System.IO.Path.GetFullPath(txt.Text + "/../");
			////}

			//if (Directory.Exists(Text)) {
			//	dlg.SelectedPath = System.IO.Path.GetFullPath(Text + "/../");
			//}

			//if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
			//	//txt.Text = dlg.SelectedPath;
			//	Text = dlg.SelectedPath;
			//	return true;
			//	// Do something with selected folder string
			//}

			//return false;

			var dlg = new CommonOpenFileDialog();
			dlg.Title = "";
			dlg.IsFolderPicker = true;

			string text = Text;
			string strParent = IsPosParent ? "/../" : "";
			if(Directory.Exists(text)) {
				dlg.InitialDirectory = System.IO.Path.GetFullPath(text + strParent);
				dlg.DefaultDirectory = System.IO.Path.GetFullPath(text + strParent);
			}

			dlg.AddToMostRecentlyUsedList = false;
			dlg.AllowNonFileSystemItems = false;
			dlg.EnsureFileExists = true;
			dlg.EnsurePathExists = true;
			dlg.EnsureReadOnly = false;
			dlg.EnsureValidNames = true;
			dlg.Multiselect = false;
			dlg.ShowPlacesList = true;


			if(dlg.ShowDialog() == CommonFileDialogResult.Ok) {
				Text = dlg.FileName;
				return true;
				// Do something with selected folder string
			}

			return false;
		}

		//IsPosParent
		public static readonly DependencyProperty IsPosParentProperty = DependencyProperty.Register("IsPosParent", typeof(bool), typeof(TextBoxFile), new PropertyMetadata(true));
		public bool IsPosParent {
			get { return (bool)GetValue(IsPosParentProperty); }
			set { SetCurrentValue(IsPosParentProperty, value); }
		}

		//IsSelectFolder
		public static readonly DependencyProperty IsSelectFolderProperty = DependencyProperty.Register("IsSelectFolder", typeof(bool), typeof(TextBoxFile), new PropertyMetadata(false));
		public bool IsSelectFolder {
			get { return (bool)GetValue(IsSelectFolderProperty); }
			set { SetCurrentValue(IsSelectFolderProperty, value); }
		}

		//Content
		public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register("ButtonContent", typeof(string), typeof(TextBoxFile), new PropertyMetadata("..."));
		public string ButtonContent {
			get { return (string)GetValue(ButtonContentProperty); }
			set { SetCurrentValue(ButtonContentProperty, value); }
		}

		//Content color
		public static readonly DependencyProperty ButtonColorProperty = DependencyProperty.Register("ButtonColor", typeof(Brush), typeof(TextBoxFile), new PropertyMetadata(Brushes.Transparent));
		public Brush ButtonColor {
			get { return (Brush)GetValue(ButtonColorProperty); }
			set { SetCurrentValue(ButtonColorProperty, value); }
		}

		//Button Width
		public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register("ButtonWidth", typeof(Double), typeof(TextBoxFile), new PropertyMetadata(20.0));
		public Double ButtonWidth {
			get { return (Double)GetValue(ButtonWidthProperty); }
			//get { return ActualWidth - LeftWidth; }
			set { SetCurrentValue(ButtonWidthProperty, value); }
		}

		//Button Height
		public static readonly DependencyProperty _ButtonHeightProperty = DependencyProperty.Register("_ButtonHeight", typeof(Double), typeof(TextBoxFile), new PropertyMetadata(5.0));
		public Double _ButtonHeight {
			get { return (Double)GetValue(_ButtonHeightProperty); }
			//get { return ActualWidth - LeftWidth; }
			set { SetCurrentValue(_ButtonHeightProperty, value); }
		}

		//选择文件完成事件
		public static readonly RoutedEvent SelectedFileProperty = EventManager.RegisterRoutedEvent("SelectedFile", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextBoxLabel));
		public event RoutedEventHandler SelectedFile {
			//将路由事件添加路由事件处理程序
			add { AddHandler(SelectedFileProperty, value); }
			//从路由事件处理程序中移除路由事件
			remove { RemoveHandler(SelectedFileProperty, value); }
		}

	}
}
