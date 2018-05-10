using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	/// <summary>
	/// 注册表设置
	/// </summary>
	public class RegistryCtl {
		private Dictionary<string, RegistryKey> mapReg = new Dictionary<string, RegistryKey>();
		private Dictionary<string, RegistryHive> mapBaseReg = new Dictionary<string, RegistryHive>();
		private Dictionary<string, RegistryValueKind> mapRegType = new Dictionary<string, RegistryValueKind>();
		public string logInfo = "";
		public bool isOpenBaseKey = false;

		public RegistryCtl() {
			mapReg["HKEY_CLASSES_ROOT"] = Registry.ClassesRoot;
			mapReg["HKCR"] = Registry.ClassesRoot;
			mapReg["HKEY_CURRENT_USER"] = Registry.CurrentUser;
			mapReg["HKCU"] = Registry.CurrentUser;
			mapReg["HKEY_LOCAL_MACHINE"] = Registry.LocalMachine;
			mapReg["HKLM"] = Registry.LocalMachine;
			mapReg["HKEY_USERS"] = Registry.Users;
			mapReg["HKU"] = Registry.Users;
			mapReg["HKEY_CURRENT_CONFIG"] = Registry.CurrentConfig;
			mapReg["HKCC"] = Registry.CurrentConfig;

			mapBaseReg["HKEY_CLASSES_ROOT"] = RegistryHive.ClassesRoot;
			mapBaseReg["HKCR"] = RegistryHive.ClassesRoot;
			mapBaseReg["HKEY_CURRENT_USER"] = RegistryHive.CurrentUser;
			mapBaseReg["HKCU"] = RegistryHive.CurrentUser;
			mapBaseReg["HKEY_LOCAL_MACHINE"] = RegistryHive.LocalMachine;
			mapBaseReg["HKLM"] = RegistryHive.LocalMachine;
			mapBaseReg["HKEY_USERS"] = RegistryHive.Users;
			mapBaseReg["HKU"] = RegistryHive.Users;
			mapBaseReg["HKEY_CURRENT_CONFIG"] = RegistryHive.CurrentConfig;
			mapBaseReg["HKCC"] = RegistryHive.CurrentConfig;

			mapRegType["reg_sz"] = RegistryValueKind.String;
			mapRegType["reg_binary"] = RegistryValueKind.Binary;
			mapRegType["reg_dword"] = RegistryValueKind.DWord;
			mapRegType["reg_qword"] = RegistryValueKind.QWord;
			mapRegType["reg_multi_sz"] = RegistryValueKind.MultiString;
			mapRegType["reg_expand_sz"] = RegistryValueKind.ExpandString;
		}

		private RegistryKey getPath(string path, bool isCreate) {
			RegistryKey temp = null;
			string name = "";

			getPath(path, isCreate, out temp, out name);
			if(temp == null) {
				return null;
			}

			return temp.OpenSubKey(name, true);
		}
		
		private void getPath(string path, bool isCreate, out RegistryKey root, out string name) {
			root = null;
			name = "";

			path = path.Trim();
			//get sub key idx
			int idx = path.IndexOf('\\');
			if(idx < 0) {
				idx = path.IndexOf('/');
			}
			if(idx < 0) {
				return ;
			}

			//get value idx
			int valueIdx = path.LastIndexOf('\\');
			if(valueIdx < 0) {
				valueIdx = path.LastIndexOf('/');
			}
			if(valueIdx < 0) {
				return ;
			}
			if(idx > valueIdx) {
				return ;
			}

			//get root
			string strRoot = path.Substring(0, idx);
			RegistryKey regRoot = null;
			if(isOpenBaseKey) {
				if(mapBaseReg.ContainsKey(strRoot)) {
					try {
						regRoot = RegistryKey.OpenBaseKey(mapBaseReg[strRoot], RegistryView.Registry64);
					} catch(Exception) {
						regRoot = RegistryKey.OpenBaseKey(mapBaseReg[strRoot], RegistryView.Registry32);
					}
				} else {
					return;
				}
			} else {
				if(mapReg.ContainsKey(strRoot)) {
					regRoot = mapReg[strRoot];
				} else {
					return;
				}
			}

			//sub key
			RegistryKey regSub = null;
			string value = path.Substring(valueIdx + 1);
			if(idx == valueIdx) {
				regSub = regRoot;
			} else {
				string sub = path.Substring(idx + 1, valueIdx - 1 - idx);
				//string value = path.Substring(valueIdx + 1);

				if(isCreate) {
					regSub = regRoot.CreateSubKey(sub);
				} else {
					regSub = regRoot.OpenSubKey(sub, true);
				}
				regRoot.Close();
			}
			root = regSub;
			name = value;
		}

		/// <summary>
		/// 删除项或键，最后一位为"/"或"\"，则删除项
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool remove(string path) {
			if(path == "") {
				return false;
			}

			bool isPath = false;
			if(lastChar(path) == "/" || lastChar(path) == "\\") {
				path = path.Substring(0, path.Length - 1);
				isPath = true;
			}

			bool result = false;
			RegistryKey root = null;
			string name = "";

			try {
				getPath(path, false, out root, out name);
				if(root == null) {
					return result;
				}

				if(isPath) {
					root.DeleteSubKey(name);
				} else {
					root.DeleteValue(name);
				}

				result = true;

			} catch(Exception) {
				//return "";
				//Debug.WriteLine(ex.ToString());
			}

			root.Close();

			return result;
		}

		/// <summary>
		/// 删除键
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool removeValue(string path) {
			if (path == "") {
				return false;
			}

			bool result = false;
			RegistryKey root = null;
			string name = "";

			try {
				getPath(path, false, out root, out name);
				if (root == null) {
					return result;
				}
				
				root.DeleteValue(name);

				result = true;

			} catch (Exception ex) {
				//return "";
				Debug.WriteLine(ex.ToString());
			}

			root.Close();

			return result;
		}

		private string lastChar(string str) {
			return str.Length <= 0 ? "" : str[str.Length - 1].ToString();
		}

		/// <summary>
		/// 删除项
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool removePath(string path) {
			bool result = false;
			RegistryKey root = null;
			string name = "";

			try {
				getPath(path, false, out root, out name);
				if(root == null) {
					return result;
				}

				root.DeleteSubKey(name);

				result = true;

			} catch(Exception) {
				//return "";
			}

			root.Close();

			return result;
		}

		/// <summary>
		/// 创建项
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool createPath(string path) {
			bool result = false;
			//创建项
			RegistryKey root = null;
			string name = "";

			getPath(path, true, out root, out name);
			if(root == null) {
				return result;
			}

			try {
				if(name != "") {
					RegistryKey sub = root.CreateSubKey(name);
					sub.Close();
				}
				result = true;
			} catch(Exception) { }

			root.Close();
			return result;
		}

		/// <summary>
		/// 设置键值，不存在则创建(递归创建)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool setValue(string path, string value, string type = "reg_sz") {
			bool result = false;

			if(type == "") {
				//创建项
				RegistryKey root = null;
				string name = "";

				getPath(path, true, out root, out name);
				if(root == null) {
					return result;
				}

				try {
					//RegistryKey sub = root;
					if(name != "") {
						RegistryKey sub = root.CreateSubKey(name);
						sub.CreateSubKey(value);
						sub.Close();
					} else {
						root.CreateSubKey(value);
					}
					result = true;
				} catch(Exception) { }

				root.Close();
				return result;
			}

			//set value
			type = type.Trim().ToLower();
			if(!mapRegType.ContainsKey(type)) {
				return false;
			}

			return setValue(path, value, mapRegType[type]);
		}

		public bool setValue(string path, string value, RegistryValueKind type) {
			bool result = false;

			RegistryKey root = null;
			string name = "";

			//try {
			getPath(path, true, out root, out name);
			if(root == null) {
				return result;
			}
			root.SetValue(name, value);
			//root.SetValue(name, value, type);

			result = true;
			//} catch(Exception) {
			//	//return "";
			//}

			root.Close();

			return result;
		}

		public bool setValueBool(string path, bool value, string type = "reg_sz") {
			return setValue(path, value ? "true" : "false", type);
		}

		public bool setValueBool(string path, bool value, RegistryValueKind type) {
			return setValue(path, value ? "true" : "false", type);
		}

		/// <summary>
		/// 获取键值，不存在这返回空字符串
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string getValue(string path, string initData = "") {
			string result = initData;

			RegistryKey root = null;
			string name = "";

			try {
				getPath(path, false, out root, out name);
				if(root == null) {
					return initData;
				}

				result = root.GetValue(name, initData).ToString();
			} catch(Exception) {
				//return "";
			}

			root?.Close();
			return result;
		}

		public bool getValueBool(string path, bool initData = false) {
			string result = getValue(path);
			if(result == "") {
				return initData;
			}
			return result == "true";
		}

		public void each(string path, Action<string> callback, bool isItem) {
			RegistryKey root = null;
			string[] names = null;

			try {
				root = getPath(path, false);
				if(root == null) {
					return ;
				}

				if(isItem) {
					names = root.GetSubKeyNames();
				}else{
					names = root.GetValueNames();
				}
			} catch(Exception) {
				//return "";
			}
			root?.Close();

			if(names == null) {
				return;
			}

			for(int i = 0; i < names.Length; ++i) {
				callback?.Invoke(names[i]);
			}
		}

		public void eachItem(string path, Action<string> callback) {
			each(path, callback, true);
		}

		public void eachName(string path, Action<string> callback) {
			each(path, callback, false);
		}

		public void clearChild(string path) {
			RegistryKey root = null;
			string name = "";

			try {
				getPath(path, false, out root, out name);
				if(root == null) {
					return;
				}

				root.DeleteSubKeyTree(name);
			} catch(Exception) {
				//return "";
			}
			root?.Close();
		}

		public void clearChildItem(string path) {
			RegistryKey root = null;
			string[] names = null;

			try {
				root = getPath(path, false);
				if(root == null) {
					return;
				}

				names = root.GetSubKeyNames();

				for(int i = 0; i < names.Length; ++i) {
					root.DeleteSubKeyTree(names[i]);
				}

			} catch(Exception) {
				//return "";
			}
			root?.Close();

			//try {
			//	each(path, (name) => {

			//	});
			//} catch(Exception) {

			//}
		}

	}
}
