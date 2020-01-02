using csharpHelp;
using httpServer.view.page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.model {
	class MainMd {
		public static MainMd ins = new MainMd();

		public XmlSerialize xmlConfig = new XmlSerialize();
		public ClassLink mdLink = new ClassLink();

		public int startPort = 8091;
		public int maxPort = 8200;

		public ConfigMd configMd = new ConfigMd();
		public Lang lang = new Lang();

		public int maxStartPort() {
			var lst = configMd.lstHttpServer;

			int port = startPort;
			for(int i = 0; i < lst.Count; ++i) {
				if(port > 8200) {
					continue;
				}
				port = Math.Max(port, lst[i].getMaxPort());
			}
			port = Math.Min(port + 1, 8200);
			return port;
		}
	}
}
