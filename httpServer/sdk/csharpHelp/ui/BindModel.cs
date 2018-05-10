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

namespace csharpHelp.util {
	public class BindModel<T> {
		private Dictionary<T, BindBase> mapData = new Dictionary<T, BindBase>();

		public void bind(T name, Control ctl, DependencyProperty bindType, object initData = null) {
			BindBase md = new BindBase();
			md.data = initData;
			ctl.SetBinding(bindType, new Binding("data") { Source=md, UpdateSourceTrigger= UpdateSourceTrigger.PropertyChanged });
			mapData[name] = md;
		}

		public void bindCheckBox(T name, CheckBox chk, bool isChecked = false) {
			bind(name, chk, CheckBox.IsCheckedProperty, isChecked);
		}

		public void setData(T name, object data) {
			mapData[name].data = data;
		}

		public object getData(T name) {
			return mapData[name].data;
		}

		public bool getBool(T name) {
			return (bool)mapData[name].data;
		}

		public string getString(T name) {
			return (string)mapData[name].data;
		}

		public int getInt(T name) {
			return (int)mapData[name].data;
		}
	}

	public class BindModelSingle : BindModel<Control> {
		public void bind(Control ctl, DependencyProperty bindType, object initData = null) {
			bind(ctl, ctl, bindType, initData);
		}

		public void bindCheckBox(CheckBox ctl, bool isChecked = false) {
			bind(ctl, ctl, CheckBox.IsCheckedProperty, isChecked);
		}

		public void bindProgressBar(ProgressBar ctl, double idx = 0) {
			bind(ctl, ctl, ProgressBar.ValueProperty, idx);
		}

		public void bindTextbox(TextBox ctl, string text = "") {
			bind(ctl, ctl, TextBox.TextProperty, text);
		}

		public void bindLabel(Label ctl, string text = "") {
			bind(ctl, ctl, Label.ContentProperty, text);
		}
	}

	public class BindBase : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged = null;

		private object _data;
		public object data {
			get { return _data; }
			set {
				//Debug.WriteLine("aa:" + value);
				_data = value;
				initEvt("data");
			}
		}

		//public void bindCheckBox(CheckBox chk, string name) {
		//	chk.SetBinding(CheckBox.IsCheckedProperty, new Binding(name) { Source=this });
		//}

		protected void initEvt(string attr) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attr));
		}
	}


}
