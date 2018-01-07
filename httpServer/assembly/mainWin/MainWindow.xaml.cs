using com.superscene.util;
using com.superscene.util.action;
using httpServer.assembly.page;
using httpServer.control;
using httpServer.entity;
using httpServer.module;
using httpServer.util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
	public class ServerItem : INotifyPropertyChanged {
		string _Content;
		public string Content {
			get { return _Content; }
			set { _Content = value; FirePropertyChanged("Content"); }
		}

		string _Source;
		public string Source {
			get { return _Source; }
			set { _Source = value; FirePropertyChanged("Source"); }
		}

		public virtual event PropertyChangedEventHandler PropertyChanged;
		public virtual void FirePropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	};

	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {

		static MainWindow ins = null;
		//string rootPath = "";

		Entity ent = null;
		ServerDataCtl serverDataCtl = null;

		string regPath = "HKCU\\Software\\superHttpServer\\";
		RegistryCtl regCtl = new RegistryCtl();
		//string lastTransmit = "";

		private ObservableCollection<ServerItem> serverItem = new ObservableCollection<ServerItem>();
		//private List<ServerCtl> lstServerClt = new List<ServerCtl>();
		//HttpCtl httpCtl = new HttpCtl();

		//private int nowIdx = -1;
		private string nowType = "";

		Dictionary<int, string> mapIdxToType = new Dictionary<int, string>();

		public MainWindow() {
			InitializeComponent();
			ins = this;

			//
			ent = Entity.getInstance();
			ent.mainModule = new MainModule();
			ent.mainWin = this;

			ent.mainModule.mapServer["httpServer"] = new HttpServerWin();
			ent.mainModule.mapServer["httpRevProxy"] = new HttpRevProxy();

			serverDataCtl = new ServerDataCtl();
			serverDataCtl.load();
			int x = 100;
			int y = 100;
			int w = 400;
			int h = 200;
			try {
				x = Int32.Parse(regCtl.getValue(regPath + "x", "100"));
				y = Int32.Parse(regCtl.getValue(regPath + "y", "100"));
				w = Int32.Parse(regCtl.getValue(regPath + "width", "680"));
				h = Int32.Parse(regCtl.getValue(regPath + "height", "270"));
			} catch(Exception) {

			}

			this.Left = x;
			this.Top = y;
			this.Width = w;
			this.Height = h;

			foreach (var key in ent.mainModule.mapServer.Keys) {
				IPage page = ent.mainModule.mapServer[key];
				page.init();
				grdPage.Children.Add(page as UserControl);
				(page as UserControl).Visibility = Visibility.Collapsed;
			}

			string[] lstType = {
				"httpServer", "http服务器",
				"httpRevProxy", "http反向代理"
			};

			for (int i = 0; i < lstType.Length ; i += 2) {
				cbxType.Items.Add(lstType[i + 1]);
				mapIdxToType[i / 2] = lstType[i];
			}

			cbxType.SelectedIndex = 0;

			//
			initServerItem(ent.mainModule.lstServer);
			lstItem.SelectedIndex = 0;
		}

		private void initServerItem(List<ServerModule> lstServer) {
			for(int i = 0; i < lstServer.Count; ++i) {
				ServerModule md = lstServer[i];

				//if(!ent.mainModule.mapServer.ContainsKey(md.type)) {
				//	continue;
				//}

				ServerItem item = new ServerItem() { Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() };
				md.serverItem = item;

				ent.mainModule.mapServer[md.type].initData(md);

				//lstItem.Items.Add(md.desc);
				serverItem.Add(item);

				//ServerCtl ctl = new ServerCtl();
				//ctl.md = md;
				//lstServerClt.Add(ctl);
				//if(md.isRun) {
				//	ctl.restartServer();
				//}
			}

			lstItem.ItemsSource = serverItem;
			//lstItem.SelectedIndex = 0;
		}
		
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

		}

		private void Window_Closed(object sender, EventArgs e) {
			clear();

			regCtl.setValue(regPath + "x", this.Left.ToString());
			regCtl.setValue(regPath + "y", this.Top.ToString());
			regCtl.setValue(regPath + "width", this.Width.ToString());
			regCtl.setValue(regPath + "height", this.Height.ToString());

			serverDataCtl.save();
		}

		private void clear() {
			List<ServerModule> server = ent.mainModule.lstServer;
			for(int i = 0; i < server.Count; ++i) {
				ent.mainModule.mapServer[server[i].type].clear(server[i]);
			}

			//for(int i = 0; i < lstServerClt.Count; ++i) {
			//	lstServerClt[i].clear();
			//}
		}

		private void btnRestart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			int idx = lstItem.SelectedIndex;

			ServerModule md = ent.mainModule.lstServer[idx];

			ent.mainModule.mapServer[md.type].start();
			btnRestart.Content = md.isRun ? "重启" : "启动";
			md.serverItem.Source = getServerStatusImgPath(md.isRun);
		}

		public string getServerStatusImgPath(bool isRun) {
			return isRun ? LocalRes.statusRun() : LocalRes.statusStop();
		}
		
		private void lstItem_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			int idx = lstItem.SelectedIndex;
			if(idx < 0 || idx >= ent.mainModule.lstServer.Count) {
				//txtDesc.Text = "";
				//cbxIp.Text = "";
				//txtPort.Text = "";
				//txtPath.Text = SysConst.rootPath();

				//txtProxy.IsChecked = false;
				//txtProxy.Text = "";
				//txtTransmit.IsChecked = false;
				//txtTransmit.Text = "";
				btnRestart.Content = "启动";
				//lblUrl.Content = "127.0.0.1:8091";
				return;
			}

			var map = ent.mainModule.mapServer;

			ServerModule md = ent.mainModule.lstServer[idx];

			if(!map.ContainsKey(md.type)){
				return;
			}

			//update page
			if (nowType != md.type) {
				if (map.ContainsKey(nowType)) {
					IPage oldPage = ent.mainModule.mapServer[nowType];
					(oldPage as UserControl).Visibility = Visibility.Collapsed;
				}
				
				IPage newPage = ent.mainModule.mapServer[md.type];
				(newPage as UserControl).Visibility = Visibility.Visible;
			}

			nowType = md.type;

			//txtDesc.Text = md.desc;
			serverItem[idx].Source = getServerStatusImgPath(md.isRun);

			ent.mainModule.mapServer[md.type].updateData(md);

			//cbxIp.Text = md.ip;
			//txtPort.Text = md.port;

			//txtPath.Text = md.path;

			//txtProxy.IsChecked = md.isProxy;
			//txtProxy.Text = md.proxyUrl;

			//txtTransmit.IsChecked = md.isTransmit;
			//txtTransmit.Text = md.transmitUrl;

			btnRestart.Content = md.isRun ? "重启" : "启动";

			//serverPath = md.path + "/";
			//lblUrl.Content = "http://" + md.ip + ":" + md.port;
		}

		private void switchPage(string type) {

		}

		private void btnStop_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			int idx = lstItem.SelectedIndex;


			ServerModule md = ent.mainModule.lstServer[idx];

			ent.mainModule.mapServer[md.type].stop();
			btnRestart.Content = md.isRun ? "重启" : "启动";
			md.serverItem.Source = getServerStatusImgPath(md.isRun);
		}

		private void btnNew_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			List<ServerModule> lstServer = ent.mainModule.lstServer;

			string type = mapIdxToType[cbxType.SelectedIndex];

			ServerModule md = ent.mainModule.mapServer[type].createNewModel();
			md.type = type;

			ServerItem item = new ServerItem() { Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() };
			md.serverItem = item;

			ent.mainModule.mapServer[md.type].initData(md);
			serverItem.Add(item);

			//ServerModule md = new ServerModule();
			//md.port = SystemCtl.getFreePort(8091).ToString();
			//md.desc = md.ip + ":" + md.port;
			//md.path = SysConst.rootPath();
			lstServer.Add(md);

			//serverItem.Add(new ServerItem() { Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() });

			//ServerCtl ctl = new ServerCtl();
			//ctl.md = md;
			//lstServerClt.Add(ctl);

			lstItem.SelectedIndex = serverItem.Count - 1;

			lstItem.ScrollIntoView(lstItem.Items[serverItem.Count - 1]);
		}

		private void btnDel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			int idx = lstItem.SelectedIndex;
			if(idx < 0 || idx >= serverItem.Count) {
				return;
			}

			List<ServerModule> lstServer = ent.mainModule.lstServer;
			ServerModule md = lstServer[idx];
			ent.mainModule.mapServer[md.type].clear(md);
			//lstServerClt[idx].clear();

			lstServer.RemoveAt(idx);
			serverItem.RemoveAt(idx);
			//lstServerClt.RemoveAt(idx);

			if(serverItem.Count <= 0) {
				return;
			}

			int newIdx = idx;
			if(newIdx >= serverItem.Count) {
				newIdx = idx - 1;
			}
			lstItem.SelectedIndex = newIdx;
		}

	}
}
