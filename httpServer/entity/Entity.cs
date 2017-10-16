using httpServer.module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.entity {
	public class Entity {
		public static Entity instance = null;
		public static Entity getInstance() {
			if(instance == null) {
				instance = new Entity();
			}

			return instance;
		}

		public MainModule mainModule = null;

	}
}
