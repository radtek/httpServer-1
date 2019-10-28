using httpServer.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.control {
	public interface IServerCtl {
		void setModel(ServerMd _md);
		void restartServer();
		void clear();
	}
}
