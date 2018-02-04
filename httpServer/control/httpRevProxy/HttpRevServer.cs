using com.superscene.util;
using httpServer.assembly.page;
using httpServer.entity;
using httpServer.module;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace httpServer.control.httpRevProxy {
	/// <summary>
	/// http反向代理服务端
	/// </summary>
	class HttpRevServer : IServerCtl {
		HttpRevModel md = null;

		Thread thCtl = null;
		Thread thListen = null;
		//TcpListener server = null;
		//TcpClient client = null;
		TcpCtl tcpServer = null;
		//NetworkStream dataStream = null;

		Thread thTcpSend = null;
		List<HttpPackModel> lstTcpData = new List<HttpPackModel>();
		object syncTcpData = new object();

		Thread thHttpDeal = null;
		List<byte[]> lstData = new List<byte[]>();
		object syncData = new object();

		HttpListener httpListener = null;
		//HttpListenerContext httpCnt = null;

		Int64 httpCtxIdx = 0;
		Dictionary<Int64, HttpListenerContext> mapHttpCtx = new Dictionary<long, HttpListenerContext>();
		//List<HttpListenerContext> lstHttpCxt = new List<HttpListenerContext>();
		object syncHttp = new object();

		public object DebughttpCnt { get; private set; }

		//TimeTest tm = new TimeTest();

		public HttpRevServer() { }
		public HttpRevServer(ServerModule _md) { setModel(_md); }


		public void setModel(ServerModule _md) {
			md = (HttpRevModel)_md;
		}

		public void restartServer() {
			try {
				clear();

				//thTcpSend = new Thread(tcpSendProc);
				//thTcpSend.Start();

				//thHttpDeal = new Thread(httpProc);
				//thHttpDeal.Start();

				//tcp ctl
				thCtl = new Thread(cltProc);
				thCtl.Start();

				//http server
				initHttpServer();
			} catch (Exception) {

			}
		}

		private void initHttpServer() {
			string port = md.httpPort;
			if (port == "") {
				port = "80";
			}
			string url = "http://" + md.httpIp + ":" + port + "/";
			httpListener = new HttpListener();

			httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
			httpListener.Prefixes.Add(url);
			httpListener.Start();
			httpListener.BeginGetContext(new AsyncCallback(GetContextCallBack), this);
		}

		static void GetContextCallBack(IAsyncResult ar) {
			try {
				HttpRevServer ctl = ar.AsyncState as HttpRevServer;
				//HttpListener sSocket = ar.AsyncState as HttpListener;
				HttpListener sSocket = ctl.httpListener;
				HttpListenerContext context = sSocket.EndGetContext(ar);

				//sSocket.BeginGetContext(new AsyncCallback(GetContextCallBack), sSocket);
				sSocket.BeginGetContext(new AsyncCallback(GetContextCallBack), ctl);

				ctl.responseFile(context);

			} catch (Exception) { }
		}

		private void responseFile(HttpListenerContext httpListenerContext) {
			try {
				string url = System.Web.HttpUtility.UrlDecode(httpListenerContext.Request.Url.AbsolutePath);
				//httpListenerContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
				//httpListenerContext.Response.Close();
				//Debug.WriteLine("cc:" + url);

				var httpCnt = httpListenerContext;

				//byte[] buffer = Encoding.UTF8.GetBytes(url);
				HttpPackModel mdHttp = new HttpPackModel();
				mdHttp.url = url;
				//Debug.WriteLine("ccc:" + url + "," + httpCnt.Response.Headers.Count);
				foreach (string key in httpCnt.Response.Headers.Keys) {
					mdHttp.mapHead[key] = httpCnt.Response.Headers[key];
				}

				Int64 idx = 0;
				lock (syncHttp) {
					idx = ++httpCtxIdx;
					mapHttpCtx[idx] = httpCnt;
				}
				mdHttp.httpIdx = idx;

				byte[] buffer = mdHttp.pack();
				//lstHttpCxt.Add(httpCnt);
				//lock (syncHttp) {
				//	//var dataStream = client.GetStream();
				//	//if (!dataStream.CanWrite) {
				//	//	Debug.WriteLine(dataStream.CanWrite);
				//	//}
				//	//Thread.Sleep(50);
				//	dataStream?.Write(buffer, 0, buffer.Length);
				//}
				//if (dataStream != null) {
				//	dataStream.Write(buffer, 0, buffer.Length);
				//}

				//lock (syncHttp) {
				//	//tcpServer?.send(buffer);
				//	tcpServer?.sendAsync(buffer);
				//}
				//if (url == "/lib/bootstrap/css/bootstrap-theme.min.css") {
				//	tm.start();
				//}
				Task.Run(() => {
					tcpServer?.sendAsync(buffer);
				});
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		private void tcpSendProc() {

		}

		//private byte[] packHttpData(HttpListenerContext httpCnt) {
		//	List<int> lstSize = new List<int>();
		//	//string strRst = "";

		//	int totalCount = 0;

		//	string url = System.Web.HttpUtility.UrlDecode(httpCnt.Request.Url.AbsolutePath);
		//	byte[] bUrl = Encoding.UTF8.GetBytes(url);
		//	totalCount += bUrl.Length;

		//	//lstSize.Add(bUrl.Length);
		//	//for (var i = 0; i < httpCnt.Request.Headers.Count; ++i) {
		//	string headLen = httpCnt.Request.Headers.Count.ToString();
		//	List<byte[]> lstHead = new List<byte[]>();
		//	foreach (string key in httpCnt.Request.Headers.Keys) {
		//		byte[] bKey = Encoding.UTF8.GetBytes(key);
		//		byte[] bVal = Encoding.UTF8.GetBytes(httpCnt.Request.Headers[key]);
		//		lstHead.Add(bKey);
		//		lstHead.Add(bVal);
		//		totalCount += bKey.Length;
		//		totalCount += bVal.Length;
		//		//Debug.WriteLine("" + "," + key + "," + httpCnt.Request.Headers[key]);
		//	}

		//	totalCount += 2 + 2 + 2 * lstHead.Count;

		//	byte[] rst = new byte[totalCount];
		//	var idx = 0;
		//	//url len
		//	byte[] bUrlLen = BitConverter.GetBytes((short)bUrl.Length);
		//	Array.Copy(bUrlLen, 0, rst, idx, 2);
		//	idx += 2;
		//	//
		//	byte[] bHeadCountLen = BitConverter.GetBytes((short)lstHead.Count);
		//	Array.Copy(bHeadCountLen, 0, rst, idx, 2);
		//	idx += 2;
		//	//
		//	for (int i = 0; i < lstHead.Count; ++i) {
		//		byte[] bCount = BitConverter.GetBytes((short)lstHead[i].Length);
		//		Array.Copy(bCount, 0, rst, idx, 2);
		//		idx += 2;
		//	}
		//	//
		//	Array.Copy(bUrl, 0, rst, idx, bUrl.Length);
		//	idx += bUrl.Length;
		//	//
		//	for (int i = 0; i < lstHead.Count; ++i) {
		//		Array.Copy(lstHead[i], 0, rst, idx, lstHead[i].Length);
		//		idx += lstHead[i].Length;
		//	}

		//	return rst;
		//}

		public void listenProc() {
			try {
				do {
					try {
						tcpServer.wait();
					} catch(Exception) {
						Thread.Sleep(20);
					}
				} while(true);
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void cltProc() {
			try {
				//IPAddress ip = IPAddress.Parse(md.ctlIp);
				//int port = Int32.Parse(md.ctlPort);
				//server = new TcpListener(ip, port);
				//server.Start();                             //服务端启动侦听
				//client = server.AcceptTcpClient();
				//client.ReceiveBufferSize = 10 * 1024 * 1024;
				//client.SendBufferSize = 10 * 1024 * 1024;
				tcpServer = new TcpCtl(md.ctlIp, md.ctlPort);
				tcpServer.listen();

				thListen = new Thread(listenProc);
				thListen.Start();

				tcpServer.listenData((data) => {
					//lock(syncData) {
					//	lstData.Add(data);
					//}
					Task.Run(() => {
						httpDealOne(data);
					});
				});

				//do {
				//	try {
				//		//if (client == null || !client.Connected) {
				//		dataStream = client.GetStream();
				//		//}

				//		byte[] data = ComServerCtl.readStream(dataStream);
				//		//Debug.WriteLine("aaaa:" + data.Length);
				//		//string strData = Encoding.Default.GetString(data);
				//		//Entity.getInstance().mainWin.log(strData);

				//		//var httpIdx = BitConverter.ToInt64(data, 0);
				//		//Debug.WriteLine("aaa:" + httpIdx);

				//		lock(syncData) {
				//			lstData.Add(data);
				//		}
				//		continue;

				//		//try {
				//		//	HttpPackModel httpModel = new HttpPackModel();
				//		//	httpModel.unPack(data);
				//		//	//Debug.WriteLine("aa:" + httpModel.url + "," + httpModel.httpIdx);
				//		//	//foreach (string key in httpModel.mapHead.Keys) {
				//		//	//	Debug.WriteLine(key + "," + httpModel.mapHead[key]);
				//		//	//}

				//		//	HttpListenerContext httpCnt = null;
				//		//	lock (syncHttp) {
				//		//		if (mapHttpCtx.ContainsKey(httpModel.httpIdx)) {
				//		//			httpCnt = mapHttpCtx[httpModel.httpIdx];
				//		//			mapHttpCtx.Remove(httpModel.httpIdx);
				//		//		}
				//		//	}

				//		//	if (httpCnt == null) {
				//		//		continue;
				//		//	}

				//		//	//if (mapHttpCtx.ContainsKey(httpModel.httpIdx)) {
				//		//	//	var httpCnt = mapHttpCtx[httpModel.httpIdx];
				//		//	//	//lstHttpCxt.RemoveAt(0);
				//		//	//	mapHttpCtx.Remove(httpModel.httpIdx);

				//		//	var accName = "Access-Control-Allow-Origin";
				//		//	if (httpModel.mapHead.ContainsKey(accName)) {
				//		//		httpCnt.Response.Headers.Add(accName, httpModel.mapHead[accName]);
				//		//	}
				//		//	if (httpModel.mapHead.ContainsKey("Content-Type")) {
				//		//		httpCnt.Response.Headers.Add("Content-Type", httpModel.mapHead["Content-Type"]);
				//		//	}
				//		//	httpCnt.Response.StatusCode = Int32.Parse(httpModel.mapHead["Status"]);

				//		//	httpCnt.Response.Close(httpModel.data, true);
				//		//	//}

				//		//} catch (Exception ex) {
				//		//	Debug.WriteLine(ex.ToString());
				//		//}

				//		//string msg = "to client";
				//		//byte[] buffer = Encoding.Default.GetBytes(msg);
				//		//dataStream.Write(buffer, 0, buffer.Length);
				//		//Thread.Sleep(1000);
				//		//dataStream.Write(buffer, 0, buffer.Length);
				//	} catch (Exception ex) {
				//		Debug.WriteLine(ex.ToString());
				//	}
				//} while (true);
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		private void httpDealOne(byte[] data) {
			HttpPackModel httpModel = new HttpPackModel();
			httpModel.unPack(data);
			//Debug.WriteLine("aa:" + httpModel.url + "," + data.Length);
			//foreach (string key in httpModel.mapHead.Keys) {
			//	Debug.WriteLine(key + "," + httpModel.mapHead[key]);
			//}

			HttpListenerContext httpCnt = null;
			lock (syncHttp) {
				if (mapHttpCtx.ContainsKey(httpModel.httpIdx)) {
					httpCnt = mapHttpCtx[httpModel.httpIdx];
					mapHttpCtx.Remove(httpModel.httpIdx);
				}
			}

			if (httpCnt == null) {
				return;
			}

			//Debug.WriteLine("aa:" + httpModel.url + "," + httpModel.httpIdx + "," + data.Length);
			//if (mapHttpCtx.ContainsKey(httpModel.httpIdx)) {
			//	var httpCnt = mapHttpCtx[httpModel.httpIdx];
			//	//lstHttpCxt.RemoveAt(0);
			//	mapHttpCtx.Remove(httpModel.httpIdx);

			var accName = "Access-Control-Allow-Origin";
			if (httpModel.mapHead.ContainsKey(accName)) {
				httpCnt.Response.Headers.Add(accName, httpModel.mapHead[accName]);
			}
			if (httpModel.mapHead.ContainsKey("Content-Type")) {
				httpCnt.Response.Headers.Add("Content-Type", httpModel.mapHead["Content-Type"]);
			}
			httpCnt.Response.StatusCode = Int32.Parse(httpModel.mapHead["Status"]);

			httpCnt.Response.Close(httpModel.data, true);
		}

		private void httpProc() {
			try {
				do {
					List<byte[]> lstDataTmp = null;

					lock (syncData) {
						lstDataTmp = lstData;
						if (lstData.Count > 0) {
							lstData = new List<byte[]>();
						}
					}

					if (lstDataTmp.Count <= 0 || tcpServer == null) {
						Thread.Sleep(100);
						continue;
					}

					try {

						for (int i = 0; i < lstDataTmp.Count; ++i) {
							byte[] data = lstDataTmp[i];

							HttpPackModel httpModel = new HttpPackModel();
							httpModel.unPack(data);
							//Debug.WriteLine("aa:" + httpModel.url + "," + data.Length);
							//foreach (string key in httpModel.mapHead.Keys) {
							//	Debug.WriteLine(key + "," + httpModel.mapHead[key]);
							//}

							HttpListenerContext httpCnt = null;
							lock (syncHttp) {
								if (mapHttpCtx.ContainsKey(httpModel.httpIdx)) {
									httpCnt = mapHttpCtx[httpModel.httpIdx];
									mapHttpCtx.Remove(httpModel.httpIdx);
								}
							}

							if (httpCnt == null) {
								continue;
							}

							//Debug.WriteLine("aa:" + httpModel.url + "," + httpModel.httpIdx + "," + data.Length);
							//if (mapHttpCtx.ContainsKey(httpModel.httpIdx)) {
							//	var httpCnt = mapHttpCtx[httpModel.httpIdx];
							//	//lstHttpCxt.RemoveAt(0);
							//	mapHttpCtx.Remove(httpModel.httpIdx);

							var accName = "Access-Control-Allow-Origin";
							if (httpModel.mapHead.ContainsKey(accName)) {
								httpCnt.Response.Headers.Add(accName, httpModel.mapHead[accName]);
							}
							if (httpModel.mapHead.ContainsKey("Content-Type")) {
								httpCnt.Response.Headers.Add("Content-Type", httpModel.mapHead["Content-Type"]);
							}
							httpCnt.Response.StatusCode = Int32.Parse(httpModel.mapHead["Status"]);

							httpCnt.Response.Close(httpModel.data, true);
							//if (httpModel.url == "/lib/bootstrap/css/bootstrap-theme.min.css") {
							//	Debug.WriteLine(tm.end());
							//}
							//}
						}
					} catch (Exception ex) {
						Debug.WriteLine(ex.ToString());
					}
				} while (true);
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void clear() {
			try {
				tcpServer?.clear();
				tcpServer = null;
				//client?.Close();
				//client = null;
				//server?.Stop();
				//server = null;
				//dataStream = null;

				thListen?.Abort();
				thListen = null;

				thTcpSend?.Abort();
				thTcpSend = null;
				lstTcpData = new List<HttpPackModel>();

				thCtl?.Abort();
				thCtl = null;

				thHttpDeal?.Abort();
				thHttpDeal = null;
				lstData = new List<byte[]>();

				httpListener?.Abort();
				httpListener = null;
				//httpCnt = null;
				//lstHttpCxt = new List<HttpListenerContext>();
				mapHttpCtx = new Dictionary<long, HttpListenerContext>();
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}
	}
}
