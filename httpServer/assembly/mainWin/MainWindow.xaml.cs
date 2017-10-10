using com.superscene.util;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace httpServer {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		class IpAddr {
			public string ip = "";
			public string port = "";
		};
		enum ServerStatus {
			none, http, transmit
		}

		static MainWindow ins = null;
		string rootPath = "";

		bool stopServer = false;
		Thread thServer = null;
		HttpListener httpListener = null;
		string serverPath = "";
		string serverUrl = "";
		string regPath = "HKCU\\Software\\superHttpServer\\";
		RegistryCtl regCtl = new RegistryCtl();
		Dictionary<string, string> mapSuffix = new Dictionary<string, string>();
		bool isProxy = false;
		string proxyUrl = "";
		bool isTransmit = false;
		string transmitUrl = "";
		ServerStatus lastStatus = ServerStatus.none;
		//string lastTransmit = "";
		string lastServer = "";

		private TimeSpan _timeout = new TimeSpan(0, 0, 0, 0, 0);
		//HttpCtl httpCtl = new HttpCtl();

		public MainWindow() {
			InitializeComponent();

			_timeout = TimeSpan.FromMilliseconds(3000);
			//httpCtl.timeout = 3000;

			mapSuffix[".css"] = "text/css";
			mapSuffix[".js"] = "text/javascript";
			mapSuffix[".html"] = "text/html";
			mapSuffix[".png"] = "image/png";
			mapSuffix[".xml"] = "application/xml";
			mapSuffix[".jpg"] = "image/jpeg";

			ins = this;
			rootPath = AppDomain.CurrentDomain.BaseDirectory;

			string path = regCtl.getValue(regPath + "path", rootPath);
			string port = regCtl.getValue(regPath + "port", "8091");
			int x = 100;
			int y = 100;
			int w = 400;
			int h = 200;
			try {
				x = Int32.Parse(regCtl.getValue(regPath + "x", "100"));
				y = Int32.Parse(regCtl.getValue(regPath + "y", "100"));
				w = Int32.Parse(regCtl.getValue(regPath + "width", "600"));
				h = Int32.Parse(regCtl.getValue(regPath + "height", "250"));
				cbxIp.Text = regCtl.getValue(regPath + "ip", "127.0.0.1");
				txtProxy.IsChecked = regCtl.getValue(regPath + "isProxy", "false") == "true";
				txtProxy.Text = regCtl.getValue(regPath + "proxyUrl", "");
				txtTransmit.IsChecked = regCtl.getValue(regPath + "isTransmit", "false") == "true";
				txtTransmit.Text = regCtl.getValue(regPath + "transmitUrl", "");
			} catch(Exception) {

			}

			this.Left = x;
			this.Top = y;
			this.Width = w;
			this.Height = h;

			serverPath = path + "/";
			txtPath.Text = path;
			lblUrl.Content = "http://127.0.0.1:8091";
			isProxy = txtProxy.IsChecked == true;
			proxyUrl = txtProxy.Text;
			isTransmit = txtTransmit.IsChecked == true;
			transmitUrl = txtTransmit.Text;

			//
			txtProxy.IsChecked = isProxy = false;
			txtProxy.Visibility = Visibility.Collapsed;
			txtTransmit.IsChecked = isTransmit = false;
			txtTransmit.Visibility = Visibility.Collapsed;

			findAllIp();
			txtPort.Text = port;

			//try {
			//	lastStatus = "httpServer";
			//	updateCopyUrlPos();
			//} catch(Exception) {

			//}
			restartServer();
		}

		private void findAllIp() {
			try {
				List<string> lstIP = new List<string>();
				List<string> lstIPv6 = new List<string>();
				string ipLocal = "127.0.0.1";

				string hostName = Dns.GetHostName();//本机名   
				IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6   
				foreach(IPAddress ip in addressList) {
					if(ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal || ip.IsIPv6Teredo || ip.IsIPv4MappedToIPv6) {
						//ipv6
						lstIPv6.Add(ip.ToString());
					} else {
						if(ip.ToString() != ipLocal) {
							lstIP.Add(ip.ToString());
						}
					}
				}

				cbxIp.Items.Add("127.0.0.1");
				//cbxIp.ListItems.Add("127.0.0.1");
				for(int i = 0; i < lstIP.Count; ++i) {
					cbxIp.Items.Add(lstIP[i]);
					//cbxIp.ListItems.Add(lstIP[i]);
				}
				//for(int i = 0; i < lstIPv6.Count; ++i) {
				//	cbxIp.Items.Add(lstIPv6[i]);
				//}
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

		private void restartServer() {
			try {
				clear();
				if(isTransmit) {
					lastStatus = ServerStatus.transmit;

					IpAddr addr = parseServer(transmitUrl);

					string ip = cbxIp.Text;
					string port = txtPort.Text.Trim();
					lastServer = ip + ":" + port;
					runExeNoWin("netsh", "interface portproxy add v4tov4 listenaddress=" + ip + " listenport=" + port + " connectaddress=" + addr.ip + "  connectport=" + addr.port);
					return;
				}
				lastStatus = ServerStatus.http;
				//clear();
				stopServer = false;
				updateCopyUrlPos();
			} catch(Exception) {

			}
		}

		private void updateCopyUrlPos() {
			string ip = cbxIp.Text;
			string port = txtPort.Text.Trim();
			if(port == "") {
				port = "80";
			}

			string url = "http://" + ip + ":" + port;
			lblUrl.Content = url;
			serverUrl = url;
			url += "/";

			httpListener = new HttpListener();

			httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
			httpListener.Prefixes.Add(url);
			httpListener.Start();
			httpListener.BeginGetContext(new AsyncCallback(GetContextCallBack), httpListener);
			//thServer = new Thread(serverProc);
			//thServer.Start();
		}

		static void GetContextCallBack(IAsyncResult ar) {
			try {
				HttpListener sSocket = ar.AsyncState as HttpListener;
				HttpListenerContext context = sSocket.EndGetContext(ar);

				sSocket.BeginGetContext(new AsyncCallback(GetContextCallBack), sSocket);

				MainWindow.ins.responseFile(context);

			} catch { }

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
			if(isProxy) {
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

			string path = serverPath + url;
			//Console.WriteLine(path);
			
			if(!isIndex && !File.Exists(path)) {
				string newPath = serverPath + url + "/index.html";
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
			string realUrl = proxyUrl + url;

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
						location = location.Replace(proxyUrl, serverUrl);
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

		public byte[] readStream(Stream stream, out int len) {
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

		//运行Exe
		private void runExe(string exePath, string param) {
			//定义一个ProcessStartInfo实例
			ProcessStartInfo info = new ProcessStartInfo();
			//设置启动进程的初始目录
			info.WorkingDirectory = rootPath;
			//info.EnvironmentVariables["PATH"] = path;
			info.UseShellExecute = false;
			//info.EnvironmentVariables["PATH"] = path;
			//设置启动进程的应用程序或文档名
			info.FileName = exePath;
			//设置启动进程的参数
			info.Arguments = param;
			//启动由包含进程启动信息的进程资源
			try {
				Process.Start(info);
			} catch(Exception) {
				//MessageBox.Show(this, we.Message);
				return;
			}
		}

		//运行Exe
		public void runExeNoWin(string exePath, string param) {
			Process exep = new Process();
			exep.StartInfo.FileName = exePath;
			exep.StartInfo.Arguments = param;
			exep.StartInfo.CreateNoWindow = true;
			exep.StartInfo.UseShellExecute = false;
			exep.StartInfo.RedirectStandardOutput = true;
			exep.StartInfo.RedirectStandardError = true;
			//设置启动动作,确保以管理员身份运行
			exep.StartInfo.Verb = "runas";
			exep.Start();
			exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
			try {
				string data = exep.StandardOutput.ReadToEnd();
				//Console.WriteLine(data);

				data = exep.StandardError.ReadToEnd();
			} catch(Exception) {
				//Console.WriteLine(ex.ToString());
			}
			exep.Close();
		}

		private void lblUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			runExe("explorer.exe", lblUrl.Content.ToString());
		}

		private void lblCopyUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			Clipboard.SetDataObject(lblUrl.Content);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

		}

		private void Window_Closed(object sender, EventArgs e) {
			clear();
			regCtl.setValue(regPath + "x", this.Left.ToString());
			regCtl.setValue(regPath + "y", this.Top.ToString());
			regCtl.setValue(regPath + "width", this.Width.ToString());
			regCtl.setValue(regPath + "height", this.Height.ToString());

			regCtl.setValue(regPath + "path", txtPath.Text);
			regCtl.setValue(regPath + "ip", cbxIp.Text);
			regCtl.setValue(regPath + "port", txtPort.Text);
			regCtl.setValue(regPath + "isProxy", (txtProxy.IsChecked == true) ? "true" : "false");
			regCtl.setValue(regPath + "proxyUrl", txtProxy.Text);
			regCtl.setValue(regPath + "isTransmit", (txtTransmit.IsChecked == true) ? "true" : "false");
			regCtl.setValue(regPath + "transmitUrl", txtTransmit.Text);
		}

		private void clear() {
			stopServer = true;
			httpListener?.Abort();
			httpListener = null;
			thServer?.Abort();
			thServer = null;

			if(lastStatus == ServerStatus.transmit) {
				//IpAddr addr = parseServer(lastStatus);
				IpAddr addr = parseServer(lastServer);
				runExeNoWin("netsh", "interface portproxy delete v4tov4 listenaddress="+ addr.ip + " listenport=" + addr.port);
			}
			lastStatus = ServerStatus.none;
		}

		private void btnRestart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			//try {
			//	clear();
			//	stopServer = false;
			//	updateCopyUrlPos();
			//} catch(Exception) {

			//}
			restartServer();
		}

		private void txtPath_TextChanged(object sender, TextChangedEventArgs e) {
			serverPath = txtPath.Text + "/";
		}

		private void txtProxy_TextChanged(object sender, TextChangedEventArgs e) {
			proxyUrl = txtProxy.Text;
		}

		private void txtProxy_Checked(object sender, RoutedEventArgs e) {
			isProxy = true;
		}

		private void txtProxy_Unchecked(object sender, RoutedEventArgs e) {
			isProxy = false;
		}

		private void txtTransmit_Checked(object sender, RoutedEventArgs e) {
			isTransmit = true;
		}

		private void txtTransmit_Unchecked(object sender, RoutedEventArgs e) {
			isTransmit = false;
		}

		private void txtTransmit_TextChanged(object sender, TextChangedEventArgs e) {
			transmitUrl = txtTransmit.Text;
		}
	}
}
