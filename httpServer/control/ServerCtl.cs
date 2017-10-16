using httpServer.module;
using httpServer.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace httpServer.control {
	public class ServerCtl {
		public class IpAddr {
			public string ip = "";
			public string port = "";
		};
		public enum ServerStatus {
			none, http, transmit
		}

		static Dictionary<string, string> mapSuffix = new Dictionary<string, string>();

		//public string ip;
		//public string port;

		//public string serverPath = "";

		//public bool isProxy = false;
		//public string proxyUrl = "";

		//public bool isTransmit = false;
		//public string transmitUrl = "";

		public ServerModule md = null;

		string lastServer = "";
		string serverUrl = "";

		ServerStatus lastStatus = ServerStatus.none;
		private TimeSpan _timeout = new TimeSpan(0, 0, 0, 0, 0);
		bool stopServer = false;
		Thread thServer = null;
		HttpListener httpListener = null;

		static ServerCtl(){
			mapSuffix[".css"] = "text/css";
			mapSuffix[".js"] = "text/javascript";
			mapSuffix[".html"] = "text/html";
			mapSuffix[".png"] = "image/png";
			mapSuffix[".xml"] = "application/xml";
			mapSuffix[".jpg"] = "image/jpeg";
		}

		public ServerCtl() {
			_timeout = TimeSpan.FromMilliseconds(3000);
		}

		public void restartServer() {
			try {
				clear();
				if(md.isTransmit) {
					lastStatus = ServerStatus.transmit;

					IpAddr addr = parseServer(md.transmitUrl);

					//string ip = cbxIp.Text;
					//string port = txtPort.Text.Trim();
					lastServer = md.ip + ":" + md.port;
					CommonUtil.runExeNoWin("netsh", "interface portproxy add v4tov4 listenaddress=" + md.ip + " listenport=" + md.port + " connectaddress=" + addr.ip + "  connectport=" + addr.port);
					return;
				}
				lastStatus = ServerStatus.http;
				//clear();
				stopServer = false;
				updateCopyUrlPos();
			} catch(Exception) {

			}
		}

		private IpAddr parseServer(string url) {
			IpAddr addr = new IpAddr();
			string data = url.Trim();
			string[] arr = data.Split(':');

			if(arr.Length == 2) {
				addr.ip = arr[0];
				addr.port = arr[1];
				return addr;
			}

			addr.ip = data;
			addr.port = "80";
			return addr;
		}

		private void updateCopyUrlPos() {
			//string ip = cbxIp.Text;
			//string port = txtPort.Text.Trim();
			string port = md.port;
			if(port == "") {
				port = "80";
			}

			string url = "http://" + md.ip + ":" + port;
			//lblUrl.Content = url;
			serverUrl = url;
			url += "/";

			httpListener = new HttpListener();

			httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
			httpListener.Prefixes.Add(url);
			httpListener.Start();
			//httpListener.BeginGetContext(new AsyncCallback(GetContextCallBack), httpListener);
			thServer = new Thread(serverProc);
			thServer.Start();
		}

		//static void GetContextCallBack(IAsyncResult ar) {
		//	try {
		//		HttpListener sSocket = ar.AsyncState as HttpListener;
		//		HttpListenerContext context = sSocket.EndGetContext(ar);

		//		sSocket.BeginGetContext(new AsyncCallback(GetContextCallBack), sSocket);

		//		MainWindow.ins.responseFile(context);

		//	} catch { }
		//}

		private void serverProc() {
			try {
				while(!stopServer) {
					HttpListenerContext httpListenerContext = httpListener.GetContext();
					responseFile(httpListenerContext);
				}
			} catch(Exception) {

			}
		}

		private void responseFile(HttpListenerContext httpListenerContext) {
			if(md.isProxy) {
				resoonseProxy(httpListenerContext);
				return;
			}

			string url = System.Web.HttpUtility.UrlDecode(httpListenerContext.Request.Url.AbsolutePath);
			//string url = httpListenerContext.Request.Url.AbsolutePath;
			bool isIndex = false;
			if(url != "" && url[url.Length - 1] == '/') {
				isIndex = true;
				url += "index.html";
			}

			string path = md.path + "/" + url;
			//Console.WriteLine(path);

			if(!isIndex && !File.Exists(path)) {
				string newPath = md.path + "/" + url + "/index.html";
				if(File.Exists(newPath)) {
					httpListenerContext.Response.StatusCode = 302;
					httpListenerContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
					//httpListenerContext.Response.Headers.Add("Location", "http://127.0.0.1:8091/demo/basic/");
					httpListenerContext.Response.Headers.Add("Location", url + "/");
					httpListenerContext.Response.OutputStream.Close();
					return;
				}
			}

			//string result = "";
			byte[] buffer = new byte[0];
			if(!File.Exists(path)) {
				httpListenerContext.Response.StatusCode = 404;
			} else {
				httpListenerContext.Response.StatusCode = 200;

				string suffix = System.IO.Path.GetExtension(path).ToLower();
				if(mapSuffix.ContainsKey(suffix)) {
					httpListenerContext.Response.Headers.Add("Content-Type", mapSuffix[suffix]);
				}

				using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
					buffer = new byte[fs.Length];
					fs.Read(buffer, 0, (int)fs.Length); //将文件读到缓存区
				};
			}

			httpListenerContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
			//httpListenerContext.Response.Headers.Add("Content-Type", "text/html;charset=UTF-8");
			var output = httpListenerContext.Response.OutputStream;
			output.Write(buffer, 0, buffer.Length);
			output.Close();

			//using(StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream)) {
			//	writer.WriteLine("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/><title>测试服务器</title></head><body>");
			//	writer.WriteLine("<div style=\"height:20px;color:blue;text-align:center;\"><p> hello</p></div>");
			//	writer.WriteLine("<ul>");
			//	writer.WriteLine("</ul>");
			//	writer.WriteLine("</body></html>");
			//}
		}

		/// <summary>
		/// 反向代理
		/// </summary>
		/// <param name="httpListenerContext"></param>
		private void resoonseProxy(HttpListenerContext httpListenerContext) {
			string url = System.Web.HttpUtility.UrlDecode(httpListenerContext.Request.Url.AbsolutePath);
			//string url = httpListenerContext.Request.Url.AbsolutePath;
			string realUrl = md.proxyUrl + url;

			//string result = httpCtl.httpGet(realUrl);
			//byte[] buffer = Encoding.UTF8.GetBytes(result);
			//string method = httpListenerContext.Request.HttpMethod;
			//Debug.WriteLine(realUrl + "," + method);

			byte[] result = new byte[0];
			int dataLen = 0;
			try {
				//HttpContent stringContent = new StringContent(data);
				var handler = new HttpClientHandler() {
					AllowAutoRedirect = false
				};

				using(var client = new HttpClient(handler)) {
					//using(var formData = new MultipartFormDataContent()) {
					//	formData.Add(stringContent, name, name);
					if(_timeout.TotalMilliseconds != 0) {
						client.Timeout = _timeout;
					}

					//client.BaseAddress = new Uri("http://" + httpListenerContext.Request.UserHostAddress);
					//Debug.WriteLine(httpListenerContext.Request.UserHostAddress);
					//client.BaseAddress
					//if(method != "GET") {
					//	var que = httpListenerContext.Request.QueryString;
					//	que = new System.Collections.Specialized.NameValueCollection
					//	//for(int i = 0; i < que.Count; ++i) {
					//	HttpContent stringContent = new StringContent(httpListenerContext.Request.QueryString);
					//}
					//var getAsync = client.GetAsync(realUrl);
					//try {
					//	var responsea = client.GetAsync(realUrl).Result;
					//} catch(System.AggregateException ex) {
					//	Debug.WriteLine(ex.ToString());
					//	Debug.WriteLine(ex.ToString());
					//}
					//Debug.WriteLine( client.DefaultRequestHeaders.UserAgent);
					//client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.79 Safari/537.36");
					//client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
					//client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
					//client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8");
					////client.DefaultRequestHeaders.Add("Connection", "keep-alive");
					//client.DefaultRequestHeaders.Add("Host", "127.0.0.1:32767");
					//client.DefaultRequestHeaders.Add("Referer", "http://127.0.0.1:32767/14.58.58/index.html");
					//client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
					//client.DefaultRequestHeaders.Host = "127.0.0.1:32767";
					//Debug.WriteLine(client.DefaultRequestHeaders.Host);

					//Uri ur = new Uri(realUrl);
					//Debug.WriteLine(ur.Host);
					//var requestMessage = new HttpRequestMessage {
					//	Version = HttpVersion.Version11,
					//	RequestUri = new Uri(realUrl),
					//	Method = new HttpMethod("GET"),
					//};
					//requestMessage.Headers.Host = "127.0.0.1";

					//var response = client.SendAsync(requestMessage).Result;
					var response = client.GetAsync(realUrl).Result;
					bool isResponseOk = response.IsSuccessStatusCode;
					HttpStatusCode code = response.StatusCode;
					if(!isResponseOk) {
						//result = "";
					} else {
						Stream sResult = response.Content.ReadAsStreamAsync().Result;
						result = readStream(sResult, out dataLen);
					}

					//httpListenerContext.Response.Headers.Add("Content-Type", response.Headers.GetValues("Content-Type"));
					if(response.Content.Headers.ContentType != null) {
						httpListenerContext.Response.Headers.Add("Content-Type", response.Content.Headers.ContentType.ToString());
					}
					if(response.Headers.Location != null) {
						string location = response.Headers.Location.ToString();
						location = location.Replace(md.proxyUrl, serverUrl);
						httpListenerContext.Response.Headers.Add("Location", location);
					}

					//response.Headers.Location;
					int statusCode = (int)code;
					//int.TryParse(code.ToString(), out statusCode);
					//Debug.WriteLine(realUrl + "," + statusCode);
					//if(statusCode == 0) {
					//	statusCode = 200;
					//}
					httpListenerContext.Response.StatusCode = statusCode;
					//}
				}
			} catch(Exception ex) {
				//httpListenerContext.Response.StatusCode = 302;
				Debug.WriteLine(realUrl + "," + ex.ToString());
			}

			httpListenerContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
			var output = httpListenerContext.Response.OutputStream;
			output.Write(result, 0, dataLen);
			output.Close();
		}

		private byte[] readStream(Stream stream, out int len) {
			//string result = "";
			int bufSize = 1024;
			int totaLen = (int)stream.Length;
			byte[] data = new byte[totaLen + bufSize];
			int idx = 0;
			int readCount = 0;

			do {

				readCount = stream.Read(data, idx, bufSize);
				idx += readCount;

				//result += Encoding.UTF8.GetString(data, 0, readCount);
			} while(readCount >= bufSize);

			//result = Encoding.UTF8.GetString(data, 0, totaLen);

			len = totaLen;
			return data;
		}
		
		public void clear() {
			stopServer = true;
			httpListener?.Abort();
			httpListener = null;
			thServer?.Abort();
			thServer = null;

			if(lastStatus == ServerStatus.transmit) {
				//IpAddr addr = parseServer(lastStatus);
				IpAddr addr = parseServer(lastServer);
				CommonUtil.runExeNoWin("netsh", "interface portproxy delete v4tov4 listenaddress=" + addr.ip + " listenport=" + addr.port);
			}
			lastStatus = ServerStatus.none;
		}

	}
}
