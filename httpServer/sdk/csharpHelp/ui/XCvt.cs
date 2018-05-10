using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace csharpHelp.ui {
	[ValueConversion(typeof(int), typeof(bool))]
	class XCvt : IValueConverter {
		public static readonly XCvt ins = new XCvt();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			//Debug.WriteLine("aa:" + (value as int?));
			//Debug.WriteLine("bb:" + parameter.GetType());

			int? iValue = value as int?;
			string param = parameter as string;
			int iParam = 0;
			bool isOk = int.TryParse(param, out iParam);
			return iValue > iParam;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return -1;
		}

	}
}
