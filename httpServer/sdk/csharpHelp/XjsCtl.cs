using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace csharpHelp.util {
	public class XjsCtl {
		private string filePath = "";
		private Encoding ecd = null;

		//private string data = "";
		private List<string> lstData = new List<string>();
		public Dictionary<string, Func<List<string>, string>> mapParser = new Dictionary<string, Func<List<string>, string>>();

		Dictionary<string, string> mapVar = new Dictionary<string, string>();
		public XjsCtl() {
			regFunction("cvt", cvtParser);
		}

		public void load(string _filePath, Encoding _ecd = null) {
			if(_ecd == null) {
				_ecd = Encoding.Default;
			}

			filePath = _filePath;
			ecd = _ecd;

			lstData = new List<string>();

			StreamReader sw = new StreamReader(filePath);
			//data = sw.ReadToEnd();
			//string temp = "1";
			while(true) {
				string temp = sw.ReadLine();
				if(temp == null) {
					break;
				}

				temp = temp.Trim(new char[]{'\t',' ' });

				if(temp == "") {
					continue;
				}

				lstData.Add(temp);
			}
			sw.Close();
		}

		public void run() {
			List<List<string>> lstCmd = new List<List<string>>();
			for(int i = 0; i < lstData.Count; ++i) {
				splitStr(lstData[i], lstCmd);
			}

			for(int i = 0; i < lstCmd.Count; ++i) {
				parseGrammar(lstCmd[i]);
				for(int j = 0; j < lstCmd[i].Count; ++j) {
					Debug.Write(lstCmd[i][j] + ",");
				}
				Debug.WriteLine("");
			}
		}

		private string parseGrammar(List<string> data) {
			if(data.Count == 1) {
				return "";
			}

			if(data[0] == "var") {
				if(data.Count == 1 || data.Count == 3) {
					return "";
				}

				if(data.Count == 2) {
					mapVar[data[1]] = "";
					return "";
				}

				List<string> lstTemp = new List<string>();
				for(int i = 0; i < data.Count; ++i) {
					lstTemp.Add(data[i]);
				}

				return "";
			}

			//string status = "";
			for(int i = 0; i < data.Count; ++i) {

			}

			return "";
		}

		private void splitStr(string data, List<List<string>> lstCmd) {
			List<string> result = new List<string>();

			string status = "";
			string status1 = "";
			//string status2 = "";
			//string status3 = "";
			string temp = "";
			for(int i = 0; i < data.Length; ++i) {
				if(status == "strStart" && status1 == "\\") {
					status1 = "";
					temp += data[i];
					continue;
				}

				if(status == "strStart" && data[i] == '\\') {
					status1 = "\\";
					continue;
				}

				if(isStrStart(data[i])) {
					if(status == "strStart") {
						//end string
						result.Add(temp);
						status = "";
						temp = "";
					} else {
						//start string
						if(temp != "") {
							result.Add(temp);
							temp = "";
						}
						//temp = "\"";
						status = "strStart";
					}
					result.Add("\"");
					continue;
				}

				if(status == "strStart") {
					temp += data[i];
					continue;
				}

				if(data[i] == ';') {
					if(temp != "") {
						result.Add(temp);
						temp = "";
					}
					if(result.Count != 0) {
						lstCmd.Add(result);
						result = new List<string>();
					}
					continue;
				}

				if(data[i] == ' ' || data[i] == '\t') {
					if(temp != "") {
						result.Add(temp);
						temp = "";
					}
					continue;
				}

				//if(data[i] == '=') {
				//	if(temp != "") {
				//		result.Add(temp);
				//		temp = "";
				//	}
				//	result.Add("=");
				//	continue;
				//}

				//if(data[i] == '(' || data[i] == ')') {
				//	if(temp != "") {
				//		result.Add(temp);
				//		temp = "";
				//	}
				//	result.Add(data[i].ToString());
				//	continue;
				//}

				if(!isLatter(data[i])) {
					if(temp != "") {
						result.Add(temp);
						temp = "";
					}
					result.Add(data[i].ToString());
					continue;
				}

				//if(temp == "") {
				//	temp = "_";
				//}
				temp += data[i];
			}

			if(temp != "") {
				result.Add(temp);
				temp = "";
			}

			if(result.Count != 0) {
				lstCmd.Add(result);
				result = new List<string>();
			}

			return;
		}

		private bool isLatter(char ch) {
			if(ch >= 'a' && ch <= 'z') {
				return true;
			}
			if(ch >= 'A' && ch <= 'Z') {
				return true;
			}
			if(ch >= '0' && ch <= '9') {
				return true;
			}
			if(ch == '_') {
				return true;
			}
			if(ch == '$') {
				return true;
			}
			return false;
		}

		private bool isStrStart(char ch) {
			HashSet<char> hsCh = new HashSet<char>();
			hsCh.Add('\'');
			hsCh.Add('\"');
			hsCh.Add('`');
			return hsCh.Contains(ch);
		}

		public void regFunction(string name, Func<List<string>, string> args) {
			mapParser[name] = args;
		}

		public string cvtParser(List<string> args) {
			if(args.Count <= 0) {
				return "";
			}

			string data = args[0];
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
}
