using httpServer.assembly.page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.module {
	class MainModule {
		public int startPort = 8091;
		public int maxPort = 8200;

		public Dictionary<string, IPage> mapServer = new Dictionary<string, IPage>();

		public List<ServerModule> lstServer = new List<ServerModule>();

		//public void updateStartPort(int usedPort) {
		//	startPort = Math.Max(startPort, Math.Min(usedPort + 1, 8200));
		//}

		//public void updateStartPort(string usedPort) {
		//	int iPort = 0;
		//	bool isOk = Int32.TryParse(usedPort, out iPort);
		//	if(!isOk) {
		//		return;
		//	}

		//	updateStartPort(iPort);
		//}

		public int maxStartPort() {
			int port = startPort;
			for(int i = 0; i < lstServer.Count; ++i) {
				if(port > 8200) {
					continue;
				}
				port = Math.Max(port, lstServer[i].getMaxPort());
			}
			port = Math.Min(port + 1, 8200);
			return port;
		}
	}
}
