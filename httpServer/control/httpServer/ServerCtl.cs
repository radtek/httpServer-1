using csharpHelp.util;
using HttpMultipartParser;
using httpServer.view.page;
using httpServer.model;
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
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace httpServer.control {
	public class ServerCtl {
		public class IpAddr {
			public string ip = "";
			public string port = "";
		};

		public static Dictionary<string, string> mapSuffix = new Dictionary<string, string>();
		public static int timeout = 3000;
		static Cache cache = new Cache();

		public string ip = "";
		public HttpServerMd md = null;
		
		string serverUrl = "";
		
		private TimeSpan _timeout = new TimeSpan(0, 0, 0, 0, 0);
		Thread thServer = null;
		HttpListener httpListener = null;

		//RequestCtl requestCtl = null;

		static ServerCtl(){
			mapSuffix[".css"] = "text/css";
			mapSuffix[".js"] = "text/javascript";
			mapSuffix[".html"] = "text/html";
			mapSuffix[".png"] = "image/png";
			mapSuffix[".xml"] = "application/xml";
			mapSuffix[".jpg"] = "image/jpeg";
			mapSuffix[".svg"] = "image/svg+xml";
		}

		public static void setContentType(Dictionary<string, string> map) {
			foreach(string key in map.Keys) {
				mapSuffix[key] = map[key];
			}
		}

		public ServerCtl() {
			_timeout = TimeSpan.FromMilliseconds(timeout);

			//requestCtl = new RequestCtl();
		}

		public void restartServer() {
			try {
				clear();
				updateCopyUrlPos();
			} catch(Exception ex) {
				MainWindow.ins.error(ex);
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
			string port = ""+md.port;
			if(port == "") {
				port = "80";
			}

			string protocol = "http";
			if(md.isHttps) {
				protocol = "https";
			}
			string url = protocol + "://" + ip + ":" + port;
			serverUrl = url;
			url += "/";

			httpListener = new HttpListener();

			httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
			httpListener.Prefixes.Add(url);
			httpListener.Start();
			httpListener.BeginGetContext(new AsyncCallback(GetContextCallBack), this);
		}

		static void GetContextCallBack(IAsyncResult ar) {
			try {
				ServerCtl ctl = ar.AsyncState as ServerCtl;
				HttpListener sSocket = ctl.httpListener;
				if(sSocket == null || !sSocket.IsListening) {
					return;
				}
				HttpListenerContext context = sSocket.EndGetContext(ar);
				
				sSocket.BeginGetContext(new AsyncCallback(GetContextCallBack), ctl);

				ctl.responseFile(context);

			} catch(Exception ex) {
				MainWindow.ins.error(ex);
			}
		}

		private void responseFile(HttpListenerContext httpListenerContext) {
			//if(md.isProxy) {
			//	resoonseProxy(httpListenerContext);
			//	return;
			//}

			string url = System.Web.HttpUtility.UrlDecode(httpListenerContext.Request.Url.AbsolutePath);
			foreach(string key in md.mapRewrite.Keys) {
				if(url.IndexOf(key) >= 0) {
					var arr = md.mapRewrite[key].Split(',');
					string rewriteUrl = md.mapRewrite[key];
					string rewriteParam = "";

					if(arr.Length >= 2) {
						rewriteUrl = arr[0].Trim();
						rewriteParam = arr[1].Trim();
					}
					url = System.Web.HttpUtility.UrlDecode(httpListenerContext.Request.Url.PathAndQuery);
					int idx = url.IndexOf(key);
					string newUrl = rewriteUrl + url.Substring(0, idx) + rewriteParam + url.Substring(idx + key.Length);

					rewrite(newUrl, httpListenerContext);
					return;
				}
			}
			
			bool isIndex = false;
			if(url != "" && url[url.Length - 1] == '/') {
				isIndex = true;
				url += "index.html";
			}

			string path = "";
			try {
				path = Path.GetFullPath(md.path + "/" + url.Substring(md.virtualDir.Length + 1));
			} catch(Exception ex) {
				MainWindow.ins.error(ex);
			}

			if(!isIndex && !File.Exists(path)) {
				string newPath = md.path + "/" + url.Substring(md.virtualDir.Length + 1) + "/index.html";
				if(File.Exists(newPath)) {
					httpListenerContext.Response.StatusCode = 302;
					httpListenerContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
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
					//重定向匹配
					foreach(string key in md.mapAutoRewrite.Keys) {
						if(url.IndexOf(key) < 0) {
							continue;
						}
						
						string rewriteUrl = md.mapAutoRewrite[key];
						url = System.Web.HttpUtility.UrlDecode(httpListenerContext.Request.Url.PathAndQuery);
						int idx = url.IndexOf(key);
						string newUrl = rewriteUrl + url.Substring(0, idx) + url.Substring(idx + key.Length);
						//Debug.WriteLine(newUrl);
						rewrite(newUrl, httpListenerContext);
						return;
					}
					//404
					httpListenerContext.Response.StatusCode = 404;
				}
			} else {
				//文件存在
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
		private async void rewrite(string newUrl, HttpListenerContext httpListenerContext) {
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

					foreach(string key in httpListenerContext.Request.Headers.Keys) {
						try {
							string val = httpListenerContext.Request.Headers[key];

							if(key.ToLower() == "host") {
								val = (new Regex("(?:.*?://)(.*?)(?:/)")).Match(newUrl).Groups[1].Value;
							} else if(key.ToLower() == "origin") {
								val = (new Regex("(.*?://.*?)(?:/)")).Match(newUrl).Groups[1].Value;
							} else if(key.ToLower() == "referer") {
								val = newUrl;
							}
							client.DefaultRequestHeaders.Add(key, val);
						} catch(Exception) { }
					}
					
					HttpResponseMessage response = null;
					if(httpListenerContext.Request.HttpMethod.ToLower() == "post") {
						var postData = new ByteArrayContent(intputData, 0, inputLen);
						response = await client.PostAsync(newUrl, postData);
					} else {
						response = await client.GetAsync(newUrl);
					}

					bool isResponseOk = response.IsSuccessStatusCode;
					HttpStatusCode code = response.StatusCode;
					if(isResponseOk) {
						Stream sResult = await response.Content.ReadAsStreamAsync();
						result = readStream(sResult, out dataLen);
					}
					
					int statusCode = (int)code;
					httpListenerContext.Response.StatusCode = statusCode;

					var lst = response.Headers.ToList();
					for(int i = 0; i < lst.Count; ++i) {
						var vals = (lst[i].Value as string[]);
						if(vals != null && vals.Length > 0) {
							try {
								httpListenerContext.Response.Headers.Add(lst[i].Key, vals[0]);
							} catch(Exception) { }
						}
					}
					var lst2 = response.Content.Headers.ToList();
					for(int i = 0; i < lst2.Count; ++i) {
						var vals = (lst2[i].Value as string[]);
						if(vals != null && vals.Length > 0) {
							try {
								httpListenerContext.Response.Headers.Add(lst2[i].Key, vals[0]);
							} catch(Exception) { }
						}
					}
				}
				
				var output = httpListenerContext.Response.OutputStream;
				output.Write(result, 0, dataLen);
				output.Close();
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
				MainWindow.ins.debug(ex);

				try {
					httpListenerContext.Response.StatusCode = 404;
					var output = httpListenerContext.Response.OutputStream;
					output.Close();
				} catch(Exception ex2) {
					MainWindow.ins.debug(ex2);
				}
			}
		}

		private bool parseCtl(string url, HttpListenerContext ctx) {
			//bool isDeal = false;

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

			return false;
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
				
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
				MainWindow.ins.debug(ex);
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
				
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
				MainWindow.ins.debug(ex);
				return new byte[0];
			}
		}

		public void clear() {
			httpListener?.Abort();
			httpListener = null;
			thServer?.Abort();
			thServer = null;
		}

	}
}
