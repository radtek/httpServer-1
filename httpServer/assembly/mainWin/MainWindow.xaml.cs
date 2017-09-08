using com.superscene.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
		static MainWindow ins = null;
		string rootPath = "";

		bool stopServer = false;
		Thread thServer = null;
		HttpListener httpListener = null;
		string serverPath = "";
		string regPath = "HKCU\\Software\\superHttpServer\\";
		RegistryCtl regCtl = new RegistryCtl();
		Dictionary<string, string> mapSuffix = new Dictionary<string, string>();

		public MainWindow() {
			InitializeComponent();

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
			} catch(Exception) {

			}

			this.Left = x;
			this.Top = y;
			this.Width = w;
			this.Height = h;

			serverPath = path + "/";
			txtPath.Text = path;
			lblUrl.Content = "http://127.0.0.1:8091";

			findAllIp();
			txtPort.Text = port;

			try {
				updateCopyUrlPos();
			} catch(Exception) {

			}
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
				for(int i = 0; i < lstIP.Count; ++i) {
					cbxIp.Items.Add(lstIP[i]);
				}
				//for(int i = 0; i < lstIPv6.Count; ++i) {
				//	cbxIp.Items.Add(lstIPv6[i]);
				//}
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
			string url = httpListenerContext.Request.Url.AbsolutePath;
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
					httpListenerContext.Response.Headers.Add("Location", "http://127.0.0.1:8091/demo/basic/");
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
			info.FileName = "explorer.exe";
			//设置启动进程的参数
			info.Arguments = param;
			//启动由包含进程启动信息的进程资源
			try {
				Process.Start(info);
			} catch(Exception wx) {
				//MessageBox.Show(this, we.Message);
				return;
			}
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
			regCtl.setValue(regPath + "path", txtPath.Text);
			regCtl.setValue(regPath + "ip", cbxIp.Text);
			regCtl.setValue(regPath + "port", txtPort.Text);
			regCtl.setValue(regPath + "x", this.Left.ToString());
			regCtl.setValue(regPath + "y", this.Top.ToString());
			regCtl.setValue(regPath + "width", this.Width.ToString());
			regCtl.setValue(regPath + "height", this.Height.ToString());
		}

		private void clear() {
			stopServer = true;
			httpListener?.Abort();
			thServer?.Abort();
		}

		private void btnRestart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			try {
				clear();
				stopServer = false;
				updateCopyUrlPos();
			} catch(Exception) {

			}
		}

		private void txtPath_TextChanged(object sender, TextChangedEventArgs e) {
			serverPath = txtPath.Text + "/";
		}
	}
}
