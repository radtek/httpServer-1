using httpServer.assembly.page;
using httpServer.entity;
using httpServer.module;
using System;
using System.Collections.Generic;
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
		TcpListener server = null;
		TcpClient client = null;
		NetworkStream dataStream = null;
		
		HttpListener httpListener = null;

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
			string url = System.Web.HttpUtility.UrlDecode(httpListenerContext.Request.Url.AbsolutePath);
			httpListenerContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
			httpListenerContext.Response.Close();
		}

		public void cltProc() {
			try {
				IPAddress ip = IPAddress.Parse(md.ctlIp);
				int port = Int32.Parse(md.ctlPort);
				server = new TcpListener(ip, port);
				server.Start();                             //服务端启动侦听
				do {
					try {
						//if (client == null || !client.Connected) {
							client = server.AcceptTcpClient();
							dataStream = client.GetStream();
						//}

						//byte[] data = ComServerCtl.readStream(dataStream);
						//string strData = Encoding.Default.GetString(data);

						//Entity.getInstance().mainWin.log(strData);

						string msg = "to client";
						byte[] buffer = Encoding.Default.GetBytes(msg);
						dataStream.Write(buffer, 0, buffer.Length);
						Thread.Sleep(1000);
						dataStream.Write(buffer, 0, buffer.Length);
					} catch (Exception) {

					}
				} while (true);
			} catch (Exception) {

			}
		}

		public void clear() {
			try {
				client?.Close();
				client = null;
				server?.Stop();
				server = null;

				thCtl?.Abort();
				thCtl = null;
				
				httpListener?.Abort();
				httpListener = null;
			} catch (Exception) {

			}
		}
	}
}
