using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace com.superscene.util {
	public class ComUtil {
		public static SolidColorBrush createBrush(string color) {
			return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
		}

		public static int parseInt(string data, int defValue = 0) {
			int result = 0;
			if(Int32.TryParse(data, out result)) {
				return result;
			}

			return defValue;
		}
	}
}
