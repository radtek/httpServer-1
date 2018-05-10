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
	public class ButtonLabel : Label {
		static ButtonLabel() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonLabel), new FrameworkPropertyMetadata(typeof(ButtonLabel)));
		}

		private bool _isDown = false;

		public ButtonLabel() : base() {
			_isDown = false;
			MouseDown += delegate (object sender, MouseButtonEventArgs e) {
				_isDown = true;
			};
			MouseUp += delegate (object sender, MouseButtonEventArgs e) {
				if(_isDown) {

				}
				_isDown = false;
			};
		}

		private void ButtonLabel_MouseDown(object sender, MouseButtonEventArgs e) {
			throw new NotImplementedException();
		}
	}
}
