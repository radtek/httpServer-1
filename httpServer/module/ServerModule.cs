using com.superscene.util;
using httpServer.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.module {
	public class ServerModule {
		public int idx = -1;
		public ServerItem serverItem = null;

		public string type = "";

		public string desc = "";	//描述
		public bool isRun = false;  //是否启动

		//public string ip = "127.0.0.1";		//
		//public string port = "";

		//public string path = "";	//路径

		//public bool isProxy = false;	//反向代理
		//public string proxyUrl = "";

		//public bool isTransmit = false;	//端口转发
		//public string transmitUrl = "";

		public virtual void load(RegistryCtl regCtl, string subPath) {
			type = regCtl.getValue(subPath + "type", "httpServer");
			desc = regCtl.getValue(subPath + "desc");
			isRun = regCtl.getValueBool(subPath + "isRun");
		}

		public virtual void save(RegistryCtl regCtl, string subPath) {
			regCtl.setValue(subPath + "type", type);
			regCtl.setValue(subPath + "desc", desc);
			regCtl.setValueBool(subPath + "isRun", isRun);
		}

		public virtual int getMaxPort() {
			return Entity.getInstance().mainModule.startPort;
		}
	}
}
