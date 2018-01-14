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
	/// http反向代理客户端
	/// </summary>
	class HttpRevClient : IServerCtl {
		HttpRevModel md = null;

		Thread thCtl = null;
		TcpListener server = null;
		TcpClient client = null;

		public HttpRevClient() { }
		public HttpRevClient(ServerModule _md) { setModel(_md); }

		public void setModel(ServerModule _md) {
			md = (HttpRevModel)_md;
		}

		public void restartServer() {
			try {
				clear();

				thCtl = new Thread(cltProc);
				thCtl.Start();
				
			} catch (Exception) {

			}
		}

		public void cltProc() {
			try {
				IPAddress ip = IPAddress.Parse(md.serverCtlIp);
				int port = Int32.Parse(md.serverCtlPort);

				client = new TcpClient();
				client.Connect(ip, port);
				NetworkStream dataStream = client.GetStream();

				do {
					byte[] data = ComServerCtl.readStream(dataStream);
					if (data.Length == 0) {
						Thread.Sleep(100);
						continue;
					}

					string strData = Encoding.Default.GetString(data);
					Entity.getInstance().mainWin.log(strData);
				} while (true);

				//NetworkStream dataStream = client.GetStream();
				//string msg = "服务端亲启！";
				//byte[] buffer = Encoding.Default.GetBytes(msg);
				//dataStream.Write(buffer, 0, buffer.Length);
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
			} catch (Exception) {

			}
		}

	}
}
