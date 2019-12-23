﻿using csharpHelp.util;
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
		public static MainWindow ins = null;

		//private Entity ent = null;
		//private ServerDataCtl serverDataCtl = null;

		private string title = "";
		private bool isWaitSave = false;
		private string regPath = "HKCU\\Software\\superHttpServer\\";
		private RegistryCtl regCtl = new RegistryCtl();

		private List<ServerItem> serverItem = new List<ServerItem>();

		//private string nowType = "";

		//Dictionary<int, string> mapIdxToType = new Dictionary<int, string>();

		public bool isShowLog = false;
		string strLog = "";
		private int itemTag = 0;

		public MainWindow() {
			InitializeComponent();
			ins = this;

			//
			//ent = Entity.ins;
			//ent.mainMd = new MainMd();
			//ent.mainWin = this;

			MainMd.ins.xmlConfig.load(MainMd.ins.configMd, "config.xml");
			loadServerConfig();
			convertOldConfig();

			MainMd.ins.mdLink.sendTo(MainMd.ins.configMd, this);

			colServer.Width = new GridLength(MainMd.ins.configMd.colServerWidth);
			
			//set language
			new ClassLink().sendTo(Lang.ins, this, "", LinkType.All);

			ServerCtl.setContentType(MainMd.ins.configMd.mapContextType);
			ServerCtl.timeout = Math.Max(500, MainMd.ins.configMd.timeout);
			MainMd.ins.configMd.timeout = ServerCtl.timeout;
			MainMd.ins.configMd.mapContextType = ServerCtl.mapSuffix;

			getPage().init();
			
			showLog(isShowLog);

			//
			title = Title;
			CmdServ.cfgWaitSave.listen(()=> cfgWaitSaved());
			CmdServ.cfgSaved.listen(() => cfgSaved());

			//
			var lst = MainMd.ins.configMd.lstHttpServer;
			initServerItem(lst);
			//lstItem.SelectedIndex = int.Parse(regCtl.getValue(regPath + "selectItem", "0"));
			lstItem.SelectedIndex = MainMd.ins.configMd.selectItem;
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

					string rootPath = AppDomain.CurrentDomain.BaseDirectory;

					tmp.ip = regCtl.getValue(subPath + "ip", "127.0.0.1");
					tmp.port = regCtl.getValueInt(subPath + "port", 8091);
					tmp.path = regCtl.getValue(subPath + "path", rootPath);
					tmp.urlParam = regCtl.getValue(subPath + "urlParam", "");
					tmp.rewrite = regCtl.getValue(subPath + "rewrite", "");

					md.lstHttpServer.Add(tmp);

				});
			} catch(Exception ex) {
				log(ex);
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

		private HttpServerWin getPage() {
			//if(!ent.mainMd.mapServer.ContainsKey(type)) {
			//	return null;
			//}
			//return ent.mainMd.mapServer[type];
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
			} catch(Exception ex) { log(ex); }
			
		}

		private void clear() {
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

			//IPage newPage = getPage(md.type);
			//if(newPage == null) {
			//	return;
			//}

			//IPage oldPage = getPage(nowType);
			//if(oldPage != null) {
			//	(oldPage as UserControl).Visibility = Visibility.Collapsed;
			//}
			//(newPage as UserControl).Visibility = Visibility.Visible;
			//nowType = md.type;

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

			//if(!isShowLog) {
			//	return;
			//}
			try {
				Dispatcher.Invoke(() => {
					try {
						txtLog.Text = strLog;
					} catch(Exception) { }
				});
			} catch(Exception) { }
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
			//txtLog.Text = strLog;
		}
		
	}
}
