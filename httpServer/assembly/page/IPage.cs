using httpServer.module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.assembly.page {
	interface IPage {
		void init();
		void initData(ServerModule md);

		ServerModule createModel();
		ServerModule createNewModel();

		void updateData(ServerModule md);

		void start();
		void stop();
		void clear(ServerModule md);
	}
}
