using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	public class SingleEvent {
		//List<Action<SingleEvent, Object>> lstFun = new List<Action<SingleEvent, object>>();
		private Dictionary<Action<SingleEvent>, Object> mapFun = new Dictionary<Action<SingleEvent>, object>();

		public static SingleEvent operator +(SingleEvent c1, Action<SingleEvent> c2) {
			SingleEvent evt = new SingleEvent();
			evt.mapFun = new Dictionary<Action<SingleEvent>, object>();
			foreach(var key in c1.mapFun.Keys) {
				evt.mapFun[key] = c1.mapFun[key];
			}

			evt.mapFun[c2] = null;


			return evt;
		}

		public void listen(Action<SingleEvent> fun) {

		}

		public void send() {
			foreach(var key in mapFun.Keys) {
				key(this);
			}
		}
	}

	public class SingleEvents {
		//List<Action<SingleEvent, Object>> lstFun = new List<Action<SingleEvent, object>>();
		private Dictionary<Action<SingleEvents, Object>, Object> mapFun = new Dictionary<Action<SingleEvents, object>, object>();

		public static SingleEvents operator +(SingleEvents c1, Action<SingleEvents, Object> c2){
			SingleEvents evt = new SingleEvents();
			evt.mapFun = new Dictionary<Action<SingleEvents, object>, object>();
			foreach(var key in c1.mapFun.Keys) {
				evt.mapFun[key] = c1.mapFun[key];
			}

			evt.mapFun[c2] = null;


			return evt;
        }

		public void listen(Action<SingleEvents, Object> fun, Object data) {

		}

		public void send(Object data = null) {
			foreach(var key in mapFun.Keys) {
				key(this, data);
			}
		}
	}
}
