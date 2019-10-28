using csharpHelp;
using csharpHelp.util;
using httpServer.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.model {
	public class ServerMd {
		//public int idx = -1;
		public ServerItem serverItem = null;

		[XmlAttr("type")] public string type = "";

		[XmlValue("desc")] public string desc = "";    //描述
		[XmlAttr("isRun")] public bool isRun = false;  //是否启动

		public virtual void load(XmlCtl xmlCtl) {
			type = xmlCtl.attr("type");
			desc = xmlCtl.value("desc");
			isRun = xmlCtl.attrBool("isRun");
		}

		//public virtual void load(RegistryCtl regCtl, string subPath) {
		//	type = regCtl.getValue(subPath + "type", "httpServer");
		//	desc = regCtl.getValue(subPath + "desc");
		//	isRun = regCtl.getValueBool(subPath + "isRun");
		//}

		//public virtual void save(RegistryCtl regCtl, string subPath) {
		//	regCtl.setValue(subPath + "type", type);
		//	regCtl.setValue(subPath + "desc", desc);
		//	regCtl.setValueBool(subPath + "isRun", isRun);
		//}

		public virtual int getMaxPort() {
			return Entity.getInstance().mainMd.startPort;
		}
	}
}
