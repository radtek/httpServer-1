using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.model {
	public class LocalRes {
		public static string rootPath() {
			return "/httpServer;component/resource/";
		}

		public static string statusRun() {
			return rootPath() + "status_run.png";
		}

		public static string statusStop() {
			return rootPath() + "status_stop.png";
		}

		public static string icon16() {
			return "pack://application:,,,/httpServer;component/resource/icon16.png";
			//return rootPath() + "icon16.png";
		}
	}
}
