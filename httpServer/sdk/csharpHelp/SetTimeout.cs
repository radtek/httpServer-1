using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace csharpHelp.util {
	class SetTimeoutItem {
		public long startTime = 0;
		public long waitTime = 0;
		public Action cb;

		public SetTimeoutItem(){ }
		public SetTimeoutItem(long _startTime, long _waitTime, Action _cb) { startTime = _startTime; waitTime = _waitTime; cb = _cb; }
	}

	public class SetTimeout {
		private int timeGap = 16;
		private List<SetTimeoutItem> lstData = new List<SetTimeoutItem>();
		private Timer timer = null;
		private long startTime = 0;
		private long nowTime = 0;

		public void wait(Action callback, int waitTime) {
			long nowTime = getTimestamp();
			SetTimeoutItem md = new SetTimeoutItem(nowTime, waitTime, callback);
			lstData.Add(md);

			if(timer == null) {
				restart();
			}
		}

		private void restart() {
			if(timer != null) {
				timer.Dispose();
			}
			startTime = nowTime = getTimestamp();
			timer = new Timer(timerProc, null, 0, timeGap);
		}

		public void clear() {
			timer?.Dispose();
			lstData.Clear();
			timer = null;
		}

		private void timerProc(object state) {
			nowTime += timeGap;
			//Debug.WriteLine("aa:" + nowTime);

			if(nowTime >= startTime + 60 * 1000) {
				nowTime = getTimestamp();
			}

			try {
				for(int i = lstData.Count - 1; i >= 0; --i) {
					if(lstData[i].startTime + lstData[i].waitTime > nowTime) {
						continue;
					}
					lstData[i].cb?.Invoke();
					lstData.RemoveAt(i);
				}
				if(lstData.Count <= 0) {
					timer.Dispose();
					timer = null;
				}
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		private long getTimestamp() {
			TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return (long)(ts.TotalMilliseconds);
		}
	}
}
