using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace csharpHelp.ui {
	/// <summary></summary>
	public class BaseBehavior {
		public DependencyObject _element = null;

		public virtual void onAttached() {

		}

		public virtual void onDetaching() {

		}

	}

	public class BaseBehavior<T> : BaseBehavior where T : DependencyObject {
		public T element {
			get { return _element as T; }
			set { _element = value; }
		}
	}
}
