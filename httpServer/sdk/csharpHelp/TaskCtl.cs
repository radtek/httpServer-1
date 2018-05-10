using System;
using System.Collections.Generic;
using System.Threading;

namespace csharpHelp.util.action {
	/// <summary>
	/// 后台任务
	/// </summary>
	public class TaskCtl {
		//private static TaskCtl instance = null;

		public delegate void OnProc(Object data);
		private class TaskModel {
			public OnProc proc = null;
			public Object data = null;

			public TaskModel() { }
			public TaskModel(OnProc _proc, Object _data) {
				proc = _proc;
				data = _data;
			}
		};

		private Thread backThread = null;
		//share
		private Object locker = new Object();
		//private Queue<TaskModel> queProc = new Queue<TaskModel>();
		private Dictionary<UInt64, TaskModel> mapTask = new Dictionary<UInt64, TaskModel>();
		private UInt64 idIndex = 0;
		private UInt64 nowId = 0;
		
		public TaskCtl() {
			resetThread();
		}

		~TaskCtl() {
			Console.WriteLine("end");
			clear();
		}

		//public static TaskCtl getInstance() {
		//	if(instance == null) {
		//		instance = new TaskCtl();
		//	}
		//	return instance;
		//}

		public void clear() {
			backThread?.Abort();
			backThread = null;

			mapTask.Clear();
			idIndex = 0;
			nowId = 0;
		}

		private void resetThread() {
			if (backThread != null) {
				backThread.Abort();
				nowId = 0;
			}

			backThread = new Thread(delegate () {
				while (true) {
					bool isEmpty = false;
					TaskModel task = null;

					lock (locker) {
						if (mapTask.Count == 0) {
							isEmpty = true;
						} else {
							//UInt64 id = 0;
							nowId = 0;
							foreach (UInt64 key in mapTask.Keys) {
								nowId = key;
								break;
							}
							if (nowId != 0) {
								task = mapTask[nowId];
								//nowProc = task.proc;
								mapTask.Remove(nowId);
							}
							//Console.WriteLine(nowId);
							//task = queProc.Dequeue();
							//nowProc = task.proc;
						}
					}

					if (isEmpty) {
						Thread.Sleep(10);
						continue;
					}

					if (task != null) {
						task.proc(task.data);
					}
				}
			});
			backThread.Start();
		}

		public UInt64 add(OnProc proc, Object data = null) {
			if (backThread == null) {
				resetThread();
			}

			lock (locker) {
				++idIndex;
				if (idIndex == 0) { idIndex = 1; }
				//queProc.Enqueue(new TaskModel(proc, data));
				mapTask[idIndex] = new TaskModel(proc, data);
				return idIndex;
			}
		}

		public void remove(UInt64 id) {
			//bool isResetThread = false;
			lock (locker) {
				if (nowId == id) {
					backThread.Abort();
					resetThread();
					return;
				}

				if (mapTask.ContainsKey(id)) {
					mapTask.Remove(id);
				}

			}
		}

	}
}
