using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util.service {
	public static class ComExtension {
		public static void trace(this object value) {
			Debug.WriteLine(value);
		}

		public static string lastChar(this string value) {
			return value.Length <= 0 ? "" : value[value.Length - 1].ToString();
		}

	}
}
