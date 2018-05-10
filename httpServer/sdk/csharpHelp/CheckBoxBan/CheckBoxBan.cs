using System;
using System.Collections.Generic;
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
	public class CheckBoxBan : CheckBox {
		static CheckBoxBan() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckBoxBan), new FrameworkPropertyMetadata(typeof(CheckBoxBan)));
		}

		public CheckBoxBan() : base() {
			PreviewMouseLeftButtonDown += (tag, e) => {
				if(!AllowChange) {
					e.Handled = true;
				}
			};
			PreviewMouseLeftButtonUp += (tag, e) => {
				if(!AllowChange) {
					e.Handled = true;
				}
			};
			Checked += (object sender, RoutedEventArgs e) => {
				if (e.OriginalSource != this) {
					e.Handled = true;
				}
			};
			Unchecked += (object sender, RoutedEventArgs e) => {
				if (e.OriginalSource != this) {
					e.Handled = true;
				}
			};
		}

		//AllowChange
		public static readonly DependencyProperty AllowChangeProperty = DependencyProperty.Register("AllowChange", typeof(bool), typeof(CheckBoxBan), new PropertyMetadata(true));
		public bool AllowChange {
			get { return (bool)GetValue(AllowChangeProperty); }
			set { SetCurrentValue(AllowChangeProperty, value); }
		}

	}
}
