using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace csharpHelp.ui {
	/// <summary>是否为设计模式binding</summary>
	public class IsDesignBind : Binding {
		public IsDesignBind() {
			RelativeSource = new RelativeSource() {
				Mode = RelativeSourceMode.Self
			};
			//Path = DesignerProperties.IsInDesignModeProperty;
			Path = new System.Windows.PropertyPath(DesignerProperties.IsInDesignModeProperty);
		}
	}
}
