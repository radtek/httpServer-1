using com.superscene.util;
using httpServer.assembly.page;
using httpServer.entity;
using httpServer.module;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace httpServer.control.httpRevProxy {
	/// <summary>
	/// http反向代理客户端
	/// </summary>
	class HttpRevClient : IServerCtl {
		HttpRevModel md = null;

		Thread thCtl = null;
		Thread thDeal = null;
		//TcpListener server = null;
		//TcpClient client = null;
		TcpCtl tcpClient = null;
		HttpCtl httpCtl = new HttpCtl();
		List<byte[]> lstData = new List<byte[]>();
		object syncData = new object();
		object syncSend = new object();
		//NetworkStream dataStream = null;
		HttpClient httpClient = null;

		public HttpRevClient() {
			httpCtl.timeout = 5000;

			httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromMilliseconds(5000);
			httpClient.MaxResponseContentBufferSize = 10 * 1024 * 1024;
		}
		public HttpRevClient(ServerModule _md):this() {
			setModel(_md);
		}

		public void setModel(ServerModule _md) {
			md = (HttpRevModel)_md;
		}

		public void restartServer() {
			try {
				clear();

				//thDeal = new Thread(dealProc);
				//thDeal.Start();

				thCtl = new Thread(cltProc);
				thCtl.Start();

				//cltProc();

			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void cltProc() {
			try {
				//IPAddress ip = IPAddress.Parse(md.serverCtlIp);
				//int port = Int32.Parse(md.serverCtlPort);

				//client = new TcpClient();
				//client.Connect(ip, port);
				//client.ReceiveBufferSize = 10 * 1024 * 1024;
				//client.SendBufferSize = 10 * 1024 * 1024;

				//dataStream = client.GetStream();

				tcpClient = new TcpCtl(md.serverCtlIp, md.serverCtlPort);
				tcpClient.connect();
				tcpClient.listenData((data)=> {
					if(data.Length == 0) {
						return;
					}

					//lock(syncData) {
					//	lstData.Add(data);
					//}
					Task.Run(() =>{
						dealOne(data);
					});
				});
				//do {
				//	byte[] data = ComServerCtl.readStream(dataStream);

				//	if (data.Length == 0) {
				//		Thread.Sleep(50);
				//		continue;
				//	}
				//	lock (syncData) {
				//		lstData.Add(data);
				//	}
				//	continue;

				//	//HttpPackModel httpModel = new HttpPackModel();
				//	//httpModel.unPack(data);
				//	////Debug.WriteLine("bbbb:" + httpModel.url);

				//	////string strUrl = Encoding.UTF8.GetString(data);
				//	//string strUrl = httpModel.url;
				//	//strUrl = "http://" + md.localHttpIp + ":" + md.localHttpPort + strUrl;
				//	////int len = 0;
				//	////byte[] rst = httpCtl.httpGetByte(strUrl, out len);
				//	////dataStream.Write(rst, 0, len);
				//	////continue;

				//	//try {
				//	//	//HttpContent stringContent = new StringContent(data);
				//	//	using (var httpClient = new HttpClient()) {
				//	//		//using(var formData = new MultipartFormDataContent()) {
				//	//		//	formData.Add(stringContent, name, name);
				//	//		httpClient.Timeout = TimeSpan.FromMilliseconds(5000);

				//	//		HttpResponseMessage response = httpClient.GetAsync(strUrl).Result;
				//	//		bool isResponseOk = response.IsSuccessStatusCode;
				//	//		HttpStatusCode code = response.StatusCode;
				//	//		int len = 0;
				//	//		byte[] result = new byte[0];
				//	//		if (!isResponseOk) {
				//	//		} else {
				//	//			Stream sResult = response.Content.ReadAsStreamAsync().Result;
				//	//			result = readStreamByte(sResult, out len);
				//	//		}

				//	//		HttpPackModel httpModelRst = new HttpPackModel();
				//	//		httpModelRst.httpIdx = httpModel.httpIdx;
				//	//		httpModelRst.url = httpModel.url;
				//	//		//httpModelRst.mapHead["type"] = response.typ;
				//	//		var arr = response.Headers.ToList();
				//	//		//Debug.WriteLine("bbb:" + arr.Count);
				//	//		for (int i = 0; i < arr.Count; ++i) {
				//	//			string key = arr[i].Key;
				//	//			string val = "";
				//	//			if (arr[i].Value.Count() > 0) {
				//	//				//val = ((List<string>)arr[i].Value)[0];
				//	//				val = arr[i].Value.FirstOrDefault();
				//	//			}
				//	//			httpModelRst.mapHead[key] = val;
				//	//		}
				//	//		httpModelRst.mapHead["Status"] = ((int)code).ToString();
				//	//		if (response.Content.Headers.ContentType != null) {
				//	//			httpModelRst.mapHead["Content-Type"] = response.Content.Headers.ContentType.ToString();
				//	//		} else {
				//	//			httpModelRst.mapHead["Content-Type"] = "";
				//	//		}

				//	//		httpModelRst.data = new byte[len];
				//	//		Array.Copy(result, 0, httpModelRst.data, 0, len);

				//	//		byte[] rst = httpModelRst.pack();
				//	//		rst = httpModelRst.pack();
				//	//		//var httpIdx = BitConverter.ToInt64(rst, 0);
				//	//		//Debug.WriteLine("bb:" + httpModel.url + "," + rst.Length + "," + httpIdx);
				//	//		//Debug.WriteLine("bb:" + httpModel.url + "," + rst.Length);
				//	//		dataStream.Write(rst, 0, rst.Length);
				//	//		//dataStream.Close();

				//	//		//}
				//	//	}
				//	//} catch (Exception ex) {
				//	//	Debug.WriteLine(ex.ToString());
				//	//}

				//	//string aaa = Encoding.UTF8.GetString(rst);
				//	//Entity.getInstance().mainWin.log(aaa);
				//} while (true);

				//NetworkStream dataStream = client.GetStream();
				//string msg = "服务端亲启！";
				//byte[] buffer = Encoding.Default.GetBytes(msg);
				//dataStream.Write(buffer, 0, buffer.Length);
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		//TimeTest tm = new TimeTest();
		private void dealOne(byte[] data) {
			//TimeTest tm = new TimeTest();
			//tm.start();

			HttpPackModel httpModel = new HttpPackModel();
			httpModel.unPack(data);
			//Debug.WriteLine(httpModel.httpIdx + "," + httpModel.url + "," + data.Length);
			//Debug.WriteLine("bbbb:" + httpModel.url);

			//string strUrl = Encoding.UTF8.GetString(data);
			string strUrl = httpModel.url;
			strUrl = "http://" + md.localHttpIp + ":" + md.localHttpPort + strUrl;
			//int len = 0;
			//byte[] rst = httpCtl.httpGetByte(strUrl, out len);
			//dataStream.Write(rst, 0, len);
			//continue;

			try {
				//HttpContent stringContent = new StringContent(data);
				//httpClient = new HttpClient();
				//using (HttpClient httpClient = new HttpClient()) {
				//using(var formData = new MultipartFormDataContent()) {
				//	formData.Add(stringContent, name, name);
				//httpClient.Timeout = TimeSpan.FromMilliseconds(5000);
				//tm.tag();
				HttpResponseMessage response = httpClient.GetAsync(strUrl).Result;
				bool isResponseOk = response.IsSuccessStatusCode;
				HttpStatusCode code = response.StatusCode;
				int len = 0;
				byte[] result = new byte[0];
				if (!isResponseOk) {
				} else {
					Stream sResult = response.Content.ReadAsStreamAsync().Result;
					result = readStreamByte(sResult, out len);
				}

				HttpPackModel httpModelRst = new HttpPackModel();
				httpModelRst.httpIdx = httpModel.httpIdx;
				httpModelRst.url = httpModel.url;
				//httpModelRst.mapHead["type"] = response.typ;
				var arr = response.Headers.ToList();
				//Debug.WriteLine("bbb:" + arr.Count);
				for (int j = 0; j < arr.Count; ++j) {
					string key = arr[j].Key;
					string val = "";
					if (arr[j].Value.Count() > 0) {
						//val = ((List<string>)arr[i].Value)[0];
						val = arr[j].Value.FirstOrDefault();
					}
					httpModelRst.mapHead[key] = val;
				}
				httpModelRst.mapHead["Status"] = ((int)code).ToString();
				if (response.Content.Headers.ContentType != null) {
					httpModelRst.mapHead["Content-Type"] = response.Content.Headers.ContentType.ToString();
				} else {
					httpModelRst.mapHead["Content-Type"] = "";
				}

				httpModelRst.data = new byte[len];
				Array.Copy(result, 0, httpModelRst.data, 0, len);

				byte[] rst = httpModelRst.pack();
				rst = httpModelRst.pack();

				//var httpIdx = BitConverter.ToInt64(rst, 0);
				//Debug.WriteLine("bb:" + httpModel.url + "," + rst.Length + "," + httpIdx);
				//Debug.WriteLine("bb:" + httpModel.url + "," + rst.Length);
				//dataStream.Write(rst, 0, rst.Length);
				lock (syncSend) {
					//tcpClient.send(rst);
					Task.Run(() => {
						tcpClient.sendAsync(rst);
						//Debug.WriteLine(httpModel.url + "," + tm.end());
					});
				}
				//dataStream.Close();

				//Debug.WriteLine(httpModel.url + "," + tm.tag());
				//}
				//}
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void dealProc() {
			//tm.start();
			try {
				do {
					List<byte[]> lstDataTmp = null;

					lock (syncData) {
						lstDataTmp = lstData;
						if (lstData.Count > 0) {
							lstData = new List<byte[]>();
						}
					}

					if (lstDataTmp.Count <= 0 || tcpClient == null) {
						Thread.Sleep(100);
						continue;
					}

					for (int i = 0; i < lstDataTmp.Count; ++i) {
						byte[] data = lstDataTmp[i];

						HttpPackModel httpModel = new HttpPackModel();
						httpModel.unPack(data);
						//Debug.WriteLine(httpModel.httpIdx + "," + httpModel.url + "," + data.Length);
						//Debug.WriteLine("bbbb:" + httpModel.url);

						//string strUrl = Encoding.UTF8.GetString(data);
						string strUrl = httpModel.url;
						strUrl = "http://" + md.localHttpIp + ":" + md.localHttpPort + strUrl;
						//int len = 0;
						//byte[] rst = httpCtl.httpGetByte(strUrl, out len);
						//dataStream.Write(rst, 0, len);
						//continue;

						try {
							//HttpContent stringContent = new StringContent(data);
							//httpClient = new HttpClient();
							//using (HttpClient httpClient = new HttpClient()) {
							//using(var formData = new MultipartFormDataContent()) {
							//	formData.Add(stringContent, name, name);
							//httpClient.Timeout = TimeSpan.FromMilliseconds(5000);
							//tm.tag();
							HttpResponseMessage response = httpClient.GetAsync(strUrl).Result;
							bool isResponseOk = response.IsSuccessStatusCode;
							HttpStatusCode code = response.StatusCode;
							int len = 0;
							byte[] result = new byte[0];
							if (!isResponseOk) {
							} else {
								Stream sResult = response.Content.ReadAsStreamAsync().Result;
								result = readStreamByte(sResult, out len);
							}

							HttpPackModel httpModelRst = new HttpPackModel();
							httpModelRst.httpIdx = httpModel.httpIdx;
							httpModelRst.url = httpModel.url;
							//httpModelRst.mapHead["type"] = response.typ;
							var arr = response.Headers.ToList();
							//Debug.WriteLine("bbb:" + arr.Count);
							for (int j = 0; j < arr.Count; ++j) {
								string key = arr[j].Key;
								string val = "";
								if (arr[j].Value.Count() > 0) {
									//val = ((List<string>)arr[i].Value)[0];
									val = arr[j].Value.FirstOrDefault();
								}
								httpModelRst.mapHead[key] = val;
							}
							httpModelRst.mapHead["Status"] = ((int)code).ToString();
							if (response.Content.Headers.ContentType != null) {
								httpModelRst.mapHead["Content-Type"] = response.Content.Headers.ContentType.ToString();
							} else {
								httpModelRst.mapHead["Content-Type"] = "";
							}

							httpModelRst.data = new byte[len];
							Array.Copy(result, 0, httpModelRst.data, 0, len);

							byte[] rst = httpModelRst.pack();
							rst = httpModelRst.pack();

							//var httpIdx = BitConverter.ToInt64(rst, 0);
							//Debug.WriteLine("bb:" + httpModel.url + "," + rst.Length + "," + httpIdx);
							//Debug.WriteLine("bb:" + httpModel.url + "," + rst.Length);
							//dataStream.Write(rst, 0, rst.Length);
							tcpClient.send(rst);
							//dataStream.Close();

							//Debug.WriteLine(httpModel.url + "," + tm.tag());
							//}
							//}
						} catch (Exception ex) {
							Debug.WriteLine(ex.ToString());
						}
					}
				} while (true);
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public byte[] readStreamByte(Stream stream, out int len) {
			//byte[] result = new byte[0];
			int bufSize = 1024;
			int totaLen = (int)stream.Length;
			byte[] data = new byte[totaLen + bufSize];
			int idx = 0;
			int readCount = 0;

			do {

				readCount = stream.Read(data, idx, bufSize);
				idx += readCount;

				//result += Encoding.UTF8.GetString(data, 0, readCount);
			} while (readCount >= bufSize);

			//result = Encoding.UTF8.GetString(data, 0, totaLen);

			len = totaLen;
			return data;
		}

		public void clear() {
			try {
				tcpClient?.clear();
				tcpClient = null;
				//client?.Close();
				//client = null;
				//server?.Stop();
				//server = null;

				thDeal?.Abort();
				thDeal = null;

				thCtl?.Abort();
				thCtl = null;

				lstData = new List<byte[]>();
				//dataStream = null;
			} catch (Exception) {

			}
		}

	}
}
