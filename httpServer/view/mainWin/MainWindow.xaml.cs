using csharpHelp.util;
using csharpHelp.util.action;
using httpServer.view.page;
using httpServer.view.util;
using httpServer.control;
using httpServer.entity;
using httpServer.model;
using httpServer.services;
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
using csharpHelp;

namespace httpServer {
	public class ServerItem : INotifyPropertyChanged {
		int _Tag;
		public int Tag {
			get { return _Tag; }
			set { _Tag = value; FirePropertyChanged("Tag"); }
		}

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
		private static MainWindow ins = null;

		private Entity ent = null;
		//private ServerDataCtl serverDataCtl = null;

		private string title = "";
		private bool isWaitSave = false;
		private string regPath = "HKCU\\Software\\superHttpServer\\";
		private RegistryCtl regCtl = new RegistryCtl();

		private List<ServerItem> serverItem = new List<ServerItem>();

		private string nowType = "";

		Dictionary<int, string> mapIdxToType = new Dictionary<int, string>();

		bool isShowLog = false;
		string strLog = "";
		private int itemTag = 0;

		public MainWindow() {
			InitializeComponent();
			ins = this;

			//
			ent = Entity.getInstance();
			ent.mainMd = new MainMd();
			ent.mainWin = this;

			//ent.mainMd.mapServer["httpServer"] = new HttpServerWin();
			//ent.mainMd.mapServer["httpRevProxy"] = new HttpRevProxy();

			registPage("httpServer", "http服务器", new HttpServerWin());
			registPage("httpRevProxy", "http反向代理", new HttpRevProxy());

			ent.mainMd.configMd.xmlConfig.load(ent.mainMd.configMd, "config.xml");
			loadServerConfig();
			convertOldConfig();

			ent.mainMd.configMd.mdLink.sendTo(ent.mainMd.configMd, this);

			colServer.Width = new GridLength(ent.mainMd.configMd.colServerWidth);

			//serverDataCtl = new ServerDataCtl();
			//serverDataCtl.load();
			//int x = 100;
			//int y = 100;
			//int w = 400;
			//int h = 200;
			//double colServerWidth = 0;
			//try {
			//	x = int.Parse(regCtl.getValue(regPath + "x", "100"));
			//	y = int.Parse(regCtl.getValue(regPath + "y", "100"));
			//	w = int.Parse(regCtl.getValue(regPath + "width", "680"));
			//	h = int.Parse(regCtl.getValue(regPath + "height", "270"));
			//	colServerWidth = int.Parse(regCtl.getValue(regPath + "colServer", "160"));
			//} catch(Exception) {

			//}

			//this.Left = x;
			//this.Top = y;
			//this.Width = w;
			//this.Height = h;
			//colServer.Width = new GridLength(colServerWidth);

			//foreach (var key in ent.mainMd.mapServer.Keys) {
			//	IPage page = ent.mainMd.mapServer[key];
			//	page.init();
			//	grdPage.Children.Add(page as UserControl);
			//	(page as UserControl).Visibility = Visibility.Collapsed;
			//}

			//string[] lstType = {
			//	"httpServer", "http服务器",
			//	"httpRevProxy", "http反向代理"
			//};

			//for (int i = 0; i < lstType.Length ; i += 2) {
			//	cbxType.Items.Add(lstType[i + 1]);
			//	mapIdxToType[i / 2] = lstType[i];
			//}

			cbxType.SelectedIndex = 0;

			//隐藏日志
			if (!isShowLog) {
				grdMainBox.ColumnDefinitions[3].Width = new GridLength(0);
				grdMainBox.ColumnDefinitions[4].Width = new GridLength(0);
			}

			//
			title = Title;
			CmdServ.cfgWaitSave.listen(()=> cfgWaitSaved());
			CmdServ.cfgSaved.listen(() => cfgSaved());

			//
			var lst = ent.mainMd.configMd.lstHttpServer;
			initServerItem(lst);
			//lstItem.SelectedIndex = int.Parse(regCtl.getValue(regPath + "selectItem", "0"));
			lstItem.SelectedIndex = ent.mainMd.configMd.selectItem;
		}

		private void registPage(string type, string desc, IPage page) {
			ent.mainMd.mapServer[type] = page;
			
			page.init();
			grdPage.Children.Add(page as UserControl);
			(page as UserControl).Visibility = Visibility.Collapsed;

			cbxType.Items.Add(desc);
			mapIdxToType[cbxType.Items.Count-1] = type;
		}

		private void loadServerConfig() {
			var lst = ent.mainMd.configMd.lstHttpServer;
			lst.Clear();

			var xmlCtl = ent.mainMd.configMd.xmlConfig.xml;
			var xmlServerBox = xmlCtl.child("httpServer.serverBox");

			var server = new XmlSerialize();

			xmlServerBox.each("server", (idx, ctl) => {
				string type = ctl.attr("type");
				IPage page = getPage(type);
				if(page == null) {
					return;
				}

				var md = page.createModel();
				server.load(md, ctl);
				lst.Add(md);
			});
		}

		private void convertOldConfig() {
			if(File.Exists("config.xml")) {
				return;
			}
			if(!regCtl.exist(regPath)) {
				return;
			}

			var md = Entity.getInstance().mainMd.configMd;
			try {
				md.x = int.Parse(regCtl.getValue(regPath + "x", "100"));
				md.y = int.Parse(regCtl.getValue(regPath + "y", "100"));
				md.width = int.Parse(regCtl.getValue(regPath + "width", "700"));
				md.height = int.Parse(regCtl.getValue(regPath + "height", "300"));
				md.colServerWidth = int.Parse(regCtl.getValue(regPath + "colServer", "160"));
				md.selectItem = int.Parse(regCtl.getValue(regPath + "selectItem", "0"));

				md.lstHttpServer = new List<ServerMd>();

				regCtl.eachItem(regPath, (name) => {
					string subPath = regPath + name + "\\";

					string type = regCtl.getValue(subPath + "type", "httpServer");

					if(type == "httpServer") {
						HttpServerMd tmp = new HttpServerMd();
						tmp.type = regCtl.getValue(subPath + "type");
						tmp.desc = regCtl.getValue(subPath + "desc");
						tmp.isRun = regCtl.getValueBool(subPath + "isRun");

						string rootPath = AppDomain.CurrentDomain.BaseDirectory;

						tmp.ip = regCtl.getValue(subPath + "ip", "127.0.0.1");
						tmp.port = regCtl.getValueInt(subPath + "port", 8091);
						tmp.path = regCtl.getValue(subPath + "path", rootPath);
						tmp.urlParam = regCtl.getValue(subPath + "urlParam", "");
						tmp.rewrite = regCtl.getValue(subPath + "rewrite", "");

						md.lstHttpServer.Add(tmp);
					} else if(type == "httpRevProxy") {
						RevProxyMd tmp = new RevProxyMd();
						tmp.type = regCtl.getValue(subPath + "type");
						tmp.desc = regCtl.getValue(subPath + "desc");
						tmp.isRun = regCtl.getValueBool(subPath + "isRun");

						string rootPath = AppDomain.CurrentDomain.BaseDirectory;

						//tmp.ip = regCtl.getValue(subPath + "ip", "127.0.0.1");
						//tmp.port = regCtl.getValueInt(subPath + "port", 8091);
						//tmp.path = regCtl.getValue(subPath + "path", rootPath);
						//tmp.urlParam = regCtl.getValue(subPath + "urlParam", "");
						//tmp.rewrite = regCtl.getValue(subPath + "rewrite", "");

						tmp.revType = regCtl.getValue(subPath + "revType", "local");

						tmp.localCtlIp = regCtl.getValue(subPath + "localCtlIp", "127.0.0.1");
						tmp.localCtlPort = regCtl.getValueInt(subPath + "localCtlPort", 8091);
						tmp.localHttpIp = regCtl.getValue(subPath + "localHttpIp", "127.0.0.1");
						tmp.localHttpPort = regCtl.getValueInt(subPath + "localHttpPort", 8091);
						tmp.urlParam = regCtl.getValue(subPath + "urlParam", "");

						tmp.ctlIp = regCtl.getValue(subPath + "ctlIp", "127.0.0.1");
						tmp.ctlPort = regCtl.getValueInt(subPath + "ctlPort", 8091);
						tmp.httpIp = regCtl.getValue(subPath + "httpIp", "127.0.0.1");
						tmp.httpPort = regCtl.getValueInt(subPath + "httpPort", 8092);

						md.lstHttpServer.Add(tmp);
					}

				});
			} catch(Exception) {

			}
		}

		private void cfgWaitSaved() {
			if(isWaitSave) {
				return;
			}
			isWaitSave = true;
			Title = title + "*";
		}

		private void cfgSaved() {
			if(!isWaitSave) {
				return;
			}
			isWaitSave = false;
			Title = title;
		}

		private IPage getPage(string type) {
			if(!ent.mainMd.mapServer.ContainsKey(type)) {
				return null;
			}
			return ent.mainMd.mapServer[type];
		}

		private void initServerItem(List<ServerMd> lstServer) {
			for(int i = 0; i < lstServer.Count; ++i) {
				ServerMd md = lstServer[i];

				ServerItem item = new ServerItem() { Tag = ++itemTag, Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() };
				md.serverItem = item;

				//ent.mainMd.mapServer[md.type].initData(md);
				getPage(md.type).initData(md);

				//lstItem.Items.Add(md.desc);
				serverItem.Add(item);
			}

			lstItem.ItemsSource = serverItem;
		}
		
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

		}

		private void Window_Closed(object sender, EventArgs e) {
			clear();

			try {
				ent.mainMd.configMd.selectItem = lstItem.SelectedIndex;
				ent.mainMd.configMd.colServerWidth = (int)colServer.Width.Value;

				ent.mainMd.configMd.mdLink.sendBack();
				ent.mainMd.configMd.xmlConfig.save();
			} catch(Exception) { }

			//regCtl.setValue(regPath + "x", this.Left.ToString());
			//regCtl.setValue(regPath + "y", this.Top.ToString());
			//regCtl.setValue(regPath + "width", this.Width.ToString());
			//regCtl.setValue(regPath + "height", this.Height.ToString());
			//regCtl.setValue(regPath + "selectItem", lstItem.SelectedIndex.ToString());
			//regCtl.setValue(regPath + "colServer", ((int)colServer.Width.Value).ToString());

			//serverDataCtl.save();
		}

		private void clear() {
			//List<ServerMd> server = ent.mainMd.lstServer;
			var lst = ent.mainMd.configMd.lstHttpServer;
			for(int i = 0; i < lst.Count; ++i) {
				//ent.mainMd.mapServer[server[i].type].clear(server[i]);
				//httpServerWin.clear(lst[i]);
				getPage(lst[i].type).clear(lst[i]);
			}
		}

		private void BtnRestart_Click(object sender, RoutedEventArgs e) {
			int idx = lstItem.SelectedIndex;

			//ServerMd md = ent.mainMd.lstServer[idx];
			var md = ent.mainMd.configMd.lstHttpServer[idx];

			//ent.mainMd.mapServer[md.type].start();
			//httpServerWin.start();
			getPage(md.type).start();
			btnRestart.Content = md.isRun ? "重启" : "启动";
			md.serverItem.Source = getServerStatusImgPath(md.isRun);

			cfgSaved();
		}

		public string getServerStatusImgPath(bool isRun) {
			return isRun ? LocalRes.statusRun() : LocalRes.statusStop();
		}
		
		private void lstItem_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			int idx = lstItem.SelectedIndex;
			var lst = ent.mainMd.configMd.lstHttpServer;
			if(idx < 0 || idx >= lst.Count) {
				btnRestart.Content = "启动";
				return;
			}

			//var map = ent.mainMd.mapServer;

			var md = lst[idx];

			//if(!map.ContainsKey(md.type)) {
			//	return;
			//}

			IPage newPage = getPage(md.type);
			if(newPage == null) {
				return;
			}

			IPage oldPage = getPage(nowType);
			if(oldPage != null) {
				(oldPage as UserControl).Visibility = Visibility.Collapsed;
			}
			(newPage as UserControl).Visibility = Visibility.Visible;
			nowType = md.type;

			//update page
			//if(nowType != md.type) {
			//	if(map.ContainsKey(nowType)) {
			//		IPage oldPage = ent.mainMd.mapServer[nowType];
			//		(oldPage as UserControl).Visibility = Visibility.Collapsed;
			//	}

			//	IPage newPage = ent.mainMd.mapServer[md.type];
			//	(newPage as UserControl).Visibility = Visibility.Visible;
			//}

			//nowType = md.type;

			serverItem[idx].Source = getServerStatusImgPath(md.isRun);

			//ent.mainMd.mapServer[md.type].updateData(md);
			//httpServerWin.updateData(md);
			getPage(md.type).updateData(md);

			btnRestart.Content = md.isRun ? "重启" : "启动";

			cfgSaved();
		}

		private void BtnStop_Click(object sender, RoutedEventArgs e) {
			int idx = lstItem.SelectedIndex;

			var lst = ent.mainMd.configMd.lstHttpServer;
			var md = lst[idx];

			//ent.mainMd.mapServer[md.type].stop();
			//httpServerWin.stop();
			getPage(md.type).stop();
			btnRestart.Content = md.isRun ? "重启" : "启动";
			md.serverItem.Source = getServerStatusImgPath(md.isRun);

			cfgSaved();
		}

		private void BtnNew_Click(object sender, RoutedEventArgs e) {
			//List<ServerMd> lstServer = ent.mainMd.lstServer;
			var lst = ent.mainMd.configMd.lstHttpServer;

			string type = mapIdxToType[cbxType.SelectedIndex];

			//var md = httpServerWin.createNewModel();
			var md = getPage(type).createNewModel();
			//ServerMd md = ent.mainMd.mapServer[type].createNewModel();
			md.type = type;

			ServerItem item = new ServerItem() { Tag = ++itemTag, Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() };
			md.serverItem = item;

			//ent.mainMd.mapServer[md.type].initData(md);
			//httpServerWin.initData(md);
			getPage(md.type).initData(md);
			//serverItem.Add(item);
			serverItem.Insert(0, item);

			//lstServer.Add(md);
			lst.Insert(0, md);

			lstItem.ItemsSource = null;
			lstItem.ItemsSource = serverItem;

			//lstItem.SelectedIndex = serverItem.Count - 1;
			//lstItem.ScrollIntoView(lstItem.Items[serverItem.Count - 1]);
			lstItem.SelectedIndex = 0;
			lstItem.ScrollIntoView(lstItem.Items[0]);
		}

		private void BtnDel_Click(object sender, RoutedEventArgs e) {
			int idx = lstItem.SelectedIndex;
			if(idx < 0 || idx >= serverItem.Count) {
				return;
			}

			//List<ServerMd> lstServer = ent.mainMd.lstServer;
			var lst = ent.mainMd.configMd.lstHttpServer;
			var md = lst[idx];
			//ent.mainMd.mapServer[md.type].clear(md);
			//httpServerWin.clear(md);
			getPage(md.type).clear(md);
			//lstServerClt[idx].clear();

			lst.RemoveAt(idx);
			serverItem.RemoveAt(idx);
			//lstServerClt.RemoveAt(idx);

			if(serverItem.Count <= 0) {
				return;
			}

			int newIdx = idx;
			if(newIdx >= serverItem.Count) {
				newIdx = idx - 1;
			}

			lstItem.ItemsSource = null;
			lstItem.ItemsSource = serverItem;
			lstItem.SelectedIndex = newIdx;
		}

		public void log(string info) {
			strLog += info + "\r\n";
			if(strLog.Length > 5000) {
				strLog = strLog.Substring(5000);
			}

			if(!isShowLog) {
				return;
			}
			Dispatcher.Invoke(() => {
				txtLog.Text = strLog;
			});
		}

		private void lstItem_UpClick(object sender, UpDownEvent e) {
			int idx = findItemIdx((int)e.tag);
			if(idx <= 0) {
				return;
			}

			//Debug.WriteLine(idx);
			switchItem(idx - 1, idx);
		}

		private void lstItem_DownClick(object sender, UpDownEvent e) {
			int idx = findItemIdx((int)e.tag);
			if(idx < 0 || idx >= serverItem.Count - 1) {
				return;
			}
			switchItem(idx, idx + 1);
		}

		private int findItemIdx(int tag) {
			for(int i = 0; i < serverItem.Count; ++i) {
				if(serverItem[i].Tag == tag) {
					return i;
				}
			}
			return -1;
		}

		private void switchItem(int idx, int nextIex) {
			int sltIdx = lstItem.SelectedIndex;
			if(sltIdx == idx) {
				sltIdx = nextIex;
			} else if(sltIdx == nextIex) {
				sltIdx = idx;
			}

			//List<ServerMd> lstServer = ent.mainMd.lstServer;
			var lst = ent.mainMd.configMd.lstHttpServer;
			var md = lst[idx];
			lst[idx] = lst[nextIex];
			lst[nextIex] = md;

			ServerItem item = serverItem[idx];
			serverItem[idx] = serverItem[nextIex];
			serverItem[nextIex] = item;
			lstItem.ItemsSource = null;
			lstItem.ItemsSource = serverItem;
			lstItem.SelectedIndex = sltIdx;
		}

		private void BtnLog_Click(object sender, RoutedEventArgs e) {
			//isShowLog = !isShowLog;
			//btnLog.IsSelect = isShowLog;
			//grdMainBox.ColumnDefinitions[3].Width = new GridLength(8);
			//grdMainBox.ColumnDefinitions[4].Width = new GridLength(150);
			//txtLog.Text = strLog;
		}
	}
}
