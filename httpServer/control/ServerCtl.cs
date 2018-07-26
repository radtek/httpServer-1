using csharpHelp.util;
using HttpMultipartParser;
using httpServer.assembly.page;
using httpServer.module;
using httpServer.util;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace httpServer.control {
	class ServerCtl {
		public class IpAddr {
			public string ip = "";
			public string port = "";
		};
		public enum ServerStatus {
			none, http, transmit
		}

		static Dictionary<string, string> mapSuffix = new Dictionary<string, string>();
		static Cache cache = new Cache();

		//public string ip;
		//public string port;

		//public string serverPath = "";

		//public bool isProxy = false;
		//public string proxyUrl = "";

		//public bool isTransmit = false;
		//public string transmitUrl = "";

		public HttpModel md = null;

		string lastServer = "";
		string serverUrl = "";

		ServerStatus lastStatus = ServerStatus.none;
		private TimeSpan _timeout = new TimeSpan(0, 0, 0, 0, 0);
		bool stopServer = false;
		Thread thServer = null;
		HttpListener httpListener = null;

		RequestCtl requestCtl = null;

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

			requestCtl = new RequestCtl();
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
			httpListener.BeginGetContext(new AsyncCallback(GetContextCallBack), this);
			//thServer = new Thread(serverProc);
			//thServer.Start();
		}

		static void GetContextCallBack(IAsyncResult ar) {
			try {
				ServerCtl ctl = ar.AsyncState as ServerCtl;
				//HttpListener sSocket = ar.AsyncState as HttpListener;
				HttpListener sSocket = ctl.httpListener;
				HttpListenerContext context = sSocket.EndGetContext(ar);

				//sSocket.BeginGetContext(new AsyncCallback(GetContextCallBack), sSocket);
				sSocket.BeginGetContext(new AsyncCallback(GetContextCallBack), ctl);

				ctl.responseFile(context);

			} catch(Exception) { }
		}

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
			foreach(string key in md.mapRewrite.Keys) {
				if(url.IndexOf(key) >= 0) {
					var arr = md.mapRewrite[key].Split(',');
					if(arr.Length < 2) {
						continue;
					}
					string rewriteUrl = arr[0].Trim();
					string rewriteParam = arr[1].Trim();
					string newUrl = rewriteUrl + url.Replace(key, rewriteParam);

					//foreach(string aaa in httpListenerContext.Request.Headers.Keys) {
					//	Debug.WriteLine(httpListenerContext.Request.Headers[aaa]);
					//}
					rewrite(newUrl, httpListenerContext);
					return;
				}
			}

			//Debug.WriteLine(url);
			//string url = httpListenerContext.Request.Url.AbsolutePath;
			bool isIndex = false;
			if(url != "" && url[url.Length - 1] == '/') {
				isIndex = true;
				url += "index.html";
			}

			string path = Path.GetFullPath(md.path + "/" + url);

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
				//自定义请求
				bool isDeal = parseCtl(url, httpListenerContext);
				if(isDeal) {
					return;
				} else {
					httpListenerContext.Response.StatusCode = 404;
				}
			} else {
				httpListenerContext.Response.StatusCode = 200;

				string suffix = Path.GetExtension(path).ToLower();
				if(mapSuffix.ContainsKey(suffix)) {
					httpListenerContext.Response.Headers.Add("Content-Type", mapSuffix[suffix]);
				}

				//缓存
				//buffer = cache.getFile(path);

				//直接读文件
				using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
					buffer = new byte[fs.Length];
					fs.Read(buffer, 0, (int)fs.Length);
				};

				//内存映射文件
				//try {
				//	FileInfo fileInfo = new FileInfo(path);

				//	IntPtr hFile = Kernel32.CreateFile(path, Kernel32.GENERIC_READ, Kernel32.FILE_SHARE_READ, IntPtr.Zero, Kernel32.OPEN_EXISTING, Kernel32.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
				//	if((int)hFile != Kernel32.INVALID_HANDLE_VALUE) {
				//		var hFileMap = Kernel32.CreateFileMapping(hFile, IntPtr.Zero, (uint)Kernel32.PAGE_READONLY, 0, (uint)fileInfo.Length, null);
				//		if(hFileMap != IntPtr.Zero) {
				//			IntPtr diskBuf = Kernel32.MapViewOfFile(hFileMap, Kernel32.FILE_MAP_READ, 0, 0, (uint)fileInfo.Length);
				//			if(diskBuf != IntPtr.Zero) {
				//				buffer = new byte[fileInfo.Length];
				//				Marshal.Copy(diskBuf, buffer, 0, (int)fileInfo.Length);
				//				Kernel32.UnmapViewOfFile(diskBuf);
				//			}
				//			Kernel32.CloseHandle(hFileMap);
				//		}
				//		Kernel32.CloseHandle(hFile);
				//	}
				//} catch(Exception ex) {
				//	Debug.WriteLine(ex.ToString());
				//}
			}

			httpListenerContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
			httpListenerContext.Response.Close(buffer, true);
		}

		/// <summary>重定向</summary>
		private void rewrite(string newUrl, HttpListenerContext httpListenerContext) {
			try {
				byte[] result = new byte[0];
				int dataLen = 0;

				var handler = new HttpClientHandler() {
					AllowAutoRedirect = false,
					UseCookies = false
				};

				using(var client = new HttpClient(handler)) {
					if(_timeout.TotalMilliseconds != 0) {
						client.Timeout = _timeout;
					}

					int inputLen = 0;
					byte[] intputData = readStream(httpListenerContext.Request.InputStream, out inputLen);
					//using(var reader = new BinaryReader(httpListenerContext.Request.InputStream, httpListenerContext.Request.ContentEncoding)) {
					//	Debug.WriteLine("aa:" + reader.ReadToEnd());
					//}

					foreach(string key in httpListenerContext.Request.Headers.Keys) {
						try {
							string val = httpListenerContext.Request.Headers[key];

							if(key.ToLower() == "host") {
								val = (new Regex("(?:.*?://)(.*?)(?:/)")).Match(newUrl).Groups[1].Value;
							} else if(key.ToLower() == "origin") {
								val = (new Regex("(.*?://.*?)(?:/)")).Match(newUrl).Groups[1].Value;
							} else if(key.ToLower() == "referer") {
								//var host = (new Regex(".*?://.*?/")).Match(newUrl).Value;
								val = newUrl;
							}
							//Debug.WriteLine($"request: {key},{val}");
							client.DefaultRequestHeaders.Add(key, val);
						} catch(Exception) { }
					}

					//var response = client.SendAsync(requestMessage).Result;
					HttpResponseMessage response = null;
					if(httpListenerContext.Request.HttpMethod.ToLower() == "post") {
						//Debug.WriteLine($"input:{Encoding.UTF8.GetString(intputData, 0, inputLen)},bb");
						var postData = new ByteArrayContent(intputData, 0, inputLen);
						response = client.PostAsync(newUrl, postData).Result;
					} else {
						response = client.GetAsync(newUrl).Result;
					}
					bool isResponseOk = response.IsSuccessStatusCode;
					HttpStatusCode code = response.StatusCode;
					if(!isResponseOk) {
						//result = "";
					} else {
						Stream sResult = response.Content.ReadAsStreamAsync().Result;
						result = readStream(sResult, out dataLen);
						//Debug.WriteLine("rst:" + Encoding.UTF8.GetString(result, 0, dataLen) + ",bb");
					}

					//httpListenerContext.Response.Headers.Add("Content-Type", response.Headers.GetValues("Content-Type"));
					//if(response.Content.Headers.ContentType != null) {
					//	httpListenerContext.Response.Headers.Add("Content-Type", response.Content.Headers.ContentType.ToString());
					//}
					//if(response.Headers.Location != null) {
					//	string location = response.Headers.Location.ToString();
					//	location = location.Replace(md.proxyUrl, serverUrl);
					//	httpListenerContext.Response.Headers.Add("Location", location);
					//}

					//response.Headers.Location;
					int statusCode = (int)code;
					httpListenerContext.Response.StatusCode = statusCode;
					
					var lst = response.Headers.ToList();
					for(int i = 0; i < lst.Count; ++i) {
						var vals = (lst[i].Value as string[]);
						if(vals != null && vals.Length > 0) {
							//Debug.WriteLine($"response:{lst[i].Key},{vals[0]}");
							try {
								httpListenerContext.Response.Headers.Add(lst[i].Key, vals[0]);
							} catch(Exception) { }
						}
						//httpListenerContext.Response.Headers.Add(lst[i].Key, lst[i].Value.ToString());
					}
					var lst2 = response.Content.Headers.ToList();
					for(int i = 0; i < lst2.Count; ++i) {
						var vals = (lst2[i].Value as string[]);
						if(vals != null && vals.Length > 0) {
							//Debug.WriteLine($"response2:{lst2[i].Key},{vals[0]}");
							try {
								httpListenerContext.Response.Headers.Add(lst2[i].Key, vals[0]);
							} catch(Exception) { }
						}
					}
				}

				//httpListenerContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
				var output = httpListenerContext.Response.OutputStream;
				output.Write(result, 0, dataLen);
				output.Close();
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());

				try {
					httpListenerContext.Response.StatusCode = 404;
					var output = httpListenerContext.Response.OutputStream;
					output.Write(new byte[0], 0, 0);
				} catch(Exception) { }
			}
		}

		private bool parseCtl(string url, HttpListenerContext ctx) {
			bool isDeal = false;

			if(ctx.Request.HttpMethod == "OPTIONS") {
				ctx.Response.StatusCode = 200;
				ctx.Response.Headers.Add("Access-Control-Allow-Credentials", "false");
				ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
				ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
				ctx.Response.Headers.Add("Access-Control-Allow-Methods", "*");
				var output = ctx.Response.OutputStream;
				output.Close();
				return true;
			}

			try {
				isDeal = requestCtl.parse(url, ctx);

				var parser = new MultipartFormDataParser(ctx.Request.InputStream);

				foreach(var parameter in parser.Parameters) {
					if(Regex.IsMatch(parameter.Data, "^data:.{0,30}?;base64,")) {
						//base64
						int idx = parameter.Data.IndexOf(',') + 1;
						byte[] data = Convert.FromBase64String(parameter.Data.Substring(idx));
						string strData = Encoding.UTF8.GetString(data, 0, data.Length);
						//Debug.WriteLine(url + "," + data.Length + "\r\n" + strData + "");
					} else {
						//string
						//Debug.WriteLine(parameter.Name + "," + parameter.Data);
					}
				}
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}

			return isDeal;
		}

		/// <summary>
		/// 反向代理
		/// </summary>
		/// <param name="httpListenerContext"></param>
		private void resoonseProxy(HttpListenerContext httpListenerContext) {
			string url = System.Web.HttpUtility.UrlDecode(httpListenerContext.Request.Url.AbsolutePath);
			//string url = httpListenerContext.Request.Url.AbsolutePath;
			string realUrl = md.proxyUrl + url;

			byte[] result = new byte[0];
			int dataLen = 0;
			try {
				//HttpContent stringContent = new StringContent(data);
				var handler = new HttpClientHandler() {
					AllowAutoRedirect = false
				};

				using(var client = new HttpClient(handler)) {
					if(_timeout.TotalMilliseconds != 0) {
						client.Timeout = _timeout;
					}

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
					httpListenerContext.Response.StatusCode = statusCode;
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
			try {
				const int bufferSize = 4096;
				using(var ms = new MemoryStream()) {
					byte[] buffer = new byte[bufferSize];
					int count;
					while((count = stream.Read(buffer, 0, buffer.Length)) != 0) {
						ms.Write(buffer, 0, count);
					}

					byte[] rst = ms.ToArray();
					len = rst.Length;
					return rst;
				}

				//if(!stream.CanRead || !stream.CanSeek) {
				//	len = 0;
				//	return new byte[0];
				//}
				////string result = "";
				//int bufSize = 1024;
				//int totaLen = 0;
				//if(stream.CanSeek) {
				//	totaLen = (int)stream.Length;
				//}
				//byte[] data = new byte[totaLen + bufSize];
				//int idx = 0;
				//int readCount = 0;

				//do {
				//	readCount = stream.Read(data, idx, bufSize);
				//	idx += readCount;

				//	//result += Encoding.UTF8.GetString(data, 0, readCount);
				//} while(readCount >= bufSize);

				////result = Encoding.UTF8.GetString(data, 0, totaLen);

				//len = totaLen;
				//return data;
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
				len = 0;
				return new byte[0];
			}
		}

		private byte[] readStream(Stream stream) {
			try {
				const int bufferSize = 4096;
				using(var ms = new MemoryStream()) {
					byte[] buffer = new byte[bufferSize];
					int count;
					while((count = stream.Read(buffer, 0, buffer.Length)) != 0) {
						ms.Write(buffer, 0, count);
					}
					
					return ms.ToArray();
				}

				//List<byte[]> lstData = new List<byte[]>();
				////string result = "";
				//int bufSize = 10240;
				//byte[] temp = new byte[bufSize];

				//int totaLen = 0;
				//int readCount = 0;

				//do {
				//	readCount = stream.Read(temp, 0, bufSize);
				//	totaLen += readCount;

				//	if(readCount == bufSize) {
				//		lstData.Add(temp);
				//		temp = new byte[bufSize];
				//	} else {
				//		byte[] newTemp = new byte[readCount];
				//		Array.Copy(temp, newTemp, readCount);
				//		lstData.Add(newTemp);
				//	}
				//	//result += Encoding.UTF8.GetString(data, 0, readCount);
				//} while(readCount >= bufSize);

				////result = Encoding.UTF8.GetString(data, 0, totaLen);

				//byte[] result = new byte[totaLen];
				//int dstIdx = 0;
				//for(int i = 0; i < lstData.Count; ++i) {
				//	int srcLen = lstData[i].Length;
				//	Array.Copy(lstData[i], 0, result, dstIdx, srcLen);
				//	dstIdx += srcLen;
				//}

				//return result;
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
				return new byte[0];
			}
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
