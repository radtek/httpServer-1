using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace csharpHelp.ui {
	class EvtToBool {
		private string boolValueName = "";
		private string[] evtNameTrue = new string[0];
		private string[] evtNameFalse = new string[0];
		private Dictionary<UIElement, List<Delegate>> mapHandlerTrue = new Dictionary<UIElement, List<Delegate>>();
		private Dictionary<UIElement, List<Delegate>> mapHandlerFalse = new Dictionary<UIElement, List<Delegate>>();

		public EvtToBool(string _boolValueName, string[] _evtNameTrue, string[] _evtNameFalse) {
			boolValueName = _boolValueName;
			evtNameTrue = _evtNameTrue;
			evtNameFalse = _evtNameFalse;
		}

		public void OnEvtEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var ele = d as UIElement;
			if (ele == null) {
				return;
			}
			bool? isEnable = e.NewValue as bool?;
			if (isEnable == null) {
				return;
			}
			//string evtName = e.NewValue as string;
			//if (evtName == null) {
			//	return;
			//}

			var methodInfoTrue = GetType().GetMethod("OnEvtTrue", BindingFlags.Instance | BindingFlags.NonPublic);
			var methodInfoFalse = GetType().GetMethod("OnEvtFalse", BindingFlags.Instance | BindingFlags.NonPublic);

			Type t = ele.GetType();
			if (isEnable == true) {
				List<Delegate> lstHandTrue = new List<Delegate>();
				List<Delegate> lstHandFalse = new List<Delegate>();
				for (int i = 0; i < evtNameTrue.Length; ++i) {
					EventInfo evtTrue = t.GetEvent(evtNameTrue[i]);
					Delegate handlerTrue = Delegate.CreateDelegate(evtTrue.EventHandlerType, this, methodInfoTrue);
					lstHandTrue.Add(handlerTrue);
					evtTrue.AddEventHandler(ele, handlerTrue);
				}
				for (int i = 0; i < evtNameFalse.Length; ++i) {
					EventInfo evtFalse = t.GetEvent(evtNameFalse[i]);
					Delegate handlerFalse = Delegate.CreateDelegate(evtFalse.EventHandlerType, this, methodInfoFalse);
					lstHandFalse.Add(handlerFalse);
					evtFalse.AddEventHandler(ele, handlerFalse);
				}
				mapHandlerTrue[ele] = lstHandTrue;
				mapHandlerFalse[ele] = lstHandFalse;
			} else {
				if (mapHandlerTrue.ContainsKey(ele)) {
					List<Delegate> lstHandTrue = mapHandlerTrue[ele];
					List<Delegate> lstHandFalse = mapHandlerFalse[ele];
					for (int i = 0; i < lstHandTrue.Count; ++i) {
						EventInfo evtTrue = t.GetEvent(evtNameTrue[i]);
						evtTrue.RemoveEventHandler(ele, lstHandTrue[i]);
					}
					for (int i = 0; i < lstHandFalse.Count; ++i) {
						EventInfo evtFalse = t.GetEvent(evtNameFalse[i]);
						evtFalse.RemoveEventHandler(ele, lstHandFalse[i]);
					}
					mapHandlerTrue.Remove(ele);
					mapHandlerFalse.Remove(ele);
				}
			}
			//EventInfo evtFalse = t.GetEvent(evtNameFalse);

			//var methodInfoTrue = GetType().GetMethod("OnEvtTrue", BindingFlags.Instance | BindingFlags.NonPublic);
			//var methodInfoFalse = GetType().GetMethod("OnEvtFalse", BindingFlags.Instance | BindingFlags.NonPublic);
				
			//if ((bool)e.NewValue) {
			//	Delegate handlerTrue = Delegate.CreateDelegate(evtTrue.EventHandlerType, this, methodInfoTrue);
			//	Delegate handlerFalse = Delegate.CreateDelegate(evtFalse.EventHandlerType, this, methodInfoFalse);
			//	mapHandlerTrue[ele] = handlerTrue;
			//	mapHandlerFalse[ele] = handlerFalse;
			//	evtTrue.AddEventHandler(ele, handlerTrue);
			//	evtFalse.AddEventHandler(ele, handlerFalse);
			//} else {
			//	if (mapHandlerTrue.ContainsKey(ele)) {
			//		Delegate handlerTrue = mapHandlerTrue[ele];
			//		Delegate handlerFalse = mapHandlerFalse[ele];
			//		evtTrue.RemoveEventHandler(ele, handlerTrue);
			//		evtFalse.RemoveEventHandler(ele, handlerFalse);
			//		mapHandlerTrue.Remove(ele);
			//		mapHandlerFalse.Remove(ele);
			//	}
			//}
		}

		private void OnEvtTrue(object sender, RoutedEventArgs e) {
			var ele = e.Source as UIElement;

			Type t = typeof(XEvtTrigger);
			MethodInfo mf = t.GetMethod(boolValueName, BindingFlags.Public | BindingFlags.Static);
			object[] arrParam = new object[] { ele, true };
			mf.Invoke(null, arrParam);
		}

		private void OnEvtFalse(object sender, RoutedEventArgs e) {
			var ele = e.Source as UIElement;

			Type t = typeof(XEvtTrigger);
			MethodInfo mf = t.GetMethod(boolValueName, BindingFlags.Public | BindingFlags.Static);
			object[] arrParam = new object[] {ele, false };
			mf.Invoke(null, arrParam);
		}
	}

	class EvtToCmd<T> {
		private Dictionary<UIElement, ICommand> maEvt = new Dictionary<UIElement, ICommand>();
		private Dictionary<UIElement, Delegate> mapHandler = new Dictionary<UIElement, Delegate>();
		private string evtName = "";
		//public Delegate handler = null;

		public EvtToCmd(string _evtName) {
			evtName = _evtName;
			//EvtProperty = DependencyProperty.RegisterAttached(_evtName, typeof(ICommand), typeof(EvtToCmd<T>), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnEvtChanged)));
		}

		//public readonly DependencyProperty EvtProperty = null;
		//public void SetEvt(UIElement element, bool value) {
		//	element.SetValue(EvtProperty, value);
		//}
		//public bool GetEvt(UIElement element) {
		//	return (bool)element.GetValue(EvtProperty);
		//}

		public void OnEvtChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var ele = d as UIElement;
			Type t = ele.GetType();
			EventInfo evt = t.GetEvent(evtName);
			if (evt == null) {
				return;
			}

			var methodInfo = GetType().GetMethod("OnEvt", BindingFlags.Instance | BindingFlags.NonPublic);

			var cmd = e.NewValue as ICommand;
			if (ele != null) {
				if (cmd != null) {
					Delegate handler = Delegate.CreateDelegate(evt.EventHandlerType, this, methodInfo);
					mapHandler[ele] = handler;
					maEvt[ele] = cmd;
					//ele.MouseLeftButtonUp += OnEvt;
					evt.AddEventHandler(ele, handler);
				} else {
					//ele.MouseLeftButtonUp -= OnEvt;
					if (mapHandler.ContainsKey(ele)) {
						Delegate handler = mapHandler[ele];
						evt.RemoveEventHandler(ele, handler);
						mapHandler.Remove(ele);
					}
					if (maEvt.ContainsKey(ele)) {
						maEvt.Remove(ele);
					}
				}
			}
		}

		private void OnEvt(object sender, T e) {
			var ele = sender as UIElement;
			if (ele == null) {
				return;
			}
			if (!maEvt.ContainsKey(ele)) {
				return;
			}

			maEvt[ele].Execute(null);
		}
	}

	class XEvtTrigger {
		//IsEnabledMouseLeftButtonDown
		public static EvtToBool XEnabelMouseLeftButtonDown = new EvtToBool("SetIsMouseLeftButtonDown", new string[]{"MouseLeftButtonDown"}, new string[] { "MouseLeftButtonUp", "MouseLeave" });
		public static readonly DependencyProperty IsEnabledMouseLeftButtonDownProperty = DependencyProperty.RegisterAttached("IsEnabledMouseLeftButtonDown", typeof(bool), typeof(XEvtTrigger), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(XEnabelMouseLeftButtonDown.OnEvtEnableChanged)));
		public static void SetIsEnabledMouseLeftButtonDown(UIElement element, bool value) { element.SetCurrentValue(IsEnabledMouseLeftButtonDownProperty, value); }
		public static bool GetIsEnabledMouseLeftButtonDown(UIElement element) {
			return (bool)element.GetValue(IsEnabledMouseLeftButtonDownProperty);
		}

		//IsMouseLeftButtonDown
		//public static readonly DependencyPropertyKey IsMouseLeftButtonDownPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsMouseLeftButtonDown", typeof(bool), typeof(XEvtTrigger), new FrameworkPropertyMetadata(false));
		//public static readonly DependencyProperty IsMouseLeftButtonDownProperty = IsMouseLeftButtonDownPropertyKey.DependencyProperty;
		public static readonly DependencyProperty IsMouseLeftButtonDownProperty = DependencyProperty.RegisterAttached("IsMouseLeftButtonDown", typeof(bool), typeof(XEvtTrigger), new FrameworkPropertyMetadata(false));
		public static void SetIsMouseLeftButtonDown(UIElement element, bool value) { element.SetCurrentValue(IsMouseLeftButtonDownProperty, value); }
		public static bool GetIsMouseLeftButtonDown(UIElement element) { return (bool)element.GetValue(IsMouseLeftButtonDownProperty); }
		


		//IsEnabled IsLoaded
		public static EvtToBool XEnabelIsLoaded = new EvtToBool("SetIsLoaded", new string[] { "Loaded" }, new string[] { });
		public static readonly DependencyProperty IsEnabledIsLoadedProperty = DependencyProperty.RegisterAttached("IsEnabledIsLoaded", typeof(bool), typeof(XEvtTrigger), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(XEnabelIsLoaded.OnEvtEnableChanged)));
		public static void SetIsEnabledIsLoaded(UIElement element, bool value) { element.SetCurrentValue(IsEnabledIsLoadedProperty, value); }
		public static bool GetIsEnabledIsLoaded(UIElement element) {
			return (bool)element.GetValue(IsEnabledIsLoadedProperty);
		}

		//IsLoaded
		public static readonly DependencyProperty IsLoadedProperty = DependencyProperty.RegisterAttached("IsLoaded", typeof(bool), typeof(XEvtTrigger), new FrameworkPropertyMetadata(false));
		public static void SetIsLoaded(UIElement element, bool value) { element.SetCurrentValue(IsLoadedProperty, value); }
		public static bool GetIsLoaded(UIElement element) { return (bool)element.GetValue(IsLoadedProperty); }

		////mouse down element enable
		////public static EvtToBool XEnableDownEle = new EvtToBool("SetIsEnableDownEle", new string[] { "MouseLeftButtonDown" }, new string[] { "MouseLeftButtonUp" });
		//public static readonly DependencyProperty IsEnableDownEleProperty = DependencyProperty.RegisterAttached("IsEnableDownEle", typeof(bool), typeof(XEvtTrigger), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsEnableDownChanged)));
		//private static void OnIsEnableDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		//	var ele = d as UIElement;
		//	bool? isEnable = e.NewValue as bool?;
		//	if (ele == null || isEnable == null) {
		//		return;
		//	}

		//	if (isEnable == true) {
		//		ele.MouseLeftButtonDown += ele_MouseLeftButtonDown;
		//		ele.MouseLeftButtonUp += ele_MouseLeftButtonUp;
		//	} else {
		//		ele.MouseLeftButtonDown -= ele_MouseLeftButtonDown;
		//		ele.MouseLeftButtonUp -= ele_MouseLeftButtonUp;
		//	}

		//}
		//private static void ele_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
		//	SetDownEle(sender as UIElement, sender);
		//}
		//private static void ele_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
		//	SetDownEle(sender as UIElement, null);
		//}
		//public static void SetIsEnableDownEle(UIElement element, bool value) { element.SetCurrentValue(IsEnableDownEleProperty, value); }
		//public static bool GetIsEnableDownEle(UIElement element) {
		//	return (bool)element.GetValue(IsEnableDownEleProperty);
		//}

		////mouse down element
		//public static readonly DependencyProperty DownEleProperty = DependencyProperty.RegisterAttached("DownEle", typeof(object), typeof(XEvtTrigger), new FrameworkPropertyMetadata(false));
		//public static void SetDownEle(UIElement element, object value) { element.SetCurrentValue(DownEleProperty, value); }
		//public static object GetDownEle(UIElement element) { return (object)element.GetValue(DownEleProperty); }

		//private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		//	var element = d as UIElement;
		//	if (element != null && e.NewValue != null) {
		//		if ((bool)e.NewValue) {
		//			Register(element);
		//		} else {
		//			UnRegister(element);
		//		}
		//	}
		//}

		//private static void Register(UIElement element) {
		//	element.PreviewMouseDown += element_MouseDown;
		//	element.PreviewMouseLeftButtonDown += element_MouseLeftButtonDown;
		//	element.MouseLeave += element_MouseLeave;
		//	element.PreviewMouseUp += element_MouseUp;
		//}

		//private static void UnRegister(UIElement element) {
		//	element.PreviewMouseDown -= element_MouseDown;
		//	element.PreviewMouseLeftButtonDown -= element_MouseLeftButtonDown;
		//	element.MouseLeave -= element_MouseLeave;
		//	element.PreviewMouseUp -= element_MouseUp;
		//}

		//private static void element_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
		//	var element = e.Source as UIElement;
		//	if (element != null) {
		//		SetIsMouseDown(element, true);
		//	}
		//}

		//private static void element_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
		//	var element = e.Source as UIElement;
		//	if (element != null) {
		//		SetIsMouseLeftButtonDown(element, true);
		//	}
		//}

		//private static void element_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
		//	var element = e.Source as UIElement;
		//	if (element != null) {
		//		SetIsMouseDown(element, false);
		//		SetIsMouseLeftButtonDown(element, false);
		//	}
		//}

		//private static void element_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
		//	var element = e.Source as UIElement;
		//	if (element != null) {
		//		SetIsMouseDown(element, false);
		//		SetIsMouseLeftButtonDown(element, false);
		//	}
		//}

		//IsMouseDown
		//internal static readonly DependencyPropertyKey IsMouseDownPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsMouseDown", typeof(bool), typeof(XEvtTrigger), new FrameworkPropertyMetadata(false));
		//public static readonly DependencyProperty IsMouseDownProperty = IsMouseDownPropertyKey.DependencyProperty;
		//internal static void SetIsMouseDown(UIElement element, bool value) {
		//	element.SetCurrentValue(IsMouseDownPropertyKey, value);
		//}
		//public static bool GetIsMouseDown(UIElement element) {
		//	return (bool)element.GetValue(IsMouseDownProperty);
		//}

		//public virtual event PropertyChangedEventHandler PropertyChanged;
		//public virtual void FirePropertyChanged(string propertyName) {
		//	PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		//}

		//public static readonly DependencyProperty IsMouseLeftButtonDownProperty = DependencyProperty.RegisterAttached("IsMouseLeftButtonDown", typeof(bool), typeof(XEvtTrigger), new PropertyMetadata(false));

		//public static double GetIsMouseLeftButtonDown(DependencyObject obj) {
		//	return (double)obj.GetValue(IsMouseLeftButtonDownProperty);
		//}

		//public static void SetIsMouseLeftButtonDown(DependencyObject obj, double value) {
		//	obj.SetCurrentValue(IsMouseLeftButtonDownProperty, value);
		//}

		//public static Dictionary<UIElement, ICommand> mapCmdMouseLeftButtonUp = new Dictionary<UIElement, ICommand>();

		public static EvtToCmd<RoutedEventArgs> XMouseLeftButtonUp = new EvtToCmd<RoutedEventArgs>("MouseLeftButtonUp");
		public static readonly DependencyProperty MouseLeftButtonUpProperty = DependencyProperty.RegisterAttached("MouseLeftButtonUp", typeof(ICommand), typeof(XEvtTrigger), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(XMouseLeftButtonUp.OnEvtChanged)));
		public static void SetMouseLeftButtonUp(UIElement ele, ICommand value) { ele.SetCurrentValue(MouseLeftButtonUpProperty, value); }
		public static ICommand GetMouseLeftButtonUp(UIElement ele) { return (ICommand)ele.GetValue(MouseLeftButtonUpProperty); }

		public static EvtToCmd<RoutedEventArgs> XPreviewMouseLeftButtonUp = new EvtToCmd<RoutedEventArgs>("PreviewMouseLeftButtonUp");
		public static readonly DependencyProperty PreviewMouseLeftButtonUpProperty = DependencyProperty.RegisterAttached("PreviewMouseLeftButtonUp", typeof(ICommand), typeof(XEvtTrigger), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(XPreviewMouseLeftButtonUp.OnEvtChanged)));
		public static void SetPreviewMouseLeftButtonUp(UIElement ele, ICommand value) { ele.SetCurrentValue(PreviewMouseLeftButtonUpProperty, value); }
		public static ICommand GetPreviewMouseLeftButtonUp(UIElement ele) { return (ICommand)ele.GetValue(PreviewMouseLeftButtonUpProperty); }

		public static EvtToCmd<RoutedEventArgs> XChecked = new EvtToCmd<RoutedEventArgs>("Checked");
		public static readonly DependencyProperty CheckedProperty = DependencyProperty.RegisterAttached("Checked", typeof(ICommand), typeof(XEvtTrigger), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(XChecked.OnEvtChanged)));
		public static void SetChecked(UIElement ele, ICommand value) { ele.SetCurrentValue(CheckedProperty, value); }
		public static ICommand GetChecked(UIElement ele) { return (ICommand)ele.GetValue(CheckedProperty); }

		public static EvtToCmd<RoutedEventArgs> XUnchecked = new EvtToCmd<RoutedEventArgs>("Unchecked");
		public static readonly DependencyProperty UncheckedProperty = DependencyProperty.RegisterAttached("Unchecked", typeof(ICommand), typeof(XEvtTrigger), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(XUnchecked.OnEvtChanged)));
		public static void SetUnchecked(UIElement ele, ICommand value) { ele.SetCurrentValue(UncheckedProperty, value); }
		public static ICommand GetUnchecked(UIElement ele) { return (ICommand)ele.GetValue(UncheckedProperty); }

		//private static void OnMouseLeftButtonUpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		//	var ele = d as UIElement;
		//	var cmd = e.NewValue as ICommand;
		//	if (ele != null) {
		//		if (cmd != null) {
		//			mapCmdMouseLeftButtonUp[ele] = cmd;
		//			ele.MouseLeftButtonUp += OnMouseLeftButtonUp;
		//		} else {
		//			ele.MouseLeftButtonUp -= OnMouseLeftButtonUp;
		//			if (mapCmdMouseLeftButtonUp.ContainsKey(ele)) {
		//				mapCmdMouseLeftButtonUp.Remove(ele);
		//			}
		//		}
		//	}
		//}

		//private static void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseEventArgs e) {
		//	var ele = sender as UIElement;
		//	if (ele == null) {
		//		return;
		//	}
		//	if (!mapCmdMouseLeftButtonUp.ContainsKey(ele)) {
		//		return;
		//	}

		//	mapCmdMouseLeftButtonUp[ele].Execute(null);
		//}

	}
}
