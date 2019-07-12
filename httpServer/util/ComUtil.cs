using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.util {
	public class ComUtil {
		public static List<string> findAllIp() {
			List<string> lstIP = new List<string>();
			List<string> lstIPv6 = new List<string>();
			try {
				string ipLocal = "127.0.0.1";

				string hostName = Dns.GetHostName();//本机名   
				IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6   
				foreach(IPAddress ip in addressList) {
					if(ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal || ip.IsIPv6Teredo || ip.IsIPv4MappedToIPv6) {
						//ipv6
						lstIPv6.Add(ip.ToString());
					} else {
						if(ip.ToString() != ipLocal) {
							lstIP.Add(ip.ToString());
						}
					}
				}
				
			} catch(Exception) { }

			return lstIP;
		}

		/// <summary>
		/// 获取操作系统已用的ip+端口号
		/// </summary>
		/// <returns></returns>
		internal static HashSet<string> allUsedIpPort() {
			//获取本地计算机的网络连接和通信统计数据的信息
			IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
			//返回本地计算机上的所有Tcp监听程序
			IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();
			//返回本地计算机上的所有UDP监听程序
			IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();
			//返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
			TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
			HashSet<string> allPorts = new HashSet<string>();
			foreach(IPEndPoint ep in ipsTCP) allPorts.Add(ep.Address.ToString() + ":" + ep.Port);
			foreach(IPEndPoint ep in ipsUDP) allPorts.Add(ep.Address.ToString() + ":" + ep.Port);
			foreach(TcpConnectionInformation conn in tcpConnInfoArray) allPorts.Add(conn.LocalEndPoint.Address.ToString() + ":" + conn.LocalEndPoint.Port);
			return allPorts;
		}

		//运行Exe
		public static void runExe(string exePath, string param) {
			//定义一个ProcessStartInfo实例
			ProcessStartInfo info = new ProcessStartInfo();
			//设置启动进程的初始目录
			info.WorkingDirectory = SysConst.rootPath();
			//info.EnvironmentVariables["PATH"] = path;
			info.UseShellExecute = false;
			//info.EnvironmentVariables["PATH"] = path;
			//设置启动进程的应用程序或文档名
			info.FileName = exePath;
			//设置启动进程的参数
			info.Arguments = param;
			//启动由包含进程启动信息的进程资源
			try {
				Process.Start(info);
			} catch(Exception) {
				//MessageBox.Show(this, we.Message);
				return;
			}
		}

		//运行Exe
		public static void runExeNoWin(string exePath, string param) {
			Process exep = new Process();
			exep.StartInfo.FileName = exePath;
			exep.StartInfo.Arguments = param;
			exep.StartInfo.CreateNoWindow = true;
			exep.StartInfo.UseShellExecute = false;
			exep.StartInfo.RedirectStandardOutput = true;
			exep.StartInfo.RedirectStandardError = true;
			//设置启动动作,确保以管理员身份运行
			exep.StartInfo.Verb = "runas";
			exep.Start();
			exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
			try {
				string data = exep.StandardOutput.ReadToEnd();
				//Console.WriteLine(data);

				data = exep.StandardError.ReadToEnd();
			} catch(Exception) {
				//Console.WriteLine(ex.ToString());
			}
			exep.Close();
		}

	}
}
