using csharpHelp.util;
using csharpHelp.util.action;
using httpServer.view.util;
using httpServer.control;
using httpServer.control.httpRevProxy;
using httpServer.entity;
using httpServer.model;
using httpServer.services;
using httpServer.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using csharpHelp;

namespace httpServer.view.page {
	public class RevProxyMd : ServerMd {
		public Dictionary<string, IServerCtl> mapCtl = new Dictionary<string, IServerCtl>();

		//public IServerCtl localCtl = new HttpRevClient();
		//public IServerCtl serverCtl = new HttpRevServer();
		[XmlAttr("revType")] public string revType = "local";
		[XmlValue("urlParam")] public string urlParam = "";

		[XmlAttr("local.ctlIp")] public string localCtlIp = "127.0.0.1";
		//public string localCtlPort = "8091";
		[XmlAttr("local.ctlPort")] public int localCtlPort = 8091;
		[XmlAttr("local.httpIp")] public string localHttpIp = "127.0.0.1";
		//public string localHttpPort = "8091";
		[XmlAttr("local.httpPort")] public int localHttpPort = 8091;

		[XmlAttr("server.ctlIp")] public string ctlIp = "127.0.0.1";
		//public string ctlPort = "";
		[XmlAttr("server.ctlPort")] public int ctlPort = 0;
		[XmlAttr("server.httpIp")] public string httpIp = "127.0.0.1";
		//public string httpPort = "";
		[XmlAttr("server.httpPort")] public int httpPort = 80;

		public RevProxyMd() {
			mapCtl["local"] = new HttpRevClient(this);
			mapCtl["server"] = new HttpRevServer(this);
		}

		public void clearCtl() {
			foreach(string key in mapCtl.Keys) {
				mapCtl[key].clear();
			}
		}

		public override void load(XmlCtl xmlCtl) {
			base.load(xmlCtl);

			string rootPath = AppDomain.CurrentDomain.BaseDirectory;

			revType = xmlCtl.attr("revType", "local");
			urlParam = xmlCtl.value("urlParam", "");

			localCtlIp = xmlCtl.attr("local.ctlIp", "127.0.0.1");
			localCtlPort = xmlCtl.attrInt("local.ctlPort", 8091);
			localHttpIp = xmlCtl.attr("local.httpIp", "127.0.0.1");
			localHttpPort = xmlCtl.attrInt("local.httpPort", 8091);

			ctlIp = xmlCtl.attr("server.ctlIp", "127.0.0.1");
			ctlPort = xmlCtl.attrInt("server.ctlPort", 8091);
			httpIp = xmlCtl.attr("server.httpIp", "127.0.0.1");
			httpPort = xmlCtl.attrInt("server.httpPort", 8092);
		}

		//public override void load(RegistryCtl regCtl, string subPath) {
		//	base.load(regCtl, subPath);

		//	string rootPath = AppDomain.CurrentDomain.BaseDirectory;

		//	revType = regCtl.getValue(subPath + "revType", "local");

		//	localCtlIp = regCtl.getValue(subPath + "localCtlIp", "127.0.0.1");
		//	localCtlPort = regCtl.getValue(subPath + "localCtlPort", "8091");
		//	localHttpIp = regCtl.getValue(subPath + "localHttpIp", "127.0.0.1");
		//	localHttpPort = regCtl.getValue(subPath + "localHttpPort", "8091");
		//	urlParam = regCtl.getValue(subPath + "urlParam", "");

		//	ctlIp = regCtl.getValue(subPath + "ctlIp", "127.0.0.1");
		//	ctlPort = regCtl.getValue(subPath + "ctlPort", "8091");
		//	httpIp = regCtl.getValue(subPath + "httpIp", "127.0.0.1");
		//	httpPort = regCtl.getValue(subPath + "httpPort", "8092");
		//}

		//public override void save(RegistryCtl regCtl, string subPath) {
		//	base.save(regCtl, subPath);

		//	regCtl.setValue(subPath + "revType", revType);

		//	regCtl.setValue(subPath + "localCtlIp", localCtlIp);
		//	regCtl.setValue(subPath + "localCtlPort", localCtlPort);
		//	regCtl.setValue(subPath + "localHttpIp", localHttpIp);
		//	regCtl.setValue(subPath + "localHttpPort", localHttpPort);
		//	regCtl.setValue(subPath + "urlParam", urlParam);

		//	regCtl.setValue(subPath + "ctlIp", ctlIp);
		//	regCtl.setValue(subPath + "ctlPort", ctlPort);
		//	regCtl.setValue(subPath + "httpIp", httpIp);
		//	regCtl.setValue(subPath + "httpPort", httpPort);
		//}

		public override int getMaxPort() {
			try {
				if(revType == "local") {
					return base.getMaxPort();
				}
				//int iCtlPort = int.Parse(ctlPort);
				//int iHttpPort = int.Parse(httpPort);
				int maxPort = Entity.getInstance().mainMd.maxPort;

				int port = Math.Max(ctlPort, httpPort);
				if(port > maxPort) {
					return Math.Min(ctlPort, httpPort);
				} else {
					return port;
				}
			} catch(Exception) {
				return base.getMaxPort();
			}
		}
	}

	/// <summary>
	/// HttpRevProxy.xaml 的交互逻辑
	/// Http反向代理
	/// </summary>
	public partial class HttpRevProxy : UserControl, IPage {
		Entity ent = null;
		private RevProxyMd nowData = null;
		//private HttpRevServer server = new HttpRevServer();
		//private HttpRevClient client = new HttpRevClient();

		public HttpRevProxy() {
			InitializeComponent();
		}

		public void init() {
			ent = Entity.getInstance();

			List<string> lstIP = UiService.ins().getAllIp();
			cbxCtlIp.Items.Add("localhost");
			cbxCtlIp.Items.Add("127.0.0.1");
			cbxHttpIp.Items.Add("localhost");
			cbxHttpIp.Items.Add("127.0.0.1");
			for(int i = 0; i < lstIP.Count; ++i) {
				cbxCtlIp.Items.Add(lstIP[i]);
				cbxHttpIp.Items.Add(lstIP[i]);
			}
		}

		public ServerMd createNewModel() {
			RevProxyMd md = new RevProxyMd();
			int port = ent.mainMd.maxStartPort();
			md.ctlPort = SystemCtl.getFreePort(port);
			md.httpPort = SystemCtl.getFreePort(port + 1);

			//ent.mainModule.updateStartPort(md.ctlPort);
			//ent.mainModule.updateStartPort(md.httpPort);

			md.desc = md.ctlIp + ":" + md.ctlPort;
			return md;
		}

		public void initData(ServerMd _md) {
			RevProxyMd md = (RevProxyMd)_md;
			//ent.mainModule.updateStartPort(md.ctlPort);
			//ent.mainModule.updateStartPort(md.httpPort);

			//if(md.revType == "local") {
			//	md.serverCtl = new HttpRevClient();
			//} else {
			//	md.serverCtl = new HttpRevServer();
			//}

			if(md.isRun) {
				//md.clearCtl();
				if(md.mapCtl.ContainsKey(md.revType)) {
					md.mapCtl[md.revType].restartServer();
				}
			}
		}

		public ServerMd createModel() {
			return new RevProxyMd();
		}

		public void updateData(ServerMd _md) {
			RevProxyMd md = (RevProxyMd)_md;
			nowData = md;

			txtDesc.Text = md.desc;

			txtLocalCtlIp.Text = md.localCtlIp;
			txtLocalCtlPort.Text = md.localCtlPort.ToString();
			txtLocalHttpIp.Text = md.localHttpIp;
			txtLocalHttpPort.Text = md.localHttpPort.ToString();
			txtUrlParam.Text = md.urlParam;

			cbxCtlIp.Text = md.ctlIp;
			txtCtlPort.Text = md.ctlPort.ToString();
			cbxHttpIp.Text = md.httpIp;
			txtHttpPort.Text = md.httpPort.ToString();

			lblUrl.Content = "http://" + md.httpIp + ":" + md.httpPort + "/" + md.urlParam;
			updateRevType(md.revType);
		}

		public void start() {
			startServer(true);
		}

		public void stop() {
			startServer(false);
		}
		
		private int toInt(string str, int def = 0) {
			bool isOk = int.TryParse(str, out int rst);
			if(!isOk) {
				rst = def;
			}
			return rst;
		}

		private void startServer(bool isStart) {
			RevProxyMd md = nowData;
			md.isRun = isStart;

			md.revType = borLocal.Visibility == Visibility.Visible ? "local" : "server";
			md.localCtlIp = txtLocalCtlIp.Text;
			md.localCtlPort = toInt(txtLocalCtlPort.Text);
			md.localHttpIp = txtLocalHttpIp.Text;
			md.localHttpPort = toInt(txtLocalHttpPort.Text);
			md.urlParam = txtUrlParam.Text;

			md.ctlIp = cbxCtlIp.Text;
			md.ctlPort = toInt(txtCtlPort.Text);
			md.httpIp = cbxHttpIp.Text;
			md.httpPort = toInt(txtHttpPort.Text);

			lblUrl.Content = "http://" + md.httpIp + ":" + md.httpPort + "/" + md.urlParam;
			updateDesc();

			md.clearCtl();
			if(isStart) {
				if(md.mapCtl.ContainsKey(md.revType)) {
					md.mapCtl[md.revType].restartServer();
				}
			}
		}

		public void clear(ServerMd _md) {
			RevProxyMd md = (RevProxyMd)_md;
			md.clearCtl();
		}

		private void updateDesc() {
			RevProxyMd md = nowData;

			if(md.desc == "" || ComServerCtl.isDescIp(md.desc)) {
				if(md.revType == "local") {
					md.desc = md.localCtlIp + ":" + md.localCtlPort;
				} else {
					md.desc = md.httpIp + ":" + md.httpPort;
				}
				txtDesc.Text = md.desc;
				md.serverItem.Content = md.desc;
			}
		}

		private void txtDesc_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null) { return; }

			RevProxyMd md = nowData;
			md.desc = txtDesc.Text;
			md.serverItem.Content = txtDesc.Text;
		}

		private void lblLocal_MouseEnter(object sender, MouseEventArgs e) {
			if(nowData == null) { return; }
			borLocal.Visibility = Visibility.Visible;
		}

		private void lblLocal_MouseLeave(object sender, MouseEventArgs e) {
			if(nowData == null) { return; }
			borLocal.Visibility = nowData.revType == "local" ? Visibility.Visible : Visibility.Hidden;
		}

		private void lblServer_MouseEnter(object sender, MouseEventArgs e) {
			if(nowData == null) { return; }
			borServer.Visibility = Visibility.Visible;
		}

		private void lblServer_MouseLeave(object sender, MouseEventArgs e) {
			if(nowData == null) { return; }
			borServer.Visibility = nowData.revType == "server" ? Visibility.Visible : Visibility.Hidden;
		}

		private void lblLocal_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if(nowData == null) { return; }
			updateRevType("local");
			nowData.revType = "local";
			updateDesc();
		}

		private void lblServer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if(nowData == null) { return; }
			updateRevType("server");
			nowData.revType = "server";
			updateDesc();
		}

		private void updateRevType(string revType) {
			if(revType == "local") {
				borLocal.Visibility = Visibility.Visible;
				borServer.Visibility = Visibility.Hidden;

				lblUrl.Visibility = Visibility.Hidden;
				lblCopyUrl.Visibility = Visibility.Hidden;
				txtUrlParam.Visibility = Visibility.Collapsed;
			} else {
				borLocal.Visibility = Visibility.Hidden;
				borServer.Visibility = Visibility.Visible;

				lblUrl.Visibility = Visibility.Visible;
				lblCopyUrl.Visibility = Visibility.Visible;
				txtUrlParam.Visibility = Visibility.Visible;

			}
		}

		private void lblUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			ComUtil.runExe("explorer.exe", lblUrl.Content.ToString());
		}

		private void lblCopyUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			Clipboard.SetDataObject(lblUrl.Content);
		}

		private void txtUrlParam_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null) { return; }
			nowData.urlParam = txtUrlParam.Text;
			lblUrl.Content = "http://" + nowData.httpIp + ":" + nowData.httpPort + "/" + nowData.urlParam;
		}

		private void txtLocalCtlIp_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtLocalCtlIp.Text == nowData.localCtlIp.ToString()) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void txtLocalCtlPort_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtLocalCtlPort.Text == nowData.localCtlPort.ToString()) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void txtLocalHttpIp_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtLocalHttpIp.Text == nowData.localHttpIp) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void txtLocalHttpPort_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtLocalHttpPort.Text == nowData.localHttpPort.ToString()) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void cbxCtlIp_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || cbxCtlIp.Text == nowData.ctlIp) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void txtCtlPort_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtCtlPort.Text == nowData.ctlPort.ToString()) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void cbxHttpIp_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || cbxHttpIp.Text == nowData.httpIp) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void txtHttpPort_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtHttpPort.Text == nowData.httpPort.ToString()) { return; }
			CmdServ.cfgWaitSave.send();
		}
	}
}
