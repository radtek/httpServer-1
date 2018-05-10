using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	public class StringReplace {
		public Dictionary<string, string> mapParam = new Dictionary<string, string>();

		public string getString(string data) {
			string result = data;
			foreach(string key in mapParam.Keys) {
				result = result.Replace(key, mapParam[key]);
			}

			return result;
		}
	}
}
