using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace csharpHelp.ui {
	public class XCtl {
		///ListBoxItem 点击选中
		public static readonly DependencyProperty LstItemSelectProperty = DependencyProperty.RegisterAttached("LstItemSelect", typeof(bool), typeof(XCtl), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnLstItemSelectChanged)));
		public static void SetLstItemSelect(UIElement element, bool value) { element.SetCurrentValue(LstItemSelectProperty, value); }
		public static bool GetLstItemSelect(UIElement element) { return (bool)element.GetValue(LstItemSelectProperty); }

		private static void OnLstItemSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var ele = d as ListBoxItem;
			bool? isEnable = e.NewValue as bool?;
			if(ele == null || isEnable == null) {
				return;
			}

			if(isEnable == true) {
				ele.PreviewGotKeyboardFocus += lstItemSelect_PreviewGotKeyboardFocus;
			} else {
				ele.PreviewGotKeyboardFocus -= lstItemSelect_PreviewGotKeyboardFocus;
			}
		}

		private static void lstItemSelect_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			ListBoxItem item = sender as ListBoxItem;
			if(item == null) {
				return;
			}
			item.IsSelected = true;
		}

		///清晰字体
		public static readonly DependencyProperty ClearFontProperty = DependencyProperty.RegisterAttached("ClearFont", typeof(bool), typeof(XCtl), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnClearFontChanged)));
		public static void SetClearFont(UIElement element, bool value) { element.SetCurrentValue(LstItemSelectProperty, value); }
		public static bool GetClearFont(UIElement element) { return (bool)element.GetValue(LstItemSelectProperty); }

		private static void OnClearFontChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var ele = d as UIElement;
			bool? isEnable = e.NewValue as bool?;
			if(ele == null || isEnable == null) {
				return;
			}

			if(isEnable == true) {
				ele.SetCurrentValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
				ele.SetCurrentValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Aliased);
			} else {
				
			}
		}

		////RichTextBox binding
		//public static readonly DependencyProperty RichTextSetByCtlProperty = DependencyProperty.RegisterAttached("RichTextSetByCtl", typeof(bool), typeof(XCtl), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });
		//public static void SetRichTextSetByCtl(UIElement element, bool value) { element.SetCurrentValue(RichTextSetByCtlProperty, value); }
		//public static bool GetRichTextSetByCtl(UIElement element) { return (bool)element.GetValue(RichTextSetByCtlProperty); }

		//public static readonly DependencyProperty RichTextSetByVMProperty = DependencyProperty.RegisterAttached("RichTextSetByVM", typeof(bool), typeof(XCtl), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });
		//public static void SetRichTextSetByVM(UIElement element, bool value) { element.SetCurrentValue(RichTextSetByVMProperty, value); }
		//public static bool GetRichTextSetByVM(UIElement element) { return (bool)element.GetValue(RichTextSetByVMProperty); }

		//public static readonly DependencyProperty RichTextProperty = DependencyProperty.RegisterAttached("RichText", typeof(string), typeof(XCtl), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, PropertyChangedCallback = OnRichTextChanged });
		//public static void SetRichText(UIElement element, string value) { element.SetCurrentValue(RichTextProperty, value); }
		//public static string GetRichText(UIElement element) { return (string)element.GetValue(RichTextProperty); }
		
		//private static void OnRichTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		//	//Debug.WriteLine("aa");
		//	var richTextBox = d as RichTextBox;
		//	if(richTextBox == null) {
		//		return;
		//	}

		//	FlowDocument val = new FlowDocument();
		//	Paragraph paragraph = new Paragraph();
		//	paragraph.Inlines.Add(e.NewValue as string);
		//	//val.Blocks.Add(paragraph);
			
		//	if(val == null) {
		//		return;
		//	}
		//	richTextBox.Document.Blocks.Clear();
		//	richTextBox.Document.Blocks.Add(paragraph);
		//	richTextBox.Document.PageWidth = 1000;
		//}

		//private static void RichText_TextChanged(object sender, TextChangedEventArgs e) {
		//	var richTextBox = sender as RichTextBox;
		//}
	}
}
