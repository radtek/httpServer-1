using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util.action {
	public class SystemCtl {
		/// <summary>
		/// 根据标题查找进程
		/// </summary>
		/// <param name="title">标题</param>
		/// <param name="isAccurate">是否精确查找</param>
		/// <returns>进程列表</returns>
		public static List<Process> findProcessByTitle(string title, bool isAccurate = false) {
			//bool result = false;
			List<Process> lstPro = new List<Process>();

			foreach(Process p in Process.GetProcesses()) {
				//Debug.WriteLine(p.ProcessName + "," + p.MainWindowTitle);
				//try {
				//	Debug.WriteLine(p.ProcessName + "," + p.MainWindowTitle + "," + p.MainModule.FileName.ToString());
				//} catch(Exception ex) {
				//	//Debug.WriteLine(ex.ToString());
				//}

				if(isAccurate) {
					//精确查找
					if(p.MainWindowTitle == title) {
						lstPro.Add(p);
					}
				} else {
					//模糊查找
					if(p.MainWindowTitle.ToLower().IndexOf(title.ToLower()) >= 0) {
						lstPro.Add(p);
					}
				}
			}

			return lstPro;
		}
		
		/// <summary>
		/// 是否为管理员权限
		/// </summary>
		/// <returns></returns>
		public static bool isAdministrator() {
			bool isAdmin = false;
			try {
				WindowsIdentity user = WindowsIdentity.GetCurrent();
				WindowsPrincipal principal = new WindowsPrincipal(user);
				isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
				isAdmin = false;
			}
			return isAdmin;
		}

		/// <summary>
		/// 获取本地的IP地址
		/// </summary>
		/// <returns></returns>
		public static string getIp() {
			string AddressIP = string.Empty;
			foreach(IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
				if(_IPAddress.AddressFamily.ToString() == "InterNetwork") {
					AddressIP = _IPAddress.ToString();
					Debug.WriteLine(AddressIP);
				}
			}
			return AddressIP;
		}

		/// <summary>
		/// 获取本地的所有IP地址
		/// </summary>
		/// <returns></returns>
		public static List<string> getAllIp() {
			List<string> lstIp = new List<string>();

			//string AddressIP = string.Empty;
			foreach(IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
				if(_IPAddress.AddressFamily.ToString() == "InterNetwork") {
					//AddressIP = _IPAddress.ToString();
					//Debug.WriteLine(AddressIP);
					lstIp.Add(_IPAddress.ToString());
				}
			}
			return lstIp;
		}

		/// <summary>
		/// 获取一个空闲端口号
		/// </summary>
		/// <returns></returns>
		public static int getFreePort(int startPort = 1025) {
			HashSet<int> portUsed = PortIsUsed();
			int result = startPort;

			while(portUsed.Contains(result)) {
				++result;
			}

			return result;
		}

		/// <summary>
		/// 端口是否使用
		/// </summary>
		/// <returns></returns>
		public static bool isPortUsed(int prot) {
			HashSet<int> portUsed = PortIsUsed();

			return portUsed.Contains(prot);
		}

		/// <summary>
		/// 检查指定端口是否已用
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		public static bool PortIsAvailable(int port) {
			//bool isAvailable = true;
			HashSet<int> portUsed = PortIsUsed();

			return portUsed.Contains(port);

			//foreach(int p in portUsed) {
			//	if(p == port) {
			//		isAvailable = false; break;
			//	}
			//}
			//return isAvailable;
		}

		/// <summary>
		/// 获取操作系统已用的端口号
		/// </summary>
		/// <returns></returns>
		public static HashSet<int> PortIsUsed() {
			//获取本地计算机的网络连接和通信统计数据的信息
			IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
			//返回本地计算机上的所有Tcp监听程序
			IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();
			//返回本地计算机上的所有UDP监听程序
			IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();
			//返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
			TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
			HashSet<int> allPorts = new HashSet<int>();
			foreach(IPEndPoint ep in ipsTCP) allPorts.Add(ep.Port);
			foreach(IPEndPoint ep in ipsUDP) allPorts.Add(ep.Port);
			foreach(TcpConnectionInformation conn in tcpConnInfoArray) allPorts.Add(conn.LocalEndPoint.Port);
			return allPorts;
		}

		//获取磁盘空闲空间
		public static Dictionary<string, double> getDriveFreeSize() {
			Dictionary<string, double> mapDrive = new Dictionary<string, double>();

			//double totalSize = 0;
			double freeSize = 0;
			long gb = 1024 * 1024 * 1024;

			foreach(DriveInfo drive in DriveInfo.GetDrives()) {
				//判断是否是固定磁盘  
				if(drive.DriveType == DriveType.Fixed) {
					//totalSize = (double)drive.TotalSize / gb;
					freeSize = (double)drive.TotalFreeSpace / gb;
					//richTextBox1.Text += drive.Name + ": 总空间=" + lsum.ToString() + " 剩余空间=" + ldr.ToString()+"\n\r";
					mapDrive[drive.Name[0].ToString().ToLower()] = freeSize;
				}
			}
			//progressBar1.Value = int.Parse((lsum - ldr).ToString());  
			//progressBar1.Maximum = int.Parse(lsum.ToString());  
			//lbMsg.Text = "磁盘" + disksrc + "的可用空间为" + ldr + "GB！";  

			return mapDrive;
		}

		/// <summary>  
		/// 检查服务是否存在
		/// </summary>  
		/// <param name=" strServiceName ">服务名</param>  
		/// <returns>存在返回 true,否则返回 false;</returns>  
		public static bool isServiceExist(string strServiceName) {
			ServiceController[] services = ServiceController.GetServices();
			foreach(ServiceController s in services) {
				if(s.ServiceName.ToLower() == strServiceName.ToLower()) {
					return true;
				}
			}
			return false;
		}

		/// <summary>  
		/// 检查服务是否启动
		/// </summary>  
		/// <param name=" strServiceName ">服务名</param>  
		/// <returns>存在返回 true,否则返回 false;</returns>  
		public static bool isServiceStart(string strServiceName) {
			ServiceController[] services = ServiceController.GetServices();
			foreach(ServiceController s in services) {
				if(s.ServiceName.ToLower() == strServiceName.ToLower()) {
					if(s.Status == ServiceControllerStatus.Running) {
						return true;
					}
					return false;
				}
			}
			return false;
		}

		///// <summary>  
		///// 安装服务  
		///// </summary>  
		//private bool InstallService(string NameService) {
		//	bool flag = true;
		//	if(!IsServiceIsExisted(NameService)) {
		//		try {
		//			string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
		//			string serviceFileName = location.Substring(0, location.LastIndexOf('\\') + 1) + NameService + ".exe";
		//			InstallmyService(null, serviceFileName);
		//		} catch {
		//			flag = false;
		//		}

		//	}
		//	return flag;
		//}

		///// <summary>  
		///// 卸载服务  
		///// </summary>  
		//private bool UninstallService(string NameService) {
		//	bool flag = true;
		//	if(IsServiceIsExisted(NameService)) {
		//		try {
		//			string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
		//			string serviceFileName = location.Substring(0, location.LastIndexOf('\\') + 1) + NameService + ".exe";
		//			UnInstallmyService(serviceFileName);
		//		} catch {
		//			flag = false;
		//		}
		//	}
		//	return flag;
		//}

		///// <summary>  
		///// 安装Windows服务  
		///// </summary>  
		///// <param name="stateSaver">集合</param>  
		///// <param name="filepath">程序文件路径</param>  
		//public static void InstallmyService(IDictionary stateSaver, string filepath) {
		//	AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
		//	AssemblyInstaller1.UseNewContext = true;
		//	AssemblyInstaller1.Path = filepath;
		//	AssemblyInstaller1.Install(stateSaver);
		//	AssemblyInstaller1.Commit(stateSaver);
		//	AssemblyInstaller1.Dispose();
		//}

		///// <summary>  
		///// 卸载Windows服务  
		///// </summary>  
		///// <param name="filepath">程序文件路径</param>  
		//public static void UnInstallmyService(string filepath) {
		//	AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
		//	AssemblyInstaller1.UseNewContext = true;
		//	AssemblyInstaller1.Path = filepath;
		//	AssemblyInstaller1.Uninstall(null);
		//	AssemblyInstaller1.Dispose();
		//}

	}

}
