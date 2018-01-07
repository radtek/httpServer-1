using com.superscene.util;
using com.superscene.util.action;
using httpServer.assembly.util;
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
		public string revType = "local";

		public string serverIp = "";
		public string serverPort = "";
		public string localIp = "";
		public string localPort = "";

		public string ctlIp = "127.0.0.1";
		public string ctlPort = "";
		public string httpIp = "127.0.0.1";
		public string httpPort = "";

		public override void load(RegistryCtl regCtl, string subPath) {
			base.load(regCtl, subPath);

			string rootPath = AppDomain.CurrentDomain.BaseDirectory;

			revType = regCtl.getValue(subPath + "revType", "local");

			serverIp = regCtl.getValue(subPath + "serverIp", "");
			serverPort = regCtl.getValue(subPath + "serverPort", "");
			localIp = regCtl.getValue(subPath + "localIp", "");
			localPort = regCtl.getValue(subPath + "localPort", "");

			ctlIp = regCtl.getValue(subPath + "ctlIp", "127.0.0.1");
			ctlPort = regCtl.getValue(subPath + "ctlPort", "8091");
			httpIp = regCtl.getValue(subPath + "httpIp", "127.0.0.1");
			httpPort = regCtl.getValue(subPath + "httpPort", "8092");
		}

		public override void save(RegistryCtl regCtl, string subPath) {
			base.save(regCtl, subPath);

			regCtl.setValue(subPath + "revType", revType);

			regCtl.setValue(subPath + "serverIp", serverIp);
			regCtl.setValue(subPath + "serverPort", serverPort);
			regCtl.setValue(subPath + "localIp", localIp);
			regCtl.setValue(subPath + "localPort", localPort);

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

		public void initData(ServerModule md) {

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

			txtServerIp.Text = md.serverIp;
			txtServerPort.Text = md.serverPort;
			txtLocalIp.Text = md.localIp;
			txtLocalPort.Text = md.localPort;

			cbxCtlIp.Text = md.ctlIp;
			txtCtlPort.Text = md.ctlPort;
			cbxHttpIp.Text = md.httpIp;
			txtHttpPort.Text = md.httpPort;

			updateRevType(md.revType);
		}

		public void start() {

		}

		public void stop() {

		}

		public void clear(ServerModule md) {

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
		}

		private void lblServer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if (nowData == null) { return; }
			updateRevType("server");
			nowData.revType = "server";
		}

		private void updateRevType(string type) {
			if (type == "local") {
				borLocal.Visibility = Visibility.Visible;
				borServer.Visibility = Visibility.Hidden;
			} else {
				borLocal.Visibility = Visibility.Hidden;
				borServer.Visibility = Visibility.Visible;
			}
		}

	}
}
