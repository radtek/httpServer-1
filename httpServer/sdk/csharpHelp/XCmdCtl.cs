using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace csharpHelp.util {
	public class XCmdCtl {

		private string xmlPath = "";

		public XmlCtl xml = new XmlCtl();
		public SetParser pSet = new SetParser();
		public Dictionary<string, XBaseParser> mapParser = new Dictionary<string, XBaseParser>();
		public string log = "";

		public XCmdCtl() {
			reg(pSet);
		}

		public void load(string _xmlPath) {
			xmlPath = _xmlPath;

			xml.load(xmlPath);
		}

		public void reg(XBaseParser parser) {
			mapParser[parser.name] = parser;
			parser.setCtl(this);
		}

		public void run() {
			_run(xml.child("xcmd"));
		}

		private void _run(XmlCtl node) {
			node.eachAllChild(node.name(), (idx, ctl) => {
				string name = ctl.name();
				if(mapParser.ContainsKey(name)) {
					mapParser[name].parse(ctl);
				}
			});
		}
	}

	public abstract class XBaseParser {
		public string name = "";
		public XCmdCtl ctl = null;
		public abstract void parse(XmlCtl node);

		public virtual void setCtl(XCmdCtl _ctl) {
			ctl = _ctl;
		}
	};

	public class SetParser : XBaseParser {
		Dictionary<string, string> mapVar = new Dictionary<string, string>();
		public override void parse(XmlCtl node) {
			mapVar[node.attr("name")] = cvt(node.value());
		}

		//public string cvt(string data) {
		//	return replaceVar(data);
		//}

		public string cvt(string data) {
			string result = data;
			try {
				string strReg = @"{[^}]*}";
				MatchCollection temp = Regex.Matches(data, strReg);
				for(int i = 0; i < temp.Count; i++) {
					string value = temp[i].Value;
					value = value.Substring(1, value.Length - 2);
					if(!mapVar.ContainsKey(value)) {
						continue;
					}

					//Debug.WriteLine(temp[i].Value);
					result = result.Replace(temp[i].Value, mapVar[value]);
				}
			} catch(Exception) {
				return data;
			}

			return result;
		}
	}

	public class CmdParser : XBaseParser {
		public CmdParser() {
			name = "cmd";
		}

		public override void parse(XmlCtl node) {
			string exe = ctl.pSet.cvt(node.attr("attr"));
			string param = ctl.pSet.cvt(node.value());
			runExe(exe, param);
		}

		//运行Exe
		private void runExe(string exePath, string param) {
			Process exep = new Process();
			exep.StartInfo.FileName = exePath;
			exep.StartInfo.Arguments = param;
			exep.StartInfo.CreateNoWindow = false;
			exep.StartInfo.UseShellExecute = false;
			exep.StartInfo.RedirectStandardOutput = false;
			exep.StartInfo.RedirectStandardError = false;
			//设置启动动作,确保以管理员身份运行
			exep.StartInfo.Verb = "runas";
			exep.Start();
			exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
			try {
				//string data = exep.StandardOutput.ReadToEnd();
				//data = exep.StandardError.ReadToEnd();
			} catch(Exception ex) {
				//Console.WriteLine(ex.ToString());
				ctl.log += "[CmdParser err]" + ex.ToString();
			}
			exep.Close();
		}
	};

}
