using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.control {
	public class Cache {
		Dictionary<string, byte[]> mapData = new Dictionary<string, byte[]>();
		public byte[] getFile(string path) {
			if(mapData.ContainsKey(path)) {
				//Debug.WriteLine(path);
				return mapData[path];
			}

			byte[] buffer = new byte[0];

			using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				buffer = new byte[fs.Length];
				fs.Read(buffer, 0, (int)fs.Length); //将文件读到缓存区
			};

			mapData[path] = buffer;
			return buffer;
		}
	}
}
