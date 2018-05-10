using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	public class Cmd {
		List<Action> lstFun = new List<Action>();

		public void listen(Action fun) {
			lstFun.Add(fun);
		}

		public void send() {
			for(int i = 0; i < lstFun.Count; ++i) {
				lstFun[i].Invoke();
			}
		}
	}

	public class Cmd<T> {
		List<Action<T>> lstFun1 = new List<Action<T>>();

		public void listen(Action<T> fun) {
			lstFun1.Add(fun);
		}

		public void send(T data) {
			for(int i = 0; i < lstFun1.Count; ++i) {
				lstFun1[i].Invoke(data);
			}
		}
	}
}
