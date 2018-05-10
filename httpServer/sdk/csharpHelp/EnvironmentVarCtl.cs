using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util.action {
	/// <summary>
	/// 环境变量设置
	/// </summary>
	public class EnvironmentVarCtl {
		public string logInfo = "";

		private RegistryCtl reg = new RegistryCtl();

		public EnvironmentVarCtl() {

		}

		/// <summary>
		/// 删除环境变量中的路径
		/// </summary>
		/// <param name="value"></param>
		/// <param name="isSetAllUser"></param>
		/// <param name="pathName"></param>
		public void removePath(string value, bool? isSetAllUser = null, string pathName = "path") {
			if(isSetAllUser == null) {
				_removePath(value, true, pathName);
				_removePath(value, false, pathName);
			} else {
				_removePath(value, isSetAllUser == true, pathName);
			}
		}

		private void _removePath(string value, bool isSetAllUser = false, string pathName = "path") {
			//string result = "";

			string strReg = "";
			if(isSetAllUser) {
				strReg = @"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\Session Manager\Environment\" + pathName;
			} else {
				strReg = @"HKEY_CURRENT_USER\Environment\" + pathName;
			}

			string reusltPath = "";
			try {
				value = value.Trim();
				string path = value;
				if(value != "." && value != "..") {
					path = Path.GetFullPath(value).Trim('\\');
				}
				//string path = Path.GetFullPath(value).Trim('\\');
				//result += path + "," + value + "\r\n";

				string oldValue = reg.getValue(strReg);
				if(oldValue == "") {
					return;
				}

				string[] arr = oldValue.Split(';');
				for(int i = 0; i < arr.Length; ++i) {
					arr[i] = arr[i].Trim();
					if(arr[i] == "") {
						continue;
					}

					if(arr[i] == "." || arr[i] == "..") {
						reusltPath += (reusltPath == "" ? "" : ";") + arr[i];
						continue;
					}

					string oldPath = Path.GetFullPath(arr[i]).Trim('\\');
					//result += oldPath + "," + arr[i] + "\r\n";
					if(oldPath == path) {
						continue;
					}

					reusltPath += (reusltPath == "" ? "" : ";") + oldPath;
				}

				//reusltPath += (reusltPath == "" ? "" : ";") + path;

			} catch(Exception) {
				return;
			}

			if(reusltPath == "") {
				remove(pathName, isSetAllUser);
			} else {
				reg.setValue(strReg, reusltPath);
			}

			//return result;
		}

		/// <summary>
		/// 添加环境变量
		/// </summary>
		/// <param name="value"></param>
		/// <param name="isSetAllUser"></param>
		/// <param name="pathName"></param>
		public void addPath(string value, bool isSetAllUser = false, string pathName = "path") {
			//string result = "";

			string strReg = "";
			if(isSetAllUser) {
				strReg = @"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\Session Manager\Environment\" + pathName;
			} else {
				strReg = @"HKEY_CURRENT_USER\Environment\" + pathName;
			}

			string reusltPath = "";
			try {
				value = value.Trim();
				string path = value;
				if(value != "." && value != "..") {
					path = Path.GetFullPath(value).Trim('\\');
				}
				//string path = Path.GetFullPath(value).Trim('\\');
				//result += path + "," + value + "\r\n";

				string oldValue = reg.getValue(strReg);
				string[] arr = oldValue.Split(';');
				for(int i = 0; i < arr.Length; ++i) {
					arr[i] = arr[i].Trim();
					if(arr[i] == "") {
						continue;
					}

					if(arr[i] == "." || arr[i] == "..") {
						reusltPath += (reusltPath == "" ? "" : ";") + arr[i];
						continue;
					}

					string oldPath = Path.GetFullPath(arr[i]).Trim('\\');
					//result += oldPath + "," + arr[i] + "\r\n";
					if(oldPath == path) {
						continue;
					}

					reusltPath += (reusltPath == "" ? "" : ";") + oldPath;
				}

				reusltPath += (reusltPath == "" ? "" : ";") + path;

			} catch(Exception) {
				return ;
			}

			//Console.WriteLine(strReg);
			//value = reg.getValue(strReg) + value;

			string param = "";
			if(isSetAllUser) {
				param += "/m ";
			}
			param += "\"" + pathName + "\" ";
			param += "\"" + reusltPath + "\" ";
			param = param.Replace(@"\", @"/");
			//result += param + "\r\n";

			runExe("setx.exe", param);

			Environment.SetEnvironmentVariable(pathName, reusltPath);

			//return result;
		}

		/// <summary>
		/// 删除环境变量
		/// </summary>
		/// <param name="name"></param>
		/// <param name="isSetAllUser">是否为系统变量，null：环境和用户变量都删除</param>
		public void remove(string name, bool? isSetAllUser = null) {
			//RegistryCtl reg = new RegistryCtl();

			switch(isSetAllUser) {
				case null: {
					string strReg = @"HKEY_CURRENT_USER\Environment\" + name;
					reg.remove(strReg);

					strReg = @"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\Session Manager\Environment\" + name;
					reg.remove(strReg);
					break;
				}
				case true: {
					string strReg = @"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\Session Manager\Environment\" + name;
					reg.remove(strReg);
					break;
				}
				case false: {
					string strReg = @"HKEY_CURRENT_USER\Environment\" + name;
					reg.remove(strReg);
					break;
				}
			}
		}

		/// <summary>
		/// 设置环境变量
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="isSetAllUser"></param>
		public void setValue(string name, string value, bool isSetAllUser = false) {
			if(name == "" || value == "") {
				return;
			}

			string param = "";
			if(isSetAllUser) {
				param += "/m ";
			}
			param += "\"" + name + "\" ";
			param += "\"" + value + "\" ";

			runExe("setx.exe", param);

			Environment.SetEnvironmentVariable(name, value);
		}

		//运行Exe
		private void runExe(string exePath, string param) {
			Process exep = new Process();
			exep.StartInfo.FileName = exePath;
			exep.StartInfo.Arguments = param;
			exep.StartInfo.CreateNoWindow = true;
			exep.StartInfo.UseShellExecute = false;
			exep.StartInfo.RedirectStandardOutput = true;
			exep.Start();
			//Thread.Sleep(5000);
			exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
							   //exep.WaitForInputIdle(5000);
			try {
				string data = exep.StandardOutput.ReadToEnd();
				logInfo += data;
				//Console.WriteLine(data);
				//log.Debug(data);
			} catch(Exception) {
				//Console.WriteLine(ex.ToString());
				//log.Debug(ex.ToString());
			}
			exep.Close();
		}

	}
}
