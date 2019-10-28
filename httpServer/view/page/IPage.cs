using httpServer.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.view.page {
	public interface IPage {
		void init();
		void initData(ServerMd md);

		ServerMd createModel();
		ServerMd createNewModel();

		void updateData(ServerMd md);

		void start();
		void stop();
		void clear(ServerMd md);
	}
}
