using com.superscene.util;
using com.superscene.util.action;
using httpServer.assembly.util;
using httpServer.control;
using httpServer.control.httpRevProxy;
using httpServer.entity;
using httpServer.module;
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

namespace httpServer.assembly.page {
	class HttpRevModel : ServerModule {
		//public IServerCtl localCtl = new HttpRevClient();
		//public IServerCtl serverCtl = new HttpRevServer();
		public string revType = "local";
		public Dictionary<string, IServerCtl> mapCtl = new Dictionary<string, IServerCtl>();

		public string localCtlIp = "127.0.0.1";
		public string localCtlPort = "8091";
		public string localHttpIp = "127.0.0.1";
		public string localHttpPort = "8091";
		public string urlParam = "";

		public string ctlIp = "127.0.0.1";
		public string ctlPort = "";
		public string httpIp = "127.0.0.1";
		public string httpPort = "";

		public HttpRevModel() {
			mapCtl["local"] = new HttpRevClient(this);
			mapCtl["server"] = new HttpRevServer(this);
		}

		public void clearCtl() {
			foreach (string key in mapCtl.Keys) {
				mapCtl[key].clear();
			}
		}

		public override void load(RegistryCtl regCtl, string subPath) {
			base.load(regCtl, subPath);

			string rootPath = AppDomain.CurrentDomain.BaseDirectory;

			revType = regCtl.getValue(subPath + "revType", "local");

			localCtlIp = regCtl.getValue(subPath + "localCtlIp", "127.0.0.1");
			localCtlPort = regCtl.getValue(subPath + "localCtlPort", "8091");
			localHttpIp = regCtl.getValue(subPath + "localHttpIp", "127.0.0.1");
			localHttpPort = regCtl.getValue(subPath + "localHttpPort", "8091");
			urlParam = regCtl.getValue(subPath + "urlParam", "");

			ctlIp = regCtl.getValue(subPath + "ctlIp", "127.0.0.1");
			ctlPort = regCtl.getValue(subPath + "ctlPort", "8091");
			httpIp = regCtl.getValue(subPath + "httpIp", "127.0.0.1");
			httpPort = regCtl.getValue(subPath + "httpPort", "8092");
		}

		public override void save(RegistryCtl regCtl, string subPath) {
			base.save(regCtl, subPath);

			regCtl.setValue(subPath + "revType", revType);

			regCtl.setValue(subPath + "localCtlIp", localCtlIp);
			regCtl.setValue(subPath + "localCtlPort", localCtlPort);
			regCtl.setValue(subPath + "localHttpIp", localHttpIp);
			regCtl.setValue(subPath + "localHttpPort", localHttpPort);
			regCtl.setValue(subPath + "urlParam", urlParam);

			regCtl.setValue(subPath + "ctlIp", ctlIp);
			regCtl.setValue(subPath + "ctlPort", ctlPort);
			regCtl.setValue(subPath + "httpIp", httpIp);
			regCtl.setValue(subPath + "httpPort", httpPort);
		}

		public override int getMaxPort() {
			try {
				if(revType == "local") {
					return base.getMaxPort();
				}
				int iCtlPort = Int32.Parse(ctlPort);
				int iHttpPort = Int32.Parse(httpPort);
				int maxPort = Entity.getInstance().mainModule.maxPort;

				int port = Math.Max(iCtlPort, iHttpPort);
				if(port > maxPort) {
					return Math.Min(iCtlPort, iHttpPort);
				} else {
					return port;
				}
			} catch(Exception) {
				return base.getMaxPort();
			}
		}
	};

	/// <summary>
	/// HttpRevProxy.xaml 的交互逻辑
	/// Http反向代理
	/// </summary>
	public partial class HttpRevProxy : UserControl, IPage {
		Entity ent = null;
		private HttpRevModel nowData = null;
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
			for (int i = 0; i < lstIP.Count; ++i) {
				cbxCtlIp.Items.Add(lstIP[i]);
				cbxHttpIp.Items.Add(lstIP[i]);
			}
		}

		public ServerModule createNewModel() {
			HttpRevModel md = new HttpRevModel();
			int port = ent.mainModule.maxStartPort();
			md.ctlPort = SystemCtl.getFreePort(port).ToString();
			md.httpPort = SystemCtl.getFreePort(port + 1).ToString();

			//ent.mainModule.updateStartPort(md.ctlPort);
			//ent.mainModule.updateStartPort(md.httpPort);

			md.desc = md.ctlIp + ":" + md.ctlPort;
			return md;
		}

		public void initData(ServerModule _md) {
			HttpRevModel md = (HttpRevModel)_md;
			//ent.mainModule.updateStartPort(md.ctlPort);
			//ent.mainModule.updateStartPort(md.httpPort);

			//if(md.revType == "local") {
			//	md.serverCtl = new HttpRevClient();
			//} else {
			//	md.serverCtl = new HttpRevServer();
			//}

			if(md.isRun) {
				//md.clearCtl();
				if (md.mapCtl.ContainsKey(md.revType)){
					md.mapCtl[md.revType].restartServer();
				}
			}
		}

		public ServerModule createModel() {
			return new HttpRevModel();
		}

		public void updateData(ServerModule _md) {
			HttpRevModel md = (HttpRevModel)_md;
			nowData = md;

			txtDesc.Text = md.desc;

			txtLocalCtlIp.Text = md.localCtlIp;
			txtLocalCtlPort.Text = md.localCtlPort;
			txtLocalHttpIp.Text = md.localHttpIp;
			txtLocalHttpPort.Text = md.localHttpPort;
			txtUrlParam.Text = md.urlParam;

			cbxCtlIp.Text = md.ctlIp;
			txtCtlPort.Text = md.ctlPort;
			cbxHttpIp.Text = md.httpIp;
			txtHttpPort.Text = md.httpPort;

			lblUrl.Content = "http://" + md.httpIp + ":" + md.httpPort + "/" + md.urlParam;
			updateRevType(md.revType);
		}

		public void start() {
			startServer(true);
		}

		public void stop() {
			startServer(false);
		}

		private void startServer(bool isStart) {
			HttpRevModel md = nowData;
			md.isRun = isStart;

			md.revType = borLocal.Visibility == Visibility.Visible ? "local" : "server";
			md.localCtlIp = txtLocalCtlIp.Text;
			md.localCtlPort = txtLocalCtlPort.Text;
			md.localHttpIp = txtLocalHttpIp.Text;
			md.localHttpPort = txtLocalHttpPort.Text;
			md.urlParam = txtUrlParam.Text;

			md.ctlIp = cbxCtlIp.Text;
			md.ctlPort = txtCtlPort.Text;
			md.httpIp = cbxHttpIp.Text;
			md.httpPort = txtHttpPort.Text;

			lblUrl.Content = "http://" + md.httpIp + ":" + md.httpPort + "/" + md.urlParam;
			updateDesc();

			md.clearCtl();
			if (isStart) {
				if (md.mapCtl.ContainsKey(md.revType)) {
					md.mapCtl[md.revType].restartServer();
				}
			}
		}

		public void clear(ServerModule _md) {
			HttpRevModel md = (HttpRevModel)_md;
			md.clearCtl();
		}

		private void updateDesc() {
			HttpRevModel md = nowData;

			if (md.desc == "" || ComServerCtl.isDescIp(md.desc)) {
				if (md.revType == "local") {
					md.desc = md.localCtlIp + ":" + md.localCtlPort;
				} else {
					md.desc = md.httpIp + ":" + md.httpPort;
				}
				txtDesc.Text = md.desc;
				md.serverItem.Content = md.desc;
			}
		}

		private void txtDesc_TextChanged(object sender, TextChangedEventArgs e) {
			if (nowData == null) { return; }

			HttpRevModel md = nowData;
			md.desc = txtDesc.Text;
			md.serverItem.Content = txtDesc.Text;
		}

		private void lblLocal_MouseEnter(object sender, MouseEventArgs e) {
			if (nowData == null) { return; }
			borLocal.Visibility = Visibility.Visible;
		}

		private void lblLocal_MouseLeave(object sender, MouseEventArgs e) {
			if (nowData == null) { return; }
			borLocal.Visibility = nowData.revType == "local" ? Visibility.Visible : Visibility.Hidden;
		}

		private void lblServer_MouseEnter(object sender, MouseEventArgs e) {
			if (nowData == null) { return; }
			borServer.Visibility = Visibility.Visible;
		}

		private void lblServer_MouseLeave(object sender, MouseEventArgs e) {
			if (nowData == null) { return; }
			borServer.Visibility = nowData.revType == "server" ? Visibility.Visible : Visibility.Hidden;
		}

		private void lblLocal_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if (nowData == null) { return; }
			updateRevType("local");
			nowData.revType = "local";
			updateDesc();
		}

		private void lblServer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if (nowData == null) { return; }
			updateRevType("server");
			nowData.revType = "server";
			updateDesc();
		}

		private void updateRevType(string revType) {
			if (revType == "local") {
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
			CommonUtil.runExe("explorer.exe", lblUrl.Content.ToString());
		}

		private void lblCopyUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			Clipboard.SetDataObject(lblUrl.Content);
		}

		private void txtUrlParam_TextChanged(object sender, TextChangedEventArgs e) {
			if (nowData == null) { return; }
			nowData.urlParam = txtUrlParam.Text;
			lblUrl.Content = "http://" + nowData.httpIp + ":" + nowData.httpPort + "/" + nowData.urlParam;
		}

		private void txtLocalCtlIp_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtLocalCtlIp.Text == nowData.localCtlIp) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void txtLocalCtlPort_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtLocalCtlPort.Text == nowData.localCtlPort) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void txtLocalHttpIp_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtLocalHttpIp.Text == nowData.localHttpIp) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void txtLocalHttpPort_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtLocalHttpPort.Text == nowData.localHttpPort) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void cbxCtlIp_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || cbxCtlIp.Text == nowData.ctlIp) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void txtCtlPort_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtCtlPort.Text == nowData.ctlPort) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void cbxHttpIp_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || cbxHttpIp.Text == nowData.httpIp) { return; }
			CmdServ.cfgWaitSave.send();
		}

		private void txtHttpPort_TextChanged(object sender, TextChangedEventArgs e) {
			if(nowData == null || txtHttpPort.Text == nowData.httpPort) { return; }
			CmdServ.cfgWaitSave.send();
		}
	}
}
