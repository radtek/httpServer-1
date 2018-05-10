using System;
using System.Collections.Generic;

namespace csharpHelp.util.service {
	/// <summary>
	/// 命令服务
	/// </summary>
	public class CommandService {
		public static CommandService instance = null;
		public delegate void OnTiggered(string name, Object data);

		private Dictionary<string, Dictionary<OnTiggered, string>> mapFuns = new Dictionary<string, Dictionary<OnTiggered, string>>();

		public void init(){
		
		}

		public static CommandService getInstance() {
			if(instance == null) {
				instance = new CommandService();
			}
			return instance;
		}

		//public void register(string name, ) {
		
		//}

		public void listen(string name, OnTiggered onTiggered) {
			if(!mapFuns.ContainsKey(name)) {
				mapFuns[name] = new Dictionary<OnTiggered, string>();
			}

			mapFuns[name][onTiggered] = "";
		}

		public void remove(string name, OnTiggered onTiggered) {
			if(!mapFuns.ContainsKey(name)) {
				return;
			}

			mapFuns[name].Remove(onTiggered);
		}

		public void send(string name, Object data = null) {
			if(!mapFuns.ContainsKey(name)) {
				return;
			}

			foreach(OnTiggered key in mapFuns[name].Keys) {
				key(name, data);
			}
		}

	}
}
