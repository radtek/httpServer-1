using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.control.httpRevProxy {
	class HttpPackModel {
		public string headTag = "${HTTP1.0}";
		public Int64 httpIdx = 0;
		public string url;
		public Dictionary<string, string> mapHead = new Dictionary<string, string>();
		public byte[] data = new byte[0];

		public byte[] pack() {
			List<int> lstSize = new List<int>();
			//string strRst = "";

			int totalCount = 0;

			byte[] bHeadTag = Encoding.UTF8.GetBytes(headTag);
			totalCount += bHeadTag.Length;

			//string url = System.Web.HttpUtility.UrlDecode(url);
			byte[] bUrl = Encoding.UTF8.GetBytes(url);
			totalCount += bUrl.Length;

			//lstSize.Add(bUrl.Length);
			//for (var i = 0; i < httpCnt.Request.Headers.Count; ++i) {
			//string headLen = mapHead.Count.ToString();
			List<byte[]> lstHead = new List<byte[]>();
			foreach (string key in mapHead.Keys) {
				byte[] bKey = Encoding.UTF8.GetBytes(key);
				byte[] bVal = Encoding.UTF8.GetBytes(mapHead[key]);
				lstHead.Add(bKey);
				lstHead.Add(bVal);
				if (bKey.Length > 0) {
					totalCount += bKey.Length;
				}
				if (bVal.Length > 0) {
					totalCount += bVal.Length;
				}
				//Debug.WriteLine("" + "," + key + "," + httpCnt.Request.Headers[key]);
			}
			totalCount += data.Length;

			totalCount += sizeof(Int64) + 2 + 2 + 2 * lstHead.Count + 4;

			byte[] rst = new byte[totalCount];
			var idx = 0;

			//head data
			//head tag
			Array.Copy(bHeadTag, 0, rst, idx, bHeadTag.Length);
			idx += bHeadTag.Length;
			//http idx
			byte[] bHttpIdx = BitConverter.GetBytes((Int64)httpIdx);
			Array.Copy(bHttpIdx, 0, rst, idx, sizeof(Int64));
			idx += sizeof(Int64);
			//url len
			byte[] bUrlLen = BitConverter.GetBytes((short)bUrl.Length);
			Array.Copy(bUrlLen, 0, rst, idx, 2);
			idx += 2;
			//head count
			//Debug.WriteLine("ddd:" + lstHead.Count);
			byte[] bHeadCount = BitConverter.GetBytes((short)lstHead.Count);
			Array.Copy(bHeadCount, 0, rst, idx, 2);
			idx += 2;
			//head count
			for (int i = 0; i < lstHead.Count; ++i) {
				int len = 0;
				if (lstHead[i].Length > 0) {
					len = lstHead[i].Length;
				}
				byte[] bCount = BitConverter.GetBytes((short)len);
				Array.Copy(bCount, 0, rst, idx, 2);
				idx += 2;
			}
			//data count
			byte[] bDataCount = BitConverter.GetBytes((int)data.Length);
			Array.Copy(bDataCount, 0, rst, idx, 4);
			idx += 4;

			//data
			//url
			Array.Copy(bUrl, 0, rst, idx, bUrl.Length);
			idx += bUrl.Length;
			//head
			for (int i = 0; i < lstHead.Count; ++i) {
				if (lstHead[i].Length > 0) {
					Array.Copy(lstHead[i], 0, rst, idx, lstHead[i].Length);
					idx += lstHead[i].Length;
				}
			}
			//data
			if (data.Length > 0) {
				Array.Copy(data, 0, rst, idx, data.Length);
			}

			return rst;
		}

		public void unPack(byte[] _data) {
			//Debug.WriteLine(_data.Length);
			mapHead = new Dictionary<string, string>();
			byte[] bHeadTag = Encoding.UTF8.GetBytes(headTag);

			int idx = 0;
			//head data
			//head tag
			if (bHeadTag.Length >= _data.Length) {
				return;
			}
			string headTagTmp = Encoding.UTF8.GetString(_data, idx, bHeadTag.Length);
			if (headTagTmp != headTag) {
				return;
			}
			idx += bHeadTag.Length;
			//http idx
			httpIdx = BitConverter.ToInt64(_data, idx);
			idx += sizeof(Int64);
			//url count
			int urlLen = BitConverter.ToInt16(_data, idx);
			idx += 2;
			//head count
			int headCount = BitConverter.ToInt16(_data, idx);
			idx += 2;
			//Debug.WriteLine("11:" + _data.Length + "," + headCount);
			//head count
			List<int> lstHeadCount = new List<int>();
			for (int i = 0; i < headCount; ++i) {
				int count = BitConverter.ToInt16(_data, idx);
				idx += 2;
				lstHeadCount.Add(count);
			}
			//data count
			int dataCount = BitConverter.ToInt32(_data, idx);
			idx += 4;

			//data
			//url
			url = Encoding.UTF8.GetString(_data, idx, urlLen);
			idx += urlLen;
			//head
			for (int i = 0; i < headCount; i += 2) {
				string key = "";
				if (lstHeadCount[i] > 0) {
					if (idx + lstHeadCount[i] > _data.Length) {
						Debug.WriteLine("22:" + _data.Length + "," + headCount + "," + lstHeadCount[i]);
					}
					key = Encoding.UTF8.GetString(_data, idx, lstHeadCount[i]);
					idx += lstHeadCount[i];
				}

				string val = "";
				if (lstHeadCount[i + 1] > 0) {
					if (idx + lstHeadCount[i + 1] > _data.Length) {
						Debug.WriteLine("33:" + _data.Length + "," + headCount + "," + lstHeadCount[i]);
					}
					val = Encoding.UTF8.GetString(_data, idx, lstHeadCount[i + 1]);
					idx += lstHeadCount[i + 1];
				}
				mapHead[key] = val;
			}
			//data
			//int dataCount = _data.Length - idx;
			//Debug.WriteLine(dataCount + "," + idx + "," + _data.Length);
			if (dataCount > 0 && idx + dataCount <= _data.Length) {
				data = new byte[dataCount];
				Array.Copy(_data, idx, data, 0, dataCount);
			}
		}
	}
}
