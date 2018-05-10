using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	public class MultiAsyn {
		public Action onEnd = null;
		public bool isAsyn = true;
		public Action<Object> fun = null;

		//private int count = 0;
		private int nowIdx = 0;
		private List<Object> lstData = new List<Object>();
		//private Dictionary<Action<Object>, Object> mapFun = new Dictionary<Action<object>, object>();
		//private Dictionary<Object, Object> mapData = new Dictionary<object, object>();


		public void add(object userData = null) {
			//++count;
			//lstFun.Add(fun);
			//mapFun[fun] = userData;
			lstData.Add(userData);
		}

		public void clear() {
			//count = 0;
			nowIdx = 0;
			//mapFun.Clear();
			lstData.Clear();
		}

		public void addCount() {
			++nowIdx;
			if(nowIdx == lstData.Count) {
				onEnd?.Invoke();
				return;
			}

			if(!isAsyn) {
				fun?.Invoke(lstData[nowIdx]);
			}
		}

		public void run(Action<Object> _fun = null) {
			if(_fun != null) {
				fun = _fun;
			}

			if(lstData.Count == 0) {
				return;
			}

			nowIdx = 0;

			if(isAsyn) {
				for(int i = 0; i < lstData.Count; ++i) {
					fun?.Invoke(lstData[i]);
				}
			} else {
				fun?.Invoke(lstData[0]);
			}
			//foreach(var fun in mapFun.Keys) {
			//	//fun(mapFun[fun]);
			//	fun?.Invoke(mapFun[fun]);
			//}
		}

	}
}
