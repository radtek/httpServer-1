using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.control {
	interface IServerCtl {
		void restartServer();
		void clear();
	}
}
