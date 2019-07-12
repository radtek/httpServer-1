using httpServer.assembly.page;
using httpServer.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.control {
	public class HttpServerGroup {
		//HttpHandler handler = new HttpHandler();
		public HttpModel md = null;

		int[] arrPort = new int[] { 4042, 4124, 4282 };

		List<ServerCtl> lstHttpServer = new List<ServerCtl>();

		internal void restartServer() {
			clear();

			if(md.ip != "0.0.0.0") {
				ServerCtl srv = new ServerCtl();
				lstHttpServer.Add(srv);
				//md.ctl = srv;
				srv.ip = md.ip;
				srv.md = md;
				srv.restartServer();
				return;
			}

			List<string> lstAllIp = ComUtil.findAllIp();
			lstAllIp.Add("localhost");

			HashSet<string> hashUsedIpPort = ComUtil.allUsedIpPort();

			for(int i = 0; i < lstAllIp.Count; ++i) {
				int port = findUsedPort(lstAllIp[i], hashUsedIpPort);

				ServerCtl srv = new ServerCtl();
				lstHttpServer.Add(srv);
				
				srv.ip = lstAllIp[i];
				srv.md = md;

				srv.restartServer();
			}
		}

		private int findUsedPort(string ip, HashSet<string> hashUsedIpPort) {
			int portIdx = 0;
			for(int j = 0; j < arrPort.Length; ++j) {
				string url = ip + ":" + arrPort[j];
				if(!hashUsedIpPort.Contains(url)) {
					portIdx = j;
					break;
				}
			}

			return arrPort[portIdx];
		}

		internal void clear() {
			lstHttpServer.ForEach(srv => srv.clear());

			lstHttpServer.Clear();
		}
	}
}
