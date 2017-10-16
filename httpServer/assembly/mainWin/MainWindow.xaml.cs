using com.superscene.util;
using com.superscene.util.action;
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
		private List<ServerCtl> lstServerClt = new List<ServerCtl>();
		//HttpCtl httpCtl = new HttpCtl();

		public MainWindow() {
			InitializeComponent();
			ins = this;

			//rootPath = AppDomain.CurrentDomain.BaseDirectory;

			//
			ent = Entity.getInstance();
			ent.mainModule = new MainModule();

			serverDataCtl = new ServerDataCtl();
			serverDataCtl.load();

			//httpCtl.timeout = 3000;


			//string path = regCtl.getValue(regPath + "path", rootPath);
			//string port = regCtl.getValue(regPath + "port", "8091");
			int x = 100;
			int y = 100;
			int w = 400;
			int h = 200;
			try {
				x = Int32.Parse(regCtl.getValue(regPath + "x", "100"));
				y = Int32.Parse(regCtl.getValue(regPath + "y", "100"));
				w = Int32.Parse(regCtl.getValue(regPath + "width", "680"));
				h = Int32.Parse(regCtl.getValue(regPath + "height", "270"));
				//cbxIp.Text = regCtl.getValue(regPath + "ip", "127.0.0.1");
				//txtProxy.IsChecked = regCtl.getValue(regPath + "isProxy", "false") == "true";
				//txtProxy.Text = regCtl.getValue(regPath + "proxyUrl", "");
				//txtTransmit.IsChecked = regCtl.getValue(regPath + "isTransmit", "false") == "true";
				//txtTransmit.Text = regCtl.getValue(regPath + "transmitUrl", "");
			} catch(Exception) {

			}

			this.Left = x;
			this.Top = y;
			this.Width = w;
			this.Height = h;

			//serverPath = path + "/";
			//txtPath.Text = path;
			//lblUrl.Content = "http://127.0.0.1:8091";
			//isProxy = txtProxy.IsChecked == true;
			//proxyUrl = txtProxy.Text;
			//isTransmit = txtTransmit.IsChecked == true;
			//transmitUrl = txtTransmit.Text;

			//
			//txtProxy.IsChecked = isProxy = false;
			txtProxy.Visibility = Visibility.Collapsed;
			//txtTransmit.IsChecked = isTransmit = false;
			txtTransmit.Visibility = Visibility.Collapsed;

			//find ip
			List<string> lstIP = CommonUtil.findAllIp();
			cbxIp.Items.Add("127.0.0.1");
			for(int i = 0; i < lstIP.Count; ++i) {
				cbxIp.Items.Add(lstIP[i]);
				//cbxIp.ListItems.Add(lstIP[i]);
			}
			//findAllIp();
			//txtPort.Text = port;

			//List<ServerModule> lstServer = ent.mainModule.lstServer;
			//for(int i = 0; i < lstServer.Count; ++i) {
			//	ServerModule md = lstServer[i];
			//	//lstItem.Items.Add(md.desc);
			//}
			////lstItem.Items.Add("asdf");
			//ObservableCollection<User> Users = new ObservableCollection<User>();
			//Users.Add(new User() { Content = "aaa", Source = LocalRes.statusRun() });
			//Users.Add(new User() { Content = "bbb", Source = LocalRes.statusStop() });
			//Users.Add(new User() { Content = "ccc", Source = LocalRes.statusStop() });
			//Users.Add(new User() { Content = "ccc", Source = LocalRes.statusStop() });
			//lstItem.ItemsSource = Users;
			initServerItem(ent.mainModule.lstServer);
			lstItem.SelectedIndex = 0;
			//Debug.WriteLine(lstItem.SelectedIndex);

			//try {
			//	lastStatus = "httpServer";
			//	updateCopyUrlPos();
			//} catch(Exception) {

			//}
			//restartServer();
		}

		private void initServerItem(List<ServerModule> lstServer) {
			for(int i = 0; i < lstServer.Count; ++i) {
				ServerModule md = lstServer[i];
				//lstItem.Items.Add(md.desc);
				serverItem.Add(new ServerItem() { Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() });

				ServerCtl ctl = new ServerCtl();
				ctl.md = md;
				lstServerClt.Add(ctl);
				if(md.isRun) {
					ctl.restartServer();
				}
			}

			lstItem.ItemsSource = serverItem;
			//lstItem.SelectedIndex = 0;
		}

		//private void findAllIp() {
		//	try {
		//		List<string> lstIP = new List<string>();
		//		List<string> lstIPv6 = new List<string>();
		//		string ipLocal = "127.0.0.1";

		//		string hostName = Dns.GetHostName();//本机名   
		//		IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6   
		//		foreach(IPAddress ip in addressList) {
		//			if(ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal || ip.IsIPv6Teredo || ip.IsIPv4MappedToIPv6) {
		//				//ipv6
		//				lstIPv6.Add(ip.ToString());
		//			} else {
		//				if(ip.ToString() != ipLocal) {
		//					lstIP.Add(ip.ToString());
		//				}
		//			}
		//		}

		//		cbxIp.Items.Add("127.0.0.1");
		//		//cbxIp.ListItems.Add("127.0.0.1");
		//		for(int i = 0; i < lstIP.Count; ++i) {
		//			cbxIp.Items.Add(lstIP[i]);
		//			//cbxIp.ListItems.Add(lstIP[i]);
		//		}
		//		//for(int i = 0; i < lstIPv6.Count; ++i) {
		//		//	cbxIp.Items.Add(lstIPv6[i]);
		//		//}
		//	} catch(Exception) {

		//	}
		//}
		

		private void lblUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			CommonUtil.runExe("explorer.exe", lblUrl.Content.ToString());
		}

		private void lblCopyUrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			Clipboard.SetDataObject(lblUrl.Content);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

		}

		private void Window_Closed(object sender, EventArgs e) {
			clear();

			regCtl.setValue(regPath + "x", this.Left.ToString());
			regCtl.setValue(regPath + "y", this.Top.ToString());
			regCtl.setValue(regPath + "width", this.Width.ToString());
			regCtl.setValue(regPath + "height", this.Height.ToString());

			//regCtl.setValue(regPath + "path", txtPath.Text);
			//regCtl.setValue(regPath + "ip", cbxIp.Text);
			//regCtl.setValue(regPath + "port", txtPort.Text);
			//regCtl.setValue(regPath + "isProxy", (txtProxy.IsChecked == true) ? "true" : "false");
			//regCtl.setValue(regPath + "proxyUrl", txtProxy.Text);
			//regCtl.setValue(regPath + "isTransmit", (txtTransmit.IsChecked == true) ? "true" : "false");
			//regCtl.setValue(regPath + "transmitUrl", txtTransmit.Text);

			serverDataCtl.save();
		}

		private void clear() {
			for(int i = 0; i < lstServerClt.Count; ++i) {
				lstServerClt[i].clear();
			}
		}

		private void btnRestart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			//try {
			//	clear();
			//	stopServer = false;
			//	updateCopyUrlPos();
			//} catch(Exception) {

			//}
			//int idx = lstItem.SelectedIndex;
			//if(idx < 0 || idx >= serverItem.Count) {
			//	return;
			//}

			updateData("isRun", true);

			//lstServerClt[idx].restartServer();
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
			//int idx = lstItem.SelectedIndex;
			//if(idx < 0 || idx >= serverItem.Count) {
			//	return;
			//}

			//transmitUrl = txtTransmit.Text;

			//List<ServerModule> lstServer = ent.mainModule.lstServer;
			//lstServer[idx].transmitUrl = txtTransmit.Text;

			updateData("transmitUrl", txtTransmit.Text);
		}

		private void txtDesc_TextChanged(object sender, TextChangedEventArgs e) {
			//int idx = lstItem.SelectedIndex;
			//if(idx < 0 || idx >= serverItem.Count) {
			//	return;
			//}

			//serverItem[idx].Content = txtDesc.Text;

			//List<ServerModule> lstServer = ent.mainModule.lstServer;
			//lstServer[idx].desc = txtDesc.Text;

			updateData("desc", txtDesc.Text);
		}

		private void updateData(string name, string value) {
			int idx = lstItem.SelectedIndex;
			if(idx < 0 || idx >= serverItem.Count) {
				return;
			}

			ServerModule md = ent.mainModule.lstServer[idx];

			switch(name) {
			case "desc": {
					md.desc = value;
					serverItem[idx].Content = value;
					//if(isDescIp(md.desc)) {
					//	serverItem[idx].Content = value.Substring(1);
					//} else {
					//	serverItem[idx].Content = value;
					//}
					break;
				}
			case "transmitUrl":md.transmitUrl = value; break;
			case "proxyUrl": md.proxyUrl = value; break;
			case "path": md.path = value; break;
			}
		}

		private void updateData(string name, bool value) {
			int idx = lstItem.SelectedIndex;
			if(idx < 0 || idx >= serverItem.Count) {
				return;
			}

			ServerModule md = ent.mainModule.lstServer[idx];

			switch(name) {
			case "isTransmit": md.isTransmit = value; break;
			case "isProxy": md.isProxy = value; break;
			case "isRun": {
					md.isRun = value;
					serverItem[idx].Source = getServerStatusImgPath(md.isRun);
					if(value == true) {
						md.ip = cbxIp.Text;
						md.port = txtPort.Text;
						lblUrl.Content = "http://" + md.ip + ":" + md.port;
						lstServerClt[idx].restartServer();
					} else {
						lstServerClt[idx].clear();
					}
					if(md.desc == "" || isDescIp(md.desc)) {
						txtDesc.Text = md.ip + ":" + md.port;
						updateData("desc", md.ip + ":" + md.port);
					}
					break;
				}
			}
		}

		private string getServerStatusImgPath(bool isRun) {
			return isRun ? LocalRes.statusRun() : LocalRes.statusStop();
		}

		private void lstItem_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			int idx = lstItem.SelectedIndex;
			if(idx < 0 || idx >= ent.mainModule.lstServer.Count) {
				txtDesc.Text = "";
				cbxIp.Text = "";
				txtPort.Text = "";
				txtPath.Text = SysConst.rootPath();

				txtProxy.IsChecked = false;
				txtProxy.Text = "";
				txtTransmit.IsChecked = false;
				txtTransmit.Text = "";
				btnRestart.Content = "启动";
				lblUrl.Content = "127.0.0.1:8091";
				return;
			}

			ServerModule md = ent.mainModule.lstServer[idx];
			txtDesc.Text = md.desc;
			serverItem[idx].Source = getServerStatusImgPath(md.isRun);

			cbxIp.Text = md.ip;
			txtPort.Text = md.port;

			txtPath.Text = md.path;

			txtProxy.IsChecked = md.isProxy;
			txtProxy.Text = md.proxyUrl;

			txtTransmit.IsChecked = md.isTransmit;
			txtTransmit.Text = md.transmitUrl;

			btnRestart.Content = md.isRun ? "重启" : "启动";

			//serverPath = md.path + "/";
			lblUrl.Content = "http://" + md.ip + ":" + md.port;

		}

		private bool isDescIp(string desc) {
			return Regex.IsMatch(desc, "^[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}:[0-9]{1,5}");
		}

		private void btnStop_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			updateData("isRun", false);
		}

		private void btnNew_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			List<ServerModule> lstServer = ent.mainModule.lstServer;

			ServerModule md = new ServerModule();
			md.port = SystemCtl.getFreePort(8091).ToString();
			md.desc = md.ip + ":" + md.port;
			md.path = SysConst.rootPath();
			lstServer.Add(md);

			serverItem.Add(new ServerItem() { Content = md.desc, Source = md.isRun ? LocalRes.statusRun() : LocalRes.statusStop() });

			ServerCtl ctl = new ServerCtl();
			ctl.md = md;
			lstServerClt.Add(ctl);

			lstItem.SelectedIndex = serverItem.Count - 1;

			lstItem.ScrollIntoView(lstItem.Items[serverItem.Count - 1]);
		}

		private void btnDel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			int idx = lstItem.SelectedIndex;
			if(idx < 0 || idx >= serverItem.Count) {
				return;
			}

			List<ServerModule> lstServer = ent.mainModule.lstServer;
			lstServerClt[idx].clear();

			lstServer.RemoveAt(idx);
			serverItem.RemoveAt(idx);
			lstServerClt.RemoveAt(idx);

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
