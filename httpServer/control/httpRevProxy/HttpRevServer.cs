using csharpHelp.util;
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

		HttpListener httpListener = null;
		//HttpListenerContext httpCnt = null;

		Int64 httpCtxIdx = 0;
		Dictionary<Int64, HttpListenerContext> mapHttpCtx = new Dictionary<long, HttpListenerContext>();
		//List<HttpListenerContext> lstHttpCxt = new List<HttpListenerContext>();
		object syncHttp = new object();

		public HttpRevServer() { }
		public HttpRevServer(ServerModule _md) { setModel(_md); }
		
		public void setModel(ServerModule _md) {
			md = (HttpRevModel)_md;
		}

		public void restartServer() {
			try {
				clear();

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

				Task.Run(() => {
					tcpServer?.sendAsync(buffer);
				});
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

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
				tcpServer = new TcpCtl(md.ctlIp, md.ctlPort);
				tcpServer.listen();

				thListen = new Thread(listenProc);
				thListen.Start();

				tcpServer.listenData((data) => {
					Task.Run(() => {
						httpDealOne(data);
					});
				});
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		private void httpDealOne(byte[] data) {
			HttpPackModel httpModel = new HttpPackModel();
			httpModel.unPack(data);

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

		public void clear() {
			try {
				tcpServer?.clear();
				tcpServer = null;

				thListen?.Abort();
				thListen = null;

				thCtl?.Abort();
				thCtl = null;

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
