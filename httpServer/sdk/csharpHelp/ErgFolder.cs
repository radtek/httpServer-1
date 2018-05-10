using System.IO;

namespace csharpHelp.util.action {
	/// <summary>
	/// 遍历文件夹
	/// </summary>
	class ErgFolder {
		public delegate void Callback(string filePath);

		public Callback callback = null;
		public string path = "";

		public bool ignoreFolder = false;	//不输出目录
		public bool ignoreHideFile = false;	//不输出隐藏文件
		public bool isDeep = true;			//递归
		public void start() {
			ergFolder(path);
		}

		private void ergFolder(string path) {
			if(callback == null) {
				return;
			}

			string[] filenames = Directory.GetFileSystemEntries(path);

			// 遍历所有的文件和目录
			foreach(string file in filenames) {
				// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
				if(Directory.Exists(file)) {
					if(!ignoreFolder) {
						callback(file);
					}

					if(isDeep) {
						ergFolder(file);
					}
				} else {
					if(ignoreHideFile && (new FileInfo(file).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) {
						//忽略隐藏文件
						continue;
					}
					callback(file);
				}
			}
		}
	}
}
