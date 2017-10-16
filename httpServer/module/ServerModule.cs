using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.module {
	public class ServerModule {
		public string desc = "";	//描述
		public bool isRun = false;	//是否启动

		public string ip = "127.0.0.1";		//
		public string port = "";

		public string path = "";	//路径
		
		public bool isProxy = false;	//反向代理
		public string proxyUrl = "";

		public bool isTransmit = false;	//端口转发
		public string transmitUrl = "";
	}
}
