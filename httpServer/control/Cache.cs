using com.superscene.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

			//内存映射文件
			try {
				FileInfo fileInfo = new FileInfo(path);

				IntPtr hFile = Kernel32.CreateFile(path, Kernel32.GENERIC_READ, Kernel32.FILE_SHARE_READ, IntPtr.Zero, Kernel32.OPEN_EXISTING, Kernel32.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
				if((int)hFile != Kernel32.INVALID_HANDLE_VALUE) {
					var hFileMap = Kernel32.CreateFileMapping(hFile, IntPtr.Zero, (uint)Kernel32.PAGE_READONLY, 0, (uint)fileInfo.Length, null);
					if(hFileMap != IntPtr.Zero) {
						IntPtr diskBuf = Kernel32.MapViewOfFile(hFileMap, Kernel32.FILE_MAP_READ, 0, 0, (uint)fileInfo.Length);
						if(diskBuf != IntPtr.Zero) {
							buffer = new byte[fileInfo.Length];
							Marshal.Copy(diskBuf, buffer, 0, (int)fileInfo.Length);
						}
						Kernel32.CloseHandle(hFileMap);
					}
					Kernel32.CloseHandle(hFile);
				}
			} catch(Exception ex) {
				Console.WriteLine(ex.ToString());
			}

			//read file
			//using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
			//	buffer = new byte[fs.Length];
			//	fs.Read(buffer, 0, (int)fs.Length); //将文件读到缓存区
			//};

			mapData[path] = buffer;
			return buffer;
		}
	}
}
