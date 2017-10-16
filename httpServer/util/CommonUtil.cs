using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.util {
	public class CommonUtil {
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

				//cbxIp.Items.Add("127.0.0.1");
				////cbxIp.ListItems.Add("127.0.0.1");
				//for(int i = 0; i < lstIP.Count; ++i) {
				//	cbxIp.Items.Add(lstIP[i]);
				//	//cbxIp.ListItems.Add(lstIP[i]);
				//}
				//for(int i = 0; i < lstIPv6.Count; ++i) {
				//	cbxIp.Items.Add(lstIPv6[i]);
				//}
			} catch(Exception) {

			}

			return lstIP;
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
