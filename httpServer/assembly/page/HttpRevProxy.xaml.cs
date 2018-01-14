using com.superscene.util;
using com.superscene.util.action;
using httpServer.assembly.util;
using httpServer.control;
using httpServer.control.httpRevProxy;
using httpServer.entity;
using httpServer.module;
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

		public string serverCtlIp = "127.0.0.1";
		public string serverCtlPort = "8091";
		public string localHttpIp = "127.0.0.1";
		public string localHttpPort = "8091";

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

			serverCtlIp = regCtl.getValue(subPath + "serverCtlIp", "127.0.0.1");
			serverCtlPort = regCtl.getValue(subPath + "serverCtlPort", "8091");
			localHttpIp = regCtl.getValue(subPath + "localHttpIp", "127.0.0.1");
			localHttpPort = regCtl.getValue(subPath + "localHttpPort", "8091");

			ctlIp = regCtl.getValue(subPath + "ctlIp", "127.0.0.1");
			ctlPort = regCtl.getValue(subPath + "ctlPort", "8091");
			httpIp = regCtl.getValue(subPath + "httpIp", "127.0.0.1");
			httpPort = regCtl.getValue(subPath + "httpPort", "8092");
		}

		public override void save(RegistryCtl regCtl, string subPath) {
			base.save(regCtl, subPath);

			regCtl.setValue(subPath + "revType", revType);

			regCtl.setValue(subPath + "serverCtlIp", serverCtlIp);
			regCtl.setValue(subPath + "serverCtlPort", serverCtlPort);
			regCtl.setValue(subPath + "localHttpIp", localHttpIp);
			regCtl.setValue(subPath + "localHttpPort", localHttpPort);

			regCtl.setValue(subPath + "ctlIp", ctlIp);
			regCtl.setValue(subPath + "ctlPort", ctlPort);
			regCtl.setValue(subPath + "httpIp", httpIp);
			regCtl.setValue(subPath + "httpPort", httpPort);
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
			cbxCtlIp.Items.Add("127.0.0.1");
			cbxHttpIp.Items.Add("127.0.0.1");
			for (int i = 0; i < lstIP.Count; ++i) {
				cbxCtlIp.Items.Add(lstIP[i]);
				cbxHttpIp.Items.Add(lstIP[i]);
			}
		}

		public void initData(ServerModule _md) {
			HttpRevModel md = (HttpRevModel)_md;

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

		public ServerModule createNewModel() {
			HttpRevModel md = new HttpRevModel();
			md.ctlPort = SystemCtl.getFreePort(8091).ToString();
			md.httpPort = SystemCtl.getFreePort(8091).ToString();
			md.desc = md.httpIp + ":" + md.httpPort;
			return md;
		}

		public void updateData(ServerModule _md) {
			HttpRevModel md = (HttpRevModel)_md;
			nowData = md;

			txtDesc.Text = md.desc;

			txtServerCtlIp.Text = md.serverCtlIp;
			txtServerCtlPort.Text = md.serverCtlPort;
			txtLocalHttpIp.Text = md.localHttpIp;
			txtLocalHttpPort.Text = md.localHttpPort;

			cbxCtlIp.Text = md.ctlIp;
			txtCtlPort.Text = md.ctlPort;
			cbxHttpIp.Text = md.httpIp;
			txtHttpPort.Text = md.httpPort;

			lblUrl.Content = "http://" + md.httpIp + ":" + md.httpPort;
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
			md.serverCtlIp = txtServerCtlIp.Text;
			md.serverCtlPort = txtServerCtlPort.Text;
			md.localHttpIp = txtLocalHttpIp.Text;
			md.localHttpPort = txtLocalHttpPort.Text;

			md.ctlIp = cbxCtlIp.Text;
			md.ctlPort = txtCtlPort.Text;
			md.httpIp = cbxHttpIp.Text;
			md.httpPort = txtHttpPort.Text;

			lblUrl.Content = "http://" + md.httpIp + ":" + md.httpPort;
			//if(md.desc == "" || ComServerCtl.isDescIp(md.desc)) {
			//	if(md.revType == "local") {
			//		md.desc = md.localIp + ":" + md.localPort;
			//	} else {
			//		md.desc = md.httpIp + ":" + md.httpPort;
			//	}
			//	txtDesc.Text = md.desc;
			//	md.serverItem.Content = md.desc;
			//}
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
					md.desc = md.localHttpIp + ":" + md.localHttpPort;
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
			} else {
				borLocal.Visibility = Visibility.Hidden;
				borServer.Visibility = Visibility.Visible;

				lblUrl.Visibility = Visibility.Visible;
				lblCopyUrl.Visibility = Visibility.Visible;

			}
		}

		private void lblUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			CommonUtil.runExe("explorer.exe", lblUrl.Content.ToString());
		}

		private void lblCopyUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			Clipboard.SetDataObject(lblUrl.Content);
		}
	}
}
