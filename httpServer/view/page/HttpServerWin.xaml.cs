using csharpHelp;
using csharpHelp.util;
using csharpHelp.util.action;
using httpServer.view.util;
using httpServer.control;
using httpServer.model;
using httpServer.services;
using httpServer.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace httpServer.view.page {

	public class HttpServerMd {
		public ServerItem serverItem = null;

		[XmlValue("desc")] public string desc = "";    //描述
		[XmlAttr("isRun")] public bool isRun = false;  //是否启动

													   //[XmlValue("desc")] public string desc = "";
		[XmlAttr("ip")]		public string ip = "0.0.0.0";
		[XmlAttr("port")]	public int port = 0;

		[XmlValue("urlParam")]		public string urlParam = "";
		[XmlValue("path")]			public string path = "";
		[XmlValue("virtualDir")]	public string virtualDir = "";
		[XmlValue("rewrite")]		public string _rewrite = "";

		public HttpServerGroup ctl = null;

		public Dictionary<string, string> mapAutoRewrite = new Dictionary<string, string>();
		public Dictionary<string, string> mapRewrite = new Dictionary<string, string>();
		
		public string rewrite {
			get { return _rewrite; }
			set {
				_rewrite = value;
				updateRewrite();
			}
		}

		private void updateRewrite() {
			string[] arr = _rewrite.Split(new string[] { "\r\n", ";", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			mapAutoRewrite = new Dictionary<string, string>();
			mapRewrite = new Dictionary<string, string>();

			Regex regAuto = new Regex("^(?:-['\"‘“]?)(.*?)(?:['\"‘“]?)(?:[\\s]*=[\\s]*)(?:['\"‘“]?)([^'\"‘“]*)(?:['\"‘“]?)");
			Regex reg = new Regex("^(?:['\"‘“]?)(.*?)(?:['\"‘“]?)(?:[\\s]*=[\\s]*)(?:['\"‘“]?)([^'\"‘“]*)(?:['\"‘“]?)");

			for(int i = 0; i < arr.Length; ++i) {
				Match matchAuto = regAuto.Match(arr[i].Trim());
				if(matchAuto.Groups.Count >= 3) {
					mapAutoRewrite[matchAuto.Groups[1].Value] = matchAuto.Groups[2].Value;
					continue;
				}

				Match match = reg.Match(arr[i].Trim());
				if(match.Groups.Count < 3) {
					continue;
				}

				mapRewrite[match.Groups[1].Value] = match.Groups[2].Value;
			}
		}

		public void load(XmlCtl xmlCtl) {
			desc = xmlCtl.value("desc");
			isRun = xmlCtl.attrBool("isRun");

			string rootPath = AppDomain.CurrentDomain.BaseDirectory;

			ip = xmlCtl.attr( "ip", "127.0.0.1");
			port = xmlCtl.attrInt( "port", 8091);
			path = xmlCtl.value("path", rootPath);
			urlParam = xmlCtl.value("urlParam", "");
			_rewrite = xmlCtl.value("rewrite", "");
		}

		public int getMaxPort() {
			return port;
		}
	}

	/// <summary>
	/// HttpServerWin.xaml 的交互逻辑
	/// http服务器
	/// </summary>
	public partial class HttpServerWin : UserControl {
		private HttpServerMd nowData = new HttpServerMd();

		List<string> lstIPCache = new List<string>();

		public HttpServerWin() {
			InitializeComponent();

			//set language
			new ClassLink().sendTo(Lang.ins, this, "", LinkType.All);
		}

		public List<string> getAllIp() {
			if(lstIPCache.Count <= 0) {
				lstIPCache = ComUtil.findAllIp();
			}

			return lstIPCache;
		}

		public void init() {
			//find ip
			List<string> lstIP = getAllIp();
			cbxIp.Items.Add("0.0.0.0");
			cbxIp.Items.Add("localhost");
			cbxIp.Items.Add("127.0.0.1");
			for(int i = 0; i < lstIP.Count; ++i) {
				cbxIp.Items.Add(lstIP[i]);
			}
		}

		public HttpServerMd createNewModel() {
			HttpServerMd md = new HttpServerMd();
			md.port = SystemCtl.getFreePort(MainMd.ins.maxStartPort());

			md.desc = md.ip + ":" + md.port;
			md.path = SysConst.rootPath();
			return md;
		}

		public void initData(HttpServerMd md) {
			HttpServerGroup ctl = new HttpServerGroup();
			ctl.md = md;
			md.ctl = ctl;

			if (md.isRun) {
				ctl.restartServer();
			}
		}

		public void start() {
			updateData("isRun", true);
		}

		public void stop() {
			updateData("isRun", false);
		}

		public void clear(HttpServerMd md) {
			md.ctl.clear();
		}

		public void updateData(HttpServerMd md) {
			nowData = md;

			txtDesc.Text = md.desc;

			cbxIp.Text = md.ip;
			txtPort.Text = "" + md.port;

			txtPath.Text = md.path;
			txtVirtualDir.Text = md.virtualDir;
			txtUrlParam.Text = md.urlParam;
			txtRewrite.Text = md.rewrite;
			
			lblUrl.Content = getUrl();
		}

		public HttpServerMd createModel() {
			return new HttpServerMd();
		}

		private void lblUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			ComUtil.runExe("explorer.exe", lblUrl.Content.ToString());
		}

		private void lblCopyUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			Clipboard.SetDataObject(lblUrl.Content);
		}

		private void txtPath_TextChanged(object sender, TextChangedEventArgs e) {
			updateData("path", txtPath.Text);
		}

		private void txtDesc_TextChanged(object sender, TextChangedEventArgs e) {
			updateData("desc", txtDesc.Text);
		}
		private void TxtVirtualDir_TextChanged(object sender, TextChangedEventArgs e) {
			updateData("virtualDir", txtVirtualDir.Text);
		}

		private void txtUrlParam_TextChanged(object sender, TextChangedEventArgs e) {
			updateData("urlParam", txtUrlParam.Text);
		}

		private void txtRewrite_TextChanged(object sender, TextChangedEventArgs e) {
			updateData("rewrite", txtRewrite.Text);
		}

		private void updateData(string name, string value) {
			if (nowData == null) {
				return;
			}

			HttpServerMd md = nowData;

			switch(name) {
			case "desc": {
				md.desc = value;
				md.serverItem.Content = value;
				break;
			}
			case "path": md.path = value; break;
			case "urlParam": {
				md.urlParam = value;
				lblUrl.Content = getUrl();
				break;
			}
			case "rewrite": md.rewrite = value; break;
			case "virtualDir": {
				md.virtualDir = value;
				lblUrl.Content = getUrl();
				break;
			}
			}
			MainWindow.ins.delaySaveConfig();
			//Debug.WriteLine("aaa");
		}

		private string getUrl() {
			HttpServerMd md = nowData;

			string ip = (md.ip == "0.0.0.0" ? "localhost" : md.ip);
			string dir = md.virtualDir.Trim();
			if(dir != "") {
				dir = dir + "/";
			}
			return "http://" + ip + ":" + md.port + "/" + dir + md.urlParam;
		}

		private int toInt(string str, int def = 0) {
			bool isOk = int.TryParse(str, out int rst);
			if(!isOk) {
				rst = def;
			}
			return rst;
		}

		private void updateData(string name, bool value) {
			if (nowData == null) {
				return;
			}

			HttpServerMd md = nowData;

			switch (name) {
			case "isRun": {
					md.isRun = value;
					var lastIp = md.ip;
					var lastPort = md.port;
					md.ip = cbxIp.Text;
					md.port = toInt(txtPort.Text, md.port);
					lblUrl.Content = getUrl();

					md.desc = md.desc.Replace(lastIp, md.ip);
					md.desc = md.desc.Replace(""+lastPort, ""+md.port);
					txtDesc.Text = md.desc;
					md.serverItem.Content = md.desc;

					if(value == true) {
						md.ctl.restartServer();
					} else {
						md.ctl.clear();
					}
					break;
				}
			}
			MainWindow.ins.delaySaveConfig();
			//Debug.WriteLine("bbb");
		}

		private void cbxIp_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || cbxIp.Text == nowData.ip) {
				return;
			}
			nowData.serverItem.Content = nowData.desc + " *";
			//CmdServ.cfgWaitSave.send();
		}

		private void txtPort_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtPort.Text == ""+nowData.port) {
				return;
			}
			nowData.serverItem.Content = nowData.desc + " *";
			//CmdServ.cfgWaitSave.send();
		}

	}
}
