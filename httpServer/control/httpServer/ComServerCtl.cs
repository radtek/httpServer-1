using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace httpServer.control {
	class ComServerCtl {
		public static bool isDescIp(string desc) {
			return Regex.IsMatch(desc, "^[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}:[0-9]{1,5}$");
		}

		//read form stream
		public static byte[] readStream(Stream stream) {
			try {
				List<byte[]> lstData = new List<byte[]>();
				//string result = "";
				int bufSize = 10240;
				byte[] temp = new byte[bufSize];

				int totaLen = 0;
				int readCount = 0;

				do {
					readCount = stream.Read(temp, 0, bufSize);
					totaLen += readCount;

					if (readCount == bufSize) {
						lstData.Add(temp);
						temp = new byte[bufSize];
					} else {
						byte[] newTemp = new byte[readCount];
						Array.Copy(temp, newTemp, readCount);
						lstData.Add(newTemp);
					}
					//result += Encoding.UTF8.GetString(data, 0, readCount);
				} while (readCount >= bufSize);

				//result = Encoding.UTF8.GetString(data, 0, totaLen);

				byte[] result = new byte[totaLen];
				int dstIdx = 0;
				for (int i = 0; i < lstData.Count; ++i) {
					int srcLen = lstData[i].Length;
					Array.Copy(lstData[i], 0, result, dstIdx, srcLen);
					dstIdx += srcLen;
				}

				return result;
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				MainWindow.ins.log(ex);
				return new byte[0];
			}
		}
	}
}
