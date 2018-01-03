using httpServer.assembly.page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.module {
	class MainModule {

		public Dictionary<string, IPage> mapServer = new Dictionary<string, IPage>();

		public List<ServerModule> lstServer = new List<ServerModule>();
	}
}
