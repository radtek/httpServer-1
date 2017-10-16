using com.superscene.util;
using httpServer.entity;
using httpServer.module;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.control {
	public class ServerDataCtl {
		RegistryCtl regCtl = new RegistryCtl();
		string regPath = "HKCU\\Software\\superHttpServer\\";

		public void save() {
			List<ServerModule> lstServer = Entity.getInstance().mainModule.lstServer;

			try {
				regCtl.clearChildItem(regPath);

				for(int i = 0; i < lstServer.Count; ++i) {
					ServerModule md = lstServer[i];

					string subPath = regPath + i + "\\";

					regCtl.setValue(subPath + "desc", md.desc);
					regCtl.setValueBool(subPath + "isRun", md.isRun);

					regCtl.setValue(subPath + "ip", md.ip);
					regCtl.setValue(subPath + "port", md.port);

					regCtl.setValue(subPath + "path", md.path);

					regCtl.setValueBool(subPath + "isProxy", md.isProxy);
					regCtl.setValue(subPath + "proxyUrl", md.proxyUrl);

					regCtl.setValueBool(subPath + "isTransmit", md.isTransmit);
					regCtl.setValue(subPath + "transmitUrl", md.transmitUrl);
				}
			} catch(Exception) {

			}
		}

		public void load() {
			string rootPath = AppDomain.CurrentDomain.BaseDirectory;

			List<ServerModule> lstServer = Entity.getInstance().mainModule.lstServer;
			lstServer.Clear();

			try {
				regCtl.each(regPath, (name) => {
					string subPath = regPath + name + "\\";

					ServerModule md = new ServerModule();

					md.desc = regCtl.getValue(subPath + "desc");
					md.isRun = regCtl.getValueBool(subPath + "isRun");

					md.ip = regCtl.getValue(subPath + "ip", "127.0.0.1");
					md.port = regCtl.getValue(subPath + "port", "8091");

					md.path = regCtl.getValue(subPath + "path", rootPath);

					md.isProxy = regCtl.getValueBool(subPath + "isProxy");
					md.proxyUrl = regCtl.getValue(subPath + "proxyUrl", "");

					md.isTransmit = regCtl.getValueBool(subPath + "isTransmit");
					md.transmitUrl = regCtl.getValue(subPath + "transmitUrl", "");

					md.isProxy = false;
					md.isTransmit = false;

					lstServer.Add(md);
				});
			} catch(Exception) {

			}



		}
	}
}
