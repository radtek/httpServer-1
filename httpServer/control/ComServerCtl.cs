using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace httpServer.control {
	class ComServerCtl {
		public static bool isDescIp(string desc) {
			return Regex.IsMatch(desc, "^[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}:[0-9]{1,5}");
		}
	}
}
