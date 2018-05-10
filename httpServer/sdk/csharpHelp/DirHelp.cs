using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace csharpHelp.util {
	/// <summary>
	/// 遍历文件夹
	/// </summary>
	public class DirHelp {
		public enum Operate {
			/// <summary>忽略目录</summary>
			igFolder = 0x1,
			/// <summary>忽略文件</summary>
			igFile = 0x10,
			/// <summary>忽略隐藏文件</summary>
			igHideFile = 0x100,
			/// <summary>递归</summary>
			isDeep = 0x1000
		};
		public string path = "";

		public bool ignoreFolder = false;	//不输出目录
		public bool ignoreFile = false;		//不输出文件
		public bool ignoreHideFile = false;	//不输出隐藏文件
		public bool isDeep = false;         //递归
		public string regex = "";			//

		public DirHelp setPath(string _path) {
			path = _path;
			return this;
		}

		public DirHelp setIgnoreFolder(bool isTrue) {
			ignoreFolder = isTrue;
			return this;
		}

		public DirHelp setIgnoreFile(bool isTrue) {
			ignoreFile = isTrue;
			return this;
		}

		public DirHelp setIgnoreHideFile(bool isTrue) {
			ignoreHideFile = isTrue;
			return this;
		}

		public DirHelp setIsDeep(bool isTrue) {
			ignoreHideFile = isTrue;
			return this;
		}

		/// <summary>正则表达式匹配</summary>
		public DirHelp setRegex(string _regex) {
			regex = _regex;
			regex = regex.Replace(@"\/", @"\\");
			return this;
		}

		/// <summary>
		/// 通配符匹配,例子: C:/user/*.txt, C:/user/**/*.txt
		/// </summary>
		public DirHelp setWildcard(string wildcard) {
			regex = wildcard + "$";
			regex = regex.Replace(@"\", @"\\");
			regex = regex.Replace(@"/", @"\\");
			regex = regex.Replace(@".", @"\.");
			regex = regex.Replace(@"**", @".*?");
			//regex = regex.Replace(@"*", @"[^\\]*");
			//Debug.WriteLine(regex);
			//Debug.WriteLine(Regex.Match(regex, @"\*(?=[^\?])"));
			regex = Regex.Replace(regex, @"\*(?=[^\?])", @"[^\\]*");
			//Debug.WriteLine(regex);
			return this;
		}

		public DirHelp setFlag(Operate flag) {
			int iFlag = (int)flag;
			if((flag & Operate.igFolder) == Operate.igFolder) {
				ignoreFolder = true;
			}
			if((flag & Operate.igFile) == Operate.igFile) {
				ignoreFile = true;
			}
			if((flag & Operate.igHideFile) == Operate.igHideFile) {
				ignoreHideFile = true;
			}
			if((flag & Operate.isDeep) == Operate.isDeep) {
				isDeep = true;
			}
			return this;
		}

		public DirHelp resetFlag(Operate flag) {
			int iFlag = (int)flag;
			if((flag & Operate.igFolder) == Operate.igFolder) {
				ignoreFolder = false;
			}
			if((flag & Operate.igFile) == Operate.igFile) {
				ignoreFile = false;
			}
			if((flag & Operate.igHideFile) == Operate.igHideFile) {
				ignoreHideFile = false;
			}
			if((flag & Operate.isDeep) == Operate.isDeep) {
				isDeep = false;
			}

			return this;
		}

		public DirHelp each(Action<string> callback) {
			//this.path = path;
			return each(path, callback);
		}

		public List<string> files() {
			List<string> rst = new List<string>();

			return rst;
		}
		public DirHelp each(string path, Action<string> callback) {
			path = Path.GetFullPath(path);
			return _each(path, callback);
		}

		private DirHelp _each(string path, Action<string> callback) {
			if(callback == null) {
				return this;
			}

			string[] filenames = Directory.GetFileSystemEntries(path);

			// 遍历所有的文件和目录
			foreach(string file in filenames) {
				// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
				if(Directory.Exists(file)) {
					//不忽略目录
					if(!ignoreFolder) {
						if(regex != "") {
							//正则表达式匹配
							if(Regex.IsMatch(file, regex)) {
								callback(file);
							}
						} else {
							callback(file);
						}
					}

					//递归
					if(isDeep) {
						_each(file, callback);
					}
				} else {
					//忽略文件
					if(ignoreFile) {
						continue;
					}

					if(ignoreHideFile && (new FileInfo(file).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) {
						//忽略隐藏文件
						continue;
					}

					if(regex != "") {
						//正则表达式匹配
						if(Regex.IsMatch(file, regex)) {
							callback(file);
						}
					} else {
						callback(file);
					}
				}
			}
			return this;
		}

	}
}
