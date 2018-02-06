using com.superscene.util;
using com.superscene.util.action;
using httpServer.assembly.page;
using httpServer.assembly.util;
using httpServer.control;
using httpServer.entity;
using httpServer.module;
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
		private ServerDataCtl serverDataCtl = null;

		private string title = "";
		private bool isWaitSave = false;
		private string regPath = "HKCU\\Software\\superHttpServer\\";
		private RegistryCtl regCtl = new RegistryCtl();

		private List<ServerItem> serverItem = new List<ServerItem>();
		
		private string nowType = "";

		Dictionary<int, string> mapIdxToType = new Dictionary<int, string>();

		bool isDebug = false;
		string strLog = "";
		private int itemTag = 0;

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

			//隐藏日志
			if (!isDebug) {
				grdMainBox.ColumnDefinitions[3].Width = new GridLength(0);
				grdMainBox.ColumnDefinitions[4].Width = new GridLength(0);
			}

			//
			title = Title;
			CmdServ.cfgWaitSave.listen(()=> {
				cfgWaitSaved();
			});
			CmdServ.cfgSaved.listen(() => {
				cfgSaved();
			});

			//
			initServerItem(ent.mainModule.lstServer);
			lstItem.SelectedIndex = Int32.Parse(regCtl.getValue(regPath + "selectItem", "0"));
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

		private void initServerItem(List<ServerModule> lstServer) {
			for(int i = 0; i < lstServer.Count; ++i) {
				ServerModule md = lstServer[i];

				ServerItem item = new ServerItem() { Tag = ++itemTag, Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() };
				md.serverItem = item;

				ent.mainModule.mapServer[md.type].initData(md);

				//lstItem.Items.Add(md.desc);
				serverItem.Add(item);
			}

			lstItem.ItemsSource = serverItem;
		}
		
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

		}

		private void Window_Closed(object sender, EventArgs e) {
			clear();

			regCtl.setValue(regPath + "x", this.Left.ToString());
			regCtl.setValue(regPath + "y", this.Top.ToString());
			regCtl.setValue(regPath + "width", this.Width.ToString());
			regCtl.setValue(regPath + "height", this.Height.ToString());
			regCtl.setValue(regPath + "selectItem", lstItem.SelectedIndex.ToString());

			serverDataCtl.save();
		}

		private void clear() {
			List<ServerModule> server = ent.mainModule.lstServer;
			for(int i = 0; i < server.Count; ++i) {
				ent.mainModule.mapServer[server[i].type].clear(server[i]);
			}
		}

		private void btnRestart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			int idx = lstItem.SelectedIndex;

			ServerModule md = ent.mainModule.lstServer[idx];

			ent.mainModule.mapServer[md.type].start();
			btnRestart.Content = md.isRun ? "重启" : "启动";
			md.serverItem.Source = getServerStatusImgPath(md.isRun);

			cfgSaved();
		}

		public string getServerStatusImgPath(bool isRun) {
			return isRun ? LocalRes.statusRun() : LocalRes.statusStop();
		}
		
		private void lstItem_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			int idx = lstItem.SelectedIndex;
			if(idx < 0 || idx >= ent.mainModule.lstServer.Count) {
				btnRestart.Content = "启动";
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
			
			serverItem[idx].Source = getServerStatusImgPath(md.isRun);

			ent.mainModule.mapServer[md.type].updateData(md);

			btnRestart.Content = md.isRun ? "重启" : "启动";

			cfgSaved();
		}

		private void btnStop_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			int idx = lstItem.SelectedIndex;


			ServerModule md = ent.mainModule.lstServer[idx];

			ent.mainModule.mapServer[md.type].stop();
			btnRestart.Content = md.isRun ? "重启" : "启动";
			md.serverItem.Source = getServerStatusImgPath(md.isRun);

			cfgSaved();
		}

		private void btnNew_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			List<ServerModule> lstServer = ent.mainModule.lstServer;

			string type = mapIdxToType[cbxType.SelectedIndex];

			ServerModule md = ent.mainModule.mapServer[type].createNewModel();
			md.type = type;

			ServerItem item = new ServerItem() { Tag = ++itemTag, Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() };
			md.serverItem = item;

			ent.mainModule.mapServer[md.type].initData(md);
			serverItem.Add(item);

			lstServer.Add(md);

			lstItem.ItemsSource = null;
			lstItem.ItemsSource = serverItem;

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

			lstItem.ItemsSource = null;
			lstItem.ItemsSource = serverItem;
			lstItem.SelectedIndex = newIdx;
		}

		public void log(string info) {
			if (!isDebug) {
				return;
			}

			strLog += info + "\r\n";
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

			List<ServerModule> lstServer = ent.mainModule.lstServer;
			ServerModule md = lstServer[idx];
			lstServer[idx] = lstServer[nextIex];
			lstServer[nextIex] = md;

			ServerItem item = serverItem[idx];
			serverItem[idx] = serverItem[nextIex];
			serverItem[nextIex] = item;
			lstItem.ItemsSource = null;
			lstItem.ItemsSource = serverItem;
			lstItem.SelectedIndex = sltIdx;
		}
	}
}
