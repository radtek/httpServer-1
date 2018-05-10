using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace csharpHelp.util {
    class TcpCtl {
		public string ip = "";
		public string port = "";
		//Thread thCtl = null;
		//Thread thDeal = null;
		int rcvSize = 30 * 1024 * 1024;
		int sedSize = 30 * 1024 * 1024;
		int maxPackageSize = 100 * 1024 * 1024;

		TcpListener server = null;
		TcpClient client = null;
		NetworkStream dataStream = null;
		Action<byte[]> callback = null;

		List<byte[]> lstBuffer = new List<byte[]>();
		byte[] lenBuffer = new byte[4];
		int lenBufLen = 0;
		bool waitLen = true;
		byte[] rstBuffer = null;
		int rstLen = 0;
		int rstPos = 0;

		object asyncSend = new object();

		public TcpCtl() {
			
		}

		public TcpCtl(string _ip, string _port) {
			ip = _ip;
			port = _port;
		}

		public void listen() {
			IPAddress ipAddr = IPAddress.Parse(ip);
			int iPort = Int32.Parse(port);

			server = new TcpListener(ipAddr, iPort);
			server.Start();

			client?.Close();

			client = server.AcceptTcpClient();
			client.ReceiveBufferSize = rcvSize;
			client.SendBufferSize = sedSize;

			dataStream = client.GetStream();
			//listenData();
		}

		public void wait() {
			TcpClient clientTmp = server.AcceptTcpClient();
			clientTmp.ReceiveBufferSize = rcvSize;
			clientTmp.SendBufferSize = sedSize;

			client?.Close();
			client = clientTmp;

			dataStream = client.GetStream();
		}

		public void connect() {
			IPAddress ipAddr = IPAddress.Parse(ip);
			int iPort = Int32.Parse(port);

			client?.Close();

			client = new TcpClient();
			client.Connect(ipAddr, iPort);
			client.ReceiveBufferSize = rcvSize;
			client.SendBufferSize = sedSize;

			dataStream = client.GetStream();
			//listenData();
		}

		public void listenData(Action<byte[]> _callback) {
			callback = _callback;

			do {
				try {
					byte[] data = readStream(dataStream);
					//Debug.WriteLine("aa:" + data.Length);

					dealData(data, 0);

					continue;
				} catch(Exception) {
					//Debug.WriteLine(ex.ToString());
					Thread.Sleep(20);
				}
			} while(true);
		}

		private void dealData(byte[] data, int startIdx) {
			int nextPos = 0;
			//等待长度数据
			if(waitLen) {
				int lastLen = 4 - lenBufLen;
				if(data.Length - startIdx < lastLen) {
					Array.Copy(data, startIdx, lenBuffer, lenBufLen, data.Length - startIdx);
					lenBufLen += data.Length - startIdx;
					return;
				} else {
					Array.Copy(data, startIdx, lenBuffer, lenBufLen, lastLen);
					lenBufLen = 0;
					nextPos = startIdx + lastLen;
					rstLen = BitConverter.ToInt32(lenBuffer, 0);
					rstPos = 0;
					if(rstLen > maxPackageSize) {
						return;
					}
					//Debug.WriteLine("len:" + rstLen);
					rstBuffer = new byte[rstLen];
					waitLen = false;
				}
			}

			//半包
			if(rstLen - rstPos > data.Length - nextPos) {
				Array.Copy(data, nextPos, rstBuffer, rstPos, data.Length - nextPos);
				rstPos = rstPos + data.Length - nextPos;
				return;
			}

			//callback
			//Debug.WriteLine(nextPos + "," + rstPos + "," + (rstLen - rstPos) + "," + data.Length + "," + rstBuffer.Length);
			Array.Copy(data, nextPos, rstBuffer, rstPos, rstLen - rstPos);
			callback?.Invoke(rstBuffer);

			nextPos = nextPos + rstLen - rstPos;

			//clear buffer
			rstBuffer = null;
			rstLen = 0;
			rstPos = 0;
			lenBufLen = 0;
			waitLen = true;

			if(nextPos >= data.Length) {
				return;
			}

			//粘包
			dealData(data, nextPos);
		}

		public void send(byte[] data) {
			byte[] bufLen = BitConverter.GetBytes((Int32)data.Length);

			//dataStream?.WriteAsync(bufLen, 0, bufLen.Length);
			//dataStream?.WriteAsync(data, 0, data.Length);
			dataStream?.Write(bufLen, 0, bufLen.Length);
			dataStream?.Write(data, 0, data.Length);
		}

		public void sendAsync(byte[] data) {
			byte[] bufLen = BitConverter.GetBytes((Int32)data.Length);
			byte[] bufRst = new byte[bufLen.Length + data.Length];
			Array.Copy(bufLen, bufRst, bufLen.Length);
			Array.Copy(data, 0, bufRst, bufLen.Length, data.Length);

			dataStream?.WriteAsync(bufRst, 0, bufRst.Length);
			//lock (asyncSend) {
			//	dataStream?.WriteAsync(bufLen, 0, bufLen.Length);
			//	dataStream?.WriteAsync(data, 0, data.Length);
			//}
			//dataStream?.Write(bufLen, 0, bufLen.Length);
			//dataStream?.Write(data, 0, data.Length);
		}

		public void send(string data, Encoding encoding = null) {
			if(encoding == null) {
				encoding = Encoding.Default;
			}
			byte[] bufData = encoding.GetBytes(data);
			send(bufData);
		}

		public void clear() {
			client?.Close();
			client = null;
			server?.Stop();
			server = null;

			dataStream = null;
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

					if(readCount == bufSize) {
						lstData.Add(temp);
						temp = new byte[bufSize];
					} else {
						byte[] newTemp = new byte[readCount];
						Array.Copy(temp, newTemp, readCount);
						lstData.Add(newTemp);
					}
					//result += Encoding.UTF8.GetString(data, 0, readCount);
				} while(readCount >= bufSize);

				//result = Encoding.UTF8.GetString(data, 0, totaLen);

				byte[] result = new byte[totaLen];
				int dstIdx = 0;
				for(int i = 0; i < lstData.Count; ++i) {
					int srcLen = lstData[i].Length;
					Array.Copy(lstData[i], 0, result, dstIdx, srcLen);
					dstIdx += srcLen;
				}

				return result;
			} catch(Exception) {
				//Debug.WriteLine(ex.ToString());
				return new byte[0];
			}
		}
	}
}
