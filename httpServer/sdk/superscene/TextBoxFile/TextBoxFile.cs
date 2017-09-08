using Microsoft.Win32;
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

namespace com.superscene.ui {
	/// <summary>
	/// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
	///
	/// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:supersceneDeploy.sdk.superscene.TextBoxFile"
	///
	///
	/// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:supersceneDeploy.sdk.superscene.TextBoxFile;assembly=supersceneDeploy.sdk.superscene.TextBoxFile"
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
	///     <MyNamespace:TextBoxFile/>
	///
	/// </summary>
	public class TextBoxFile : TextBoxLabel {
		TextBox txt = null;
		Label btn = null;

		static TextBoxFile() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxFile), new FrameworkPropertyMetadata(typeof(TextBoxFile)));
		}

		public TextBoxFile() {
			Loaded += TextBoxFile_Loaded; ;
		}

		private void TextBoxFile_Loaded(object sender, RoutedEventArgs e) {
			try {
				txt = (TextBox)(Template.FindName("txt", this));
				if(txt == null) {
					return;
				}

				btn = (Label)(Template.FindName("btn", this));
				if(btn == null) {
					return;
				}

				btn.MouseEnter += Btn_MouseEnter;
				btn.MouseLeave += Btn_MouseLeave;
				btn.MouseLeftButtonDown += Btn_MouseLeftButtonDown;
				btn.MouseLeftButtonUp += Btn_MouseLeftButtonUp;
				txt.TextChanged += Txt_TextChanged;

			} catch(Exception) { }
		}

		private void Btn_MouseLeave(object sender, MouseEventArgs e) {
			_ButtonHeight = 5;
		}

		private void Btn_MouseEnter(object sender, MouseEventArgs e) {
			_ButtonHeight = 7;
		}

		private void Btn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			_ButtonHeight = 3;
		}

		private void Txt_TextChanged(object sender, TextChangedEventArgs e) {
			e.Handled = true;
		}

		private void Btn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			_ButtonHeight = 5;

			bool isSelected = false;
			if(IsSelectFolder) {
				isSelected = selectFolder();
			} else {
				isSelected = selectFile();
			}

			if(!isSelected) {
				return;
			}

			//发送事件
			RoutedEventArgs arg = new RoutedEventArgs(SelectedFileProperty, this);
			RaiseEvent(arg);

		}

		//选择文件
		private bool selectFile() {
			//选择文件
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "所有文件(*.*)|*.*";
			ofd.ValidateNames = true;
			ofd.CheckPathExists = true;
			ofd.CheckFileExists = true;
			if(ofd.ShowDialog() != true) {
				return true;
			}

			txt.Text = ofd.FileName;
			return false;
		}

		//选择文件夹
		private bool selectFolder() {
			//System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
			//if(fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
			//	return;
			//}

			//txt.Text = fbd.SelectedPath;

			var dlg = new System.Windows.Forms.FolderBrowserDialog();
			//dlg.Title = "My Title";
			//dlg.IsFolderPicker = true;

			//if(Directory.Exists(txt.Text)) {
			//	dlg.InitialDirectory = System.IO.Path.GetFullPath(txt.Text + "/../");
			//	dlg.DefaultDirectory = System.IO.Path.GetFullPath(txt.Text + "/../");
			//}

			//dlg.AddToMostRecentlyUsedList = false;
			//dlg.AllowNonFileSystemItems = false;
			//dlg.EnsureFileExists = true;
			//dlg.EnsurePathExists = true;
			//dlg.EnsureReadOnly = false;
			//dlg.EnsureValidNames = true;
			//dlg.Multiselect = false;
			//dlg.ShowPlacesList = true;
			

			if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				txt.Text = dlg.SelectedPath;
				return true;
				// Do something with selected folder string
			}

			return false;

			//OpenFileDialog ofd = new OpenFileDialog();
			//ofd.Filter = "folders|*.sado;fjsdal;fj";
			//ofd.FileName = "\r";
			//ofd.CheckFileExists = false;
			//ofd.CheckPathExists = false;
			//ofd.ValidateNames = false;
			//if(ofd.ShowDialog() == true) {
			//	ofd.FileName = ofd.FileName.TrimEnd('\r');
			//	int lastSeparatorIndex = ofd.FileName.LastIndexOf('\\');
			//	ofd.FileName = ofd.FileName.Remove(lastSeparatorIndex);
			//	txt.Text = ofd.FileName;
			//}

			//OpenFileDialog ofd = new OpenFileDialog();
			////ofd.Filter = CoffeeScript._ofdOutputFilter;
			//ofd.FileName = "Filename will be ignored";
			//ofd.CheckFileExists = false;
			//ofd.CheckPathExists = true;
			////
			//ofd.ValidateNames = false;
			//if (ofd.ShowDialog() == true) {
			//	//
			//	ofd.FileName = ofd.FileName.TrimEnd('\r');
			//	int lastSeparatorIndex = ofd.FileName.LastIndexOf('\\');
			//	ofd.FileName = ofd.FileName.Remove(lastSeparatorIndex);
			//	txt.Text = ofd.FileName;
			//}

		}

		//IsSelectFolder
		public static readonly DependencyProperty IsSelectFolderProperty = DependencyProperty.Register("IsSelectFolder", typeof(bool), typeof(TextBoxFile), new PropertyMetadata(false));
		public bool IsSelectFolder {
			get { return (bool)GetValue(IsSelectFolderProperty); }
			set { SetValue(IsSelectFolderProperty, value); }
		}

		//Content
		public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register("ButtonContent", typeof(string), typeof(TextBoxFile), new PropertyMetadata("..."));
		public string ButtonContent {
			get { return (string)GetValue(ButtonContentProperty); }
			set { SetValue(ButtonContentProperty, value); }
		}

		//Content color
		public static readonly DependencyProperty ButtonColorProperty = DependencyProperty.Register("ButtonColor", typeof(Brush), typeof(TextBoxFile), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"))));
		public Brush ButtonColor {
			get { return (Brush)GetValue(ButtonColorProperty); }
			set { SetValue(ButtonColorProperty, value); }
		}

		//Button Width
		public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register("ButtonWidth", typeof(Double), typeof(TextBoxFile), new PropertyMetadata(20.0));
		public Double ButtonWidth {
			get { return (Double)GetValue(ButtonWidthProperty); }
			//get { return ActualWidth - LeftWidth; }
			set { SetValue(ButtonWidthProperty, value); }
		}

		//Button Height
		public static readonly DependencyProperty _ButtonHeightProperty = DependencyProperty.Register("_ButtonHeight", typeof(Double), typeof(TextBoxFile), new PropertyMetadata(5.0));
		public Double _ButtonHeight {
			get { return (Double)GetValue(_ButtonHeightProperty); }
			//get { return ActualWidth - LeftWidth; }
			set { SetValue(_ButtonHeightProperty, value); }
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
