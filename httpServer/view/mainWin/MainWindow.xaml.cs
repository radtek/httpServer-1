using csharpHelp.util;
using csharpHelp.util.action;
using httpServer.view.page;
using httpServer.view.util;
using httpServer.control;
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
using httpServer.dll;
using System.Runtime.InteropServices;

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

		public void onMouseDown(object sender, MouseButtonEventArgs e) {
			Debug.WriteLine("down:" + _Tag);
		}
	}

	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		public static MainWindow ins = null;

		private string title = "";
		private bool isWaitSave = false;
		private string regPath = "HKCU\\Software\\superHttpServer\\";
		private RegistryCtl regCtl = new RegistryCtl();

		private List<ServerItem> serverItem = new List<ServerItem>();

		public bool isShowLog = false;
		public bool isShowLogError = true;
		public bool isShowLogDebug = false;
		string strLog = "";
		private int itemTag = 0;

		private bool isWatiSave = false;
		private SetTimeout timeout = new SetTimeout();

		public MainWindow() {
			InitializeComponent();
			ins = this;
			isWatiSave = true;

			//set icon
			Uri iconUri = new Uri(LocalRes.icon16(), UriKind.RelativeOrAbsolute);
			Icon = BitmapFrame.Create(iconUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
			
			//load config
			MainMd.ins.xmlConfig.load(MainMd.ins.configMd, "config.xml");
			loadServerConfig();
			convertOldConfig();

			MainMd.ins.mdLink.sendTo(MainMd.ins.configMd, this);
			colServer.Width = new GridLength(MainMd.ins.configMd.colServerWidth);
			
			//set language
			new ClassLink().sendTo(Lang.ins, this, "", LinkType.All);

			MainMd.ins.configMd.timeout = Math.Max(500, MainMd.ins.configMd.timeout);
			//MainMd.ins.configMd.mapContextType = ServerCtl.mapSuffix;
			//ServerCtl.setContentType(MainMd.ins.configMd.mapContextType);
			//ServerCtl.timeout = Math.Max(500, MainMd.ins.configMd.timeout);

			//http server init
			HttpServerGo.Init();
			
			HttpServerGo.HttpGlobalInfo globalInfo = new HttpServerGo.HttpGlobalInfo();
			globalInfo.HttpsCrt = getAsmbText("server.crt");
			globalInfo.HttpsKey = getAsmbText("server.key");
			globalInfo.MapContextType = contextTypeToSting();
			globalInfo.Timeout = MainMd.ins.configMd.timeout;
			HttpServerGo.SetHttpGlobalInfo(ref globalInfo);

			getPage().init();
			
			showLog(isShowLog);
			btnLogError.IsSelect = isShowLogError;
			btnLogDebug.IsSelect = isShowLogDebug;

			//
			title = Title;
			CmdServ.cfgWaitSave.listen(()=> cfgWaitSaved());
			CmdServ.cfgSaved.listen(() => cfgSaved());

			//
			var lst = MainMd.ins.configMd.lstHttpServer;
			initServerItem(lst);
			//lstItem.SelectedIndex = int.Parse(regCtl.getValue(regPath + "selectItem", "0"));
			lstItem.SelectedIndex = MainMd.ins.configMd.selectItem;
			
			isWatiSave = false;

			//try {
			//	HttpServerGo.HttpParamMd md = new HttpServerGo.HttpParamMd();
			//	md.Ip = MainCtl.createStrPtr("192.168.0.139");
			//	md.Port = MainCtl.createStrPtr("");
			//	md.IsHttps = true;
			//	md.Path = MainCtl.createStrPtr("");
			//	md.VirtualPath = MainCtl.createStrPtr("");
			//	md.Proxy = MainCtl.createStrPtr("");
			//	long id = HttpServerGo.Aaa(ref md);
			//	Debug.WriteLine("aaa:" + id);
			//} catch(Exception ex) { Debug.WriteLine(ex.ToString()); }
		}

		IntPtr contextTypeToSting() {
			string rst = "";
			var map = MainMd.ins.configMd.mapContextType;
			foreach(string key in map.Keys) {
				string val = map[key];
				if(key == "" || val == "") {
					continue;
				}
				rst += $"{key}!{val}!!";
			}
			return MainCtl.createStrPtr(rst);
		}

		IntPtr getAsmbText(string fileName) {
			System.Reflection.Assembly Asmb = System.Reflection.Assembly.GetExecutingAssembly();
			string strName = Asmb.GetName().Name + ".resource.data." + fileName.Replace("/", ".");

			try {
				using(Stream ManifestStream = Asmb.GetManifestResourceStream(strName)) {
					if(ManifestStream != null) {
						//Debug.WriteLine($"res:{strName}");
						byte[] StreamData = new byte[ManifestStream.Length];
						ManifestStream.Read(StreamData, 0, (int)ManifestStream.Length);

						return Marshal.UnsafeAddrOfPinnedArrayElement(StreamData, 0);
					}
				}
			} catch(Exception ex) { Debug.WriteLine(ex.ToString()); }

			return IntPtr.Zero;
		}

		private void loadServerConfig() {
			var lst = MainMd.ins.configMd.lstHttpServer;
			lst.Clear();

			var xmlCtl = MainMd.ins.xmlConfig.xml;
			var xmlServerBox = xmlCtl.child("httpServer.serverBox");

			var server = new XmlSerialize();

			xmlServerBox.each("server", (idx, ctl) => {
				string type = ctl.attr("type");
				if(type != "" && type != "httpServer") {
					return;
				}

				var md = getPage().createModel();
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

			var md = MainMd.ins.configMd;
			try {
				md.x = int.Parse(regCtl.getValue(regPath + "x", "100"));
				md.y = int.Parse(regCtl.getValue(regPath + "y", "100"));
				md.width = int.Parse(regCtl.getValue(regPath + "width", "700"));
				md.height = int.Parse(regCtl.getValue(regPath + "height", "300"));
				md.colServerWidth = int.Parse(regCtl.getValue(regPath + "colServer", "160"));
				md.selectItem = int.Parse(regCtl.getValue(regPath + "selectItem", "0"));

				md.lstHttpServer = new List<HttpServerMd>();

				regCtl.eachItem(regPath, (name) => {
					string subPath = regPath + name + "\\";

					string type = regCtl.getValue(subPath + "type", "httpServer");
					if(type != "" && type != "httpServer") {
						return;
					}
					
					HttpServerMd tmp = new HttpServerMd();
					//tmp.type = regCtl.getValue(subPath + "type");
					tmp.desc = regCtl.getValue(subPath + "desc");
					tmp.isRun = regCtl.getValueBool(subPath + "isRun");
					tmp.isHttps = regCtl.getValueBool(subPath + "isHttps");

					string rootPath = AppDomain.CurrentDomain.BaseDirectory;

					tmp.ip = regCtl.getValue(subPath + "ip", "127.0.0.1");
					tmp.port = regCtl.getValueInt(subPath + "port", 8091);
					tmp.path = regCtl.getValue(subPath + "path", rootPath);
					tmp.urlParam = regCtl.getValue(subPath + "urlParam", "");
					tmp.rewrite = regCtl.getValue(subPath + "rewrite", "");

					md.lstHttpServer.Add(tmp);
				});

				regCtl.remove(regPath);
			} catch(Exception ex) {
				error(ex);
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

		public void delaySaveConfig() {
			if(isWatiSave || timeout == null) {
				return;
			}

			isWatiSave = true;
			//Debug.WriteLine("save");

			timeout.wait(new Action(() => {
				try {
					Dispatcher.Invoke(() => {
						isWatiSave = false;
						try {
							MainMd.ins.xmlConfig.save();
						} catch(Exception) { }
					});
				} catch(Exception) { }
			}), 2000);
		}

		private HttpServerWin getPage() {
			return httpServerWin;
		}

		private void initServerItem(List<HttpServerMd> lstServer) {
			for(int i = 0; i < lstServer.Count; ++i) {
				HttpServerMd md = lstServer[i];

				ServerItem item = new ServerItem() { Tag = ++itemTag, Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() };
				md.serverItem = item;

				getPage().initData(md);
				
				serverItem.Add(item);
			}

			lstItem.ItemsSource = serverItem;
		}
		
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

		}

		private void Window_Closed(object sender, EventArgs e) {
			clear();

			try {
				MainMd.ins.configMd.selectItem = lstItem.SelectedIndex;
				MainMd.ins.configMd.colServerWidth = (int)colServer.Width.Value;

				MainMd.ins.mdLink.sendBack();
				MainMd.ins.xmlConfig.save();
			} catch(Exception ex) { error(ex); }
			
		}

		private void clear() {
			timeout.clear();
			timeout = null;

			var lst = MainMd.ins.configMd.lstHttpServer;
			for(int i = 0; i < lst.Count; ++i) {
				getPage().clear(lst[i]);
			}
		}

		private void BtnRestart_Click(object sender, RoutedEventArgs e) {
			int idx = lstItem.SelectedIndex;
			
			var md = MainMd.ins.configMd.lstHttpServer[idx];

			getPage().start();
			btnRestart.Content = md.isRun ? Lang.ins.restart : Lang.ins.start;
			md.serverItem.Source = getServerStatusImgPath(md.isRun);

			cfgSaved();
		}

		public string getServerStatusImgPath(bool isRun) {
			return isRun ? LocalRes.statusRun() : LocalRes.statusStop();
		}
		
		private void lstItem_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			int idx = lstItem.SelectedIndex;
			var lst = MainMd.ins.configMd.lstHttpServer;
			if(idx < 0 || idx >= lst.Count) {
				btnRestart.Content = Lang.ins.start;
				return;
			}

			var md = lst[idx];

			serverItem[idx].Source = getServerStatusImgPath(md.isRun);

			getPage().updateData(md);

			btnRestart.Content = md.isRun ? Lang.ins.restart : Lang.ins.start;

			cfgSaved();
		}

		private void BtnStop_Click(object sender, RoutedEventArgs e) {
			int idx = lstItem.SelectedIndex;

			var lst = MainMd.ins.configMd.lstHttpServer;
			var md = lst[idx];
			
			getPage().stop();
			btnRestart.Content = md.isRun ? Lang.ins.restart : Lang.ins.start;
			md.serverItem.Source = getServerStatusImgPath(md.isRun);

			cfgSaved();
		}

		private void BtnNew_Click(object sender, RoutedEventArgs e) {
			//List<ServerMd> lstServer = ent.mainMd.lstServer;
			var lst = MainMd.ins.configMd.lstHttpServer;
			
			var md = getPage().createNewModel();

			ServerItem item = new ServerItem() { Tag = ++itemTag, Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() };
			md.serverItem = item;
			
			getPage().initData(md);
			serverItem.Insert(0, item);

			//lstServer.Add(md);
			lst.Insert(0, md);

			lstItem.ItemsSource = null;
			lstItem.ItemsSource = serverItem;
			
			lstItem.SelectedIndex = 0;
			lstItem.ScrollIntoView(lstItem.Items[0]);
		}

		private void BtnDel_Click(object sender, RoutedEventArgs e) {
			int idx = lstItem.SelectedIndex;
			if(idx < 0 || idx >= serverItem.Count) {
				return;
			}
			
			var lst = MainMd.ins.configMd.lstHttpServer;
			var md = lst[idx];
			getPage().clear(md);

			lst.RemoveAt(idx);
			serverItem.RemoveAt(idx);

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

		public void log(Exception ex) {
			if(!isShowLog) {
				return;
			}

			log(ex.ToString());
		}

		public void log(string info) {
			if(!isShowLog) {
				return;
			}

			strLog += info + "\r\n";
			if(strLog.Length > 5000) {
				strLog = strLog.Substring(5000);
			}
			
			try {
				Dispatcher.Invoke(() => {
					try {
						txtLog.Text = strLog;
					} catch(Exception) { }
				});
			} catch(Exception) { }
		}

		public void error(Exception ex) {
			if(isShowLog && isShowLogError) {
				log(ex.ToString());
			}
		}

		public void error(string info) {
			if(isShowLog && isShowLogError) {
				log(info);
			}
		}

		public void debug(Exception ex) {
			if(isShowLog && isShowLogDebug) {
				log(ex.ToString());
			}
		}

		public void debug(string info) {
			if(isShowLog && isShowLogDebug) {
				log(info);
			}
		}

		private void lstItem_UpClick(object sender, UpDownEvent e) {
			int idx = findItemIdx((int)e.tag);
			if(idx <= 0) {
				return;
			}
			
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
			
			var lst = MainMd.ins.configMd.lstHttpServer;
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

		private void showLog(bool isShow) {
			isShowLog = isShow;
			btnLog.IsSelect = isShowLog;
			if(isShowLog) {
				grdMainBox.ColumnDefinitions[3].Width = new GridLength(8);
				grdMainBox.ColumnDefinitions[4].Width = new GridLength(150);
			} else {
				grdMainBox.ColumnDefinitions[3].Width = new GridLength(0);
				grdMainBox.ColumnDefinitions[4].Width = new GridLength(0);
			}
		}

		private void BtnLog_Click(object sender, RoutedEventArgs e) {
			showLog(!isShowLog);
		}

		private void BtnLogError_Click(object sender, RoutedEventArgs e) {
			isShowLogError = !isShowLogError;
			btnLogError.IsSelect = isShowLogError;
		}

		private void BtnLogDebug_Click(object sender, RoutedEventArgs e) {
			isShowLogDebug = !isShowLogDebug;
			btnLogDebug.IsSelect = isShowLogDebug;
		}

		private void BtnLogClear_Click(object sender, RoutedEventArgs e) {
			strLog = "";
			txtLog.Text = strLog;
		}
	}
}
