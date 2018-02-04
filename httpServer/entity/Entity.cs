using com.superscene.util;
using httpServer.module;
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

		public MainModule mainModule = null;
		public MainWindow mainWin = null;
		
		public TimeTest tmTest = new TimeTest();
	}
}
