using com.superscene.util;
using com.superscene.util.action;
using httpServer.assembly.util;
using httpServer.control;
using httpServer.entity;
using httpServer.module;
using httpServer.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace httpServer.assembly.page {
	class HttpModel : ServerModule {
		public ServerCtl ctl = null;

		public string ip = "127.0.0.1";     //
		public string port = "";
		public string urlParam = "";

		public string path = "";    //路径

		public bool isProxy = false;    //反向代理
		public string proxyUrl = "";

		public bool isTransmit = false; //端口转发
		public string transmitUrl = "";

		public override void load(RegistryCtl regCtl, string subPath) {
			base.load(regCtl, subPath);

			string rootPath = AppDomain.CurrentDomain.BaseDirectory;

			ip = regCtl.getValue(subPath + "ip", "127.0.0.1");
			port = regCtl.getValue(subPath + "port", "8091");
			path = regCtl.getValue(subPath + "path", rootPath);
			urlParam = regCtl.getValue(subPath + "urlParam", "");
		}

		public override void save(RegistryCtl regCtl, string subPath) {
			base.save(regCtl, subPath);

			regCtl.setValue(subPath + "ip", ip);
			regCtl.setValue(subPath + "port", port);
			regCtl.setValue(subPath + "path", path);
			regCtl.setValue(subPath + "urlParam", urlParam);
		}
	};

	/// <summary>
	/// HttpServerWin.xaml 的交互逻辑
	/// http服务器
	/// </summary>
	public partial class HttpServerWin : UserControl, IPage {
		//HttpModel data = new HttpModel();
		Entity ent = null;

		//int dataIndex = 0;
		//ServerItem serverItem = null;
		private HttpModel nowData = null;
		//private List<ServerCtl> lstServerClt = new List<ServerCtl>();

		public HttpServerWin() {
			InitializeComponent();
		}

		public void init() {
			ent = Entity.getInstance();

			txtProxy.Visibility = Visibility.Collapsed;
			txtTransmit.Visibility = Visibility.Collapsed;

			//find ip
			List<string> lstIP = UiService.ins().getAllIp();
			cbxIp.Items.Add("127.0.0.1");
			for(int i = 0; i < lstIP.Count; ++i) {
				cbxIp.Items.Add(lstIP[i]);
			}
		}

		public void initData(ServerModule _md) {
			HttpModel md = (HttpModel)_md;

			ServerCtl ctl = new ServerCtl();
			ctl.md = md;
			md.ctl = ctl;
			//lstServerClt.Add(ctl);
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

		public void clear(ServerModule _md) {
			HttpModel md = (HttpModel)_md;
			md.ctl.clear();
		}

		public void updateData(ServerModule _md) {
			HttpModel md = (HttpModel)_md;
			nowData = md;

			txtDesc.Text = md.desc;

			cbxIp.Text = md.ip;
			txtPort.Text = md.port;

			txtPath.Text = md.path;
			txtUrlParam.Text = md.urlParam;

			txtProxy.IsChecked = md.isProxy;
			txtProxy.Text = md.proxyUrl;

			txtTransmit.IsChecked = md.isTransmit;
			txtTransmit.Text = md.transmitUrl;

			lblUrl.Content = "http://" + md.ip + ":" + md.port + "/" + md.urlParam;
		}

		public ServerModule createModel() {
			return new HttpModel();
			//return data;
		}

		public ServerModule createNewModel() {
			HttpModel md = new HttpModel();
			md.port = SystemCtl.getFreePort(8091).ToString();
			md.desc = md.ip + ":" + md.port;
			md.path = SysConst.rootPath();
			return md;
			//return data;
		}

		private void lblUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			CommonUtil.runExe("explorer.exe", lblUrl.Content.ToString());
		}

		private void lblCopyUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			Clipboard.SetDataObject(lblUrl.Content);
		}

		private void txtPath_TextChanged(object sender, TextChangedEventArgs e) {
			//serverPath = txtPath.Text + "/";
			updateData("path", txtPath.Text);
		}

		private void txtProxy_TextChanged(object sender, TextChangedEventArgs e) {
			//proxyUrl = txtProxy.Text;
			updateData("proxyUrl", txtProxy.Text);
		}

		private void txtProxy_Checked(object sender, RoutedEventArgs e) {
			//isProxy = true;
			updateData("isProxy", true);
		}

		private void txtProxy_Unchecked(object sender, RoutedEventArgs e) {
			//isProxy = false;
			updateData("isProxy", false);
		}

		private void txtTransmit_Checked(object sender, RoutedEventArgs e) {
			//isTransmit = true;
			updateData("isTransmit", true);
		}

		private void txtTransmit_Unchecked(object sender, RoutedEventArgs e) {
			//isTransmit = false;
			updateData("isTransmit", false);
		}

		private void txtTransmit_TextChanged(object sender, TextChangedEventArgs e) {
			updateData("transmitUrl", txtTransmit.Text);
		}

		private void txtDesc_TextChanged(object sender, TextChangedEventArgs e) {
			updateData("desc", txtDesc.Text);
		}

		private void txtUrlParam_TextChanged(object sender, TextChangedEventArgs e) {
			updateData("urlParam", txtUrlParam.Text);
		}

		private void updateData(string name, string value) {
			//int idx = dataIndex;
			//if(idx < 0 || serverItem == null) {
			//	return;
			//}
			if (nowData == null) {
				return;
			}

			HttpModel md = nowData;

			switch(name) {
			case "desc": {
					md.desc = value;
					md.serverItem.Content = value;
					break;
				}
			case "transmitUrl": md.transmitUrl = value; break;
			case "proxyUrl": md.proxyUrl = value; break;
			case "path": md.path = value; break;
			case "urlParam": {
					md.urlParam = value;
					lblUrl.Content = "http://" + md.ip + ":" + md.port + "/" + md.urlParam;
					break;
				}
			}
		}

		private void updateData(string name, bool value) {
			//int idx = dataIndex;
			//if(idx < 0 || serverItem == null) {
			//	return;
			//}
			if (nowData == null) {
				return;
			}

			HttpModel md = nowData;

			switch (name) {
			case "isTransmit": md.isTransmit = value; break;
			case "isProxy": md.isProxy = value; break;
			case "isRun": {
					md.isRun = value;
					//md.serverItem.Source = ent.mainWin.getServerStatusImgPath(md.isRun);
					if(value == true) {
						md.ip = cbxIp.Text;
						md.port = txtPort.Text;
						lblUrl.Content = "http://" + md.ip + ":" + md.port + "/" + md.urlParam;
						//lstServerClt[idx].restartServer();
						md.ctl.restartServer();
					} else {
						//lstServerClt[idx].clear();
						md.ctl.clear();
					}
					if(md.desc == "" || ComServerCtl.isDescIp(md.desc)) {
						txtDesc.Text = md.ip + ":" + md.port;
						updateData("desc", md.ip + ":" + md.port);
					}
					break;
				}
			}
		}

		//private bool isDescIp(string desc) {
		//	return Regex.IsMatch(desc, "^[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}:[0-9]{1,5}");
		//}

	}
}
