using httpServer.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.assembly.util {
	class UiService {
		public static UiService _ins = null;
		public static UiService ins() {
			if (_ins == null) {
				_ins = new UiService();
			}

			return _ins;
		}

		List<string> lstIP = new List<string>();

		public List<string> getAllIp() {
			if (lstIP.Count <= 0) {
				lstIP = CommonUtil.findAllIp();
			}

			return lstIP;
		}

	}
}
