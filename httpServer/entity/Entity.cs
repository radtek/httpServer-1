using csharpHelp.util;
using httpServer.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.entity {
	class Entity {
		public static Entity instance = null;
		public static Entity getInstance() {
			if(instance == null) {
				instance = new Entity();
			}

			return instance;
		}

		public MainMd mainMd = null;
		public MainWindow mainWin = null;
	}
}
