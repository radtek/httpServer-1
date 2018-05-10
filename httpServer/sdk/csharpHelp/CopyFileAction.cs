using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualBasic.FileIO;

namespace csharpHelp.util {
	/// <summary>
	/// 复制文件
	/// </summary>
	public class CopyFileAction {
		public bool isSync = false;				//是否同步文件，不同的目的文件夹多余的文件将删除，
		public bool isCompareTime = false;		//是否比较时间，只有时间不同的文件才复制
		public bool isDeleteToRecycle = false;  //是否删除到回收站
		public string logInfo = "";

		public void copy(string srcPath, string dstPath, List<string> filter = null) {
			try {
				if(File.Exists(srcPath)) {
					//复制文件
					copyFile(srcPath, dstPath);
					//Console.WriteLine("[copy file to] " + dstPath);
				} else if(Directory.Exists(srcPath)) {

					List<string> fileFilter = null;
					List<string> folderFilter = null;
					if(filter != null) {
						fileFilter = new List<string>();
						folderFilter = new List<string>();

						for(int i = 0; i < filter.Count; ++i){
							string path = Path.GetFullPath(srcPath + "/" + filter[i]).TrimEnd('\\');
							if(File.Exists(path)) {
								fileFilter.Add(path);
							} else if(Directory.Exists(path)) {
								folderFilter.Add(path);
							}
						}
					}

					srcPath = Path.GetFullPath(srcPath).TrimEnd('\\');
					dstPath = Path.GetFullPath(dstPath).TrimEnd('\\');
					copyDirectory(srcPath, dstPath, fileFilter, folderFilter);
					//Console.WriteLine("[copy folder to] " + dstPath);

					if(isSync) {
						syncDelete(srcPath, dstPath, fileFilter, folderFilter);
					}
				} else {
					//Console.WriteLine("[file or folder not exist] " + srcPath);
				}

			}catch(Exception ex){
				//Console.WriteLine(ex.StackTrace);
				//Console.WriteLine("[copy failed] src=" + srcPath + ", dst=" + dstPath + ex.ToString());
				logInfo += "[copy failed] src=" + srcPath + ", dst=" + dstPath + ex.ToString() + "\r\n";
			}
		}

		//删除目的文件夹不同步的文件
		private void syncDelete(string srcPath, string dstPath, List<string> fileFilter = null, List<string> folderFilter = null) {
			if(!Directory.Exists(dstPath)) {
				return;
			}

			DirectoryInfo TheFolder = new DirectoryInfo(dstPath);

			//遍历文件
			foreach(FileInfo NextFile in TheFolder.GetFiles()) {
				//Console.WriteLine(gap + NextFile.Name);
				string dstFile = dstPath + @"\" + NextFile.Name;
				if(fileFilter != null) {
					//过滤文件
					bool isFilter = false;
					for(int i = 0; i < fileFilter.Count; ++i) {
						if(dstFile == fileFilter[i]) {
							isFilter = true;
							break;
						}
					}
					if(isFilter) {
						continue;
					}
				}
				//copy
				//copyFile(srcFile, dstPath + @"\" + NextFile.Name);
				if(!File.Exists(srcPath + @"\" + NextFile.Name)) {
					delToRecycle(dstFile);
					//Console.WriteLine("[delete file to] " + dstFile);
				}
			}

			//遍历子目录
			foreach(DirectoryInfo NextFolder in TheFolder.GetDirectories()) {
				string subPath = dstPath + @"\" + NextFolder.Name;
				if(folderFilter != null) {
					//过滤文件夹
					bool isFilter = false;
					for(int i = 0; i < folderFilter.Count; ++i) {
						if(subPath == folderFilter[i]) {
							isFilter = true;
							break;
						}
					}
					if(isFilter) {
						continue;
					}
				}

				if(!Directory.Exists(srcPath + @"\" + NextFolder.Name)) {
					delToRecycle(subPath);
					//Console.WriteLine("[delete folder to] " + subPath);
				} else {
					//copy
					syncDelete(srcPath + @"\" + NextFolder.Name, dstPath + @"\" + NextFolder.Name, fileFilter, folderFilter);
				}
			}
		}

		//删除到回收站
		private void delToRecycle(string filePath) {
			if(File.Exists(filePath)) {
				//删除文件
				if(isDeleteToRecycle) {
					FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
				} else {
					File.Delete(filePath);
				}
			} else if(Directory.Exists(filePath)) {
				//删除文件夹
				if(isDeleteToRecycle) {
					FileSystem.DeleteDirectory(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
				} else {
					Directory.Delete(filePath);
				}
			}
		}

		/// <summary>
		/// 复制文件
		/// </summary>
		/// <param name="srcPath"></param>
		/// <param name="dstPath"></param>
		private void copyFile(string srcPath, string dstPath) {
			string dstFileName = Path.GetFullPath(dstPath).TrimEnd('\\');
			if(Directory.Exists(dstPath)) {
				dstFileName = dstPath + "/" + Path.GetFileName(srcPath);
			} else {
				string dir = Path.GetDirectoryName(dstFileName);
				if(dir != "") {
					try {
						Directory.CreateDirectory(dir);
					} catch(Exception) {
					
					}
				}
			}

			if(isCompareTime) {
				//比较修改时间进行更新
				if(File.Exists(dstFileName)) {
					FileInfo srcInfo = new FileInfo(srcPath);
					FileInfo dstInfo = new FileInfo(dstFileName);
					if(srcInfo.LastWriteTime.CompareTo(dstInfo.LastWriteTime) != 0) {
						//Console.WriteLine("[time:]" + srcPath);
						File.Copy(srcPath, dstFileName, true);
					}
				} else {
					File.Copy(srcPath, dstFileName, true);
				}
			} else {
				File.Copy(srcPath, dstFileName, true);
			}
		}

		/// <summary>
		/// 复制文件夹
		/// </summary>
		/// <param name="srcPath"></param>
		/// <param name="dstPath"></param>
		/// <param name="fileFilter"></param>
		/// <param name="folderFilter"></param>
		private void copyDirectory(string srcPath, string dstPath, List<string> fileFilter = null, List<string> folderFilter = null) {
			if(!Directory.Exists(dstPath)) {
				Directory.CreateDirectory(dstPath);
			}

			DirectoryInfo TheFolder = new DirectoryInfo(srcPath);

			//遍历文件
			foreach(FileInfo NextFile in TheFolder.GetFiles()) {
				//Console.WriteLine(gap + NextFile.Name);
				string srcFile = srcPath + @"\" + NextFile.Name;
				if(fileFilter != null) {
					//过滤文件
					bool isFilter = false;
					for(int i = 0; i < fileFilter.Count; ++i) {
						if(srcFile == fileFilter[i]) {
							isFilter = true;
							break;
						}
					}
					if(isFilter) {
						continue;
					}
				}
				//copy
				copyFile(srcFile, dstPath + @"\" + NextFile.Name);
			}

			//遍历子目录
			foreach(DirectoryInfo NextFolder in TheFolder.GetDirectories()) {
				string subPath = srcPath + @"\" + NextFolder.Name;
				if(folderFilter != null) {
					//过滤文件夹
					bool isFilter = false;
					for(int i = 0; i < folderFilter.Count; ++i) {
						if(subPath == folderFilter[i]) {
							isFilter = true;
							break;
						}
					}
					if(isFilter) {
						continue;
					}
				}
				//copy
				copyDirectory(srcPath + @"\" + NextFolder.Name, dstPath + @"\" + NextFolder.Name, fileFilter, folderFilter);
			}

		}

	}
}
