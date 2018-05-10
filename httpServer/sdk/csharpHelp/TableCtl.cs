using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace csharpHelp.util.action {
	/// <summary>
	/// 表格类型数据
	/// </summary>
	public class TableCtl {
		private List<List<string>> data = new List<List<string>>();
		private Dictionary<string, int> mapCol = new Dictionary<string, int>();

		public TableCtl setData(List<List<string>> _data) {
			data = _data;
			updateCol();
			//mapCol.Clear();

			//if(data.Count != 0) {
			//	for(int i = 0; i < data[0].Count; ++i) {
			//		mapCol[data[0][i]] = i;
			//	}
			//}

			return this;
		}

		private void updateCol() {
			mapCol.Clear();
			if(data.Count != 0) {
				for(int i = 0; i < data[0].Count; ++i) {
					mapCol[data[0][i]] = i;
				}
			}
		}

		public void setHead(List<string> lstHead) {
			List<List<string>> temp = new List<List<string>>();
			temp.Add(new List<string>(lstHead.ToArray()));
			setData(temp);
		}

		public int Count {
			get { return data.Count; }
		}

		public void add(List<string> lstData) {
			data.Add(lstData);
		}

		public int addEmpty(string defData = "") {
			List<string> lstData = new List<string>();
			for(int i = 0; i < data[0].Count; ++i) {
				lstData.Add(defData);
			}

			add(lstData);
			return data.Count - 1;
		}

		public void setValue(int row, string head, string value) {
			int col = getColIdx(head);

			setValue(row, col, value);
		}

		public void setValue(int row, int col, string value) {
			data[row][col] = value;
		}

		public string getValue(int row, string head) {
			int col = getColIdx(head);
			return getValue(row, col);
		}

		public string getValue(int row, int col) {
			return data[row][col];
		}

		public int findRow(string head, string value) {
			int col = getColIdx(head);
			return findRow(col, value);
		}

		public int findRow(int col, string value) {
			if(col < 0) {
				return -1;
			}

			for(int i = 0; i < data.Count; ++i) {
				if(data[i][col] == value) {
					return i;
				}
			}
			return -1;
		}

		public List<List<string>> getData() {
			return data;
		}

		public static implicit operator TableCtl(List<List<string>> data) {
			return new TableCtl().setData(data);
		}

		public TableCtl clone(){
			TableCtl tb = new TableCtl();
			List<List<string>> newData = new List<List<string>>();

			for(int i = 0; i < data.Count; ++i) {
				newData.Add(new List<string>(data[i].ToArray()) );
			}
			tb.setData(newData);

			return tb;
		}
		
		public void combine(TableCtl tb, string[] pkey, bool isCover = true, bool isCoverOnly = false) {
			if(data.Count == 0) {
				setData(tb.clone().data);
				//updateCol();
			}

			if(pkey.Length == 0) {
				for(int i = 1; i < tb.data.Count; ++i) {
					data.Add(new List<string>(tb.data[i].ToArray()) );
				}
				return;
			}

			//get pkey head
			List<int> pHead = headToCol(new List<string>(pkey));
			Dictionary<int, int> mapHead = new Dictionary<int, int>();
			for(int i = 0; i < pHead.Count; ++i) {
				mapHead[pHead[i]] = 0;
			}

			//create index
			Dictionary<string, int> pkeyIndex = new Dictionary<string, int>();
			for(int i = 1; i < data.Count; ++i) {
				string name = "";
				for(int j = 0; j < pHead.Count; ++j) {
					name += data[i][pHead[j]];
				}
				pkeyIndex[name] = i;
			}

			//
			for(int i = 1; i < tb.data.Count; ++i) {
				string name = "";
				for(int j = 0; j < pHead.Count; ++j) {
					name += tb.data[i][pHead[j]];
				}

				//pkey相同
				if(pkeyIndex.ContainsKey(name)) {
					if(isCover) {
						//覆盖
						int idx = pkeyIndex[name];
						for(int j = 0; j < data[idx].Count; ++j) {
							data[idx][j] = tb.data[i][j];
						}
					}
					continue;
				} else {
					if(isCoverOnly) {
						continue;
					}
				}

				data.Add(new List<string>(tb.data[i]));
			}
		}

		public TableCtl loadTable(string path, string split = "\t") {
			List<List<string>> result = new List<List<string>>();

			if (!File.Exists(path)) {
				setData(result);
				return this;
			}

			//laod scene data
			StreamReader srScene = new StreamReader(path, Encoding.Default);
			string line = "";
			int colCount = 0;
			while ((line = srScene.ReadLine()) != null) {
				line = line.Trim();
				if (line == "") {
					continue;
				}
				//string[] arrLine = line.Split(split.ToCharArray());
				string[] arrLine = Regex.Split(line, split);

				//check format
				if (arrLine.Length == 0) {
					continue;
				}
				if (colCount == 0) {
					colCount = arrLine.Length;
				} else if (colCount != arrLine.Length) {
					continue;
				}

				//save data
				List<string> lstTemp = new List<string>();
				for (int i = 0; i < arrLine.Length; ++i) {
					lstTemp.Add(arrLine[i].Trim());
				}
				result.Add(lstTemp);
			}
			srScene.Close();

			setData(result);
			return this;
		}

		public TableCtl clear() {
			data = new List<List<string>>();
			mapCol.Clear();

			return this;
		}

		public List<string> getHead(List<List<string>> data) {
			if (data.Count == 0) {
				return new List<string>();
			}

			return data[0];
		}

		public List<string> getColData(string headName) {
			List<string> result = new List<string>();

			if(!mapCol.ContainsKey(headName)) {
				return result;
			}

			//int colIdx = getColIdx(data, headName);
			//if(colIdx < 0) {
			//	return result;
			//}

			int colIdx = mapCol[headName];

			for(int row = 1; row < data.Count; ++row) {
				result.Add(data[row][colIdx]);
			}
			return result;
		}

		public int getColIdx(string headName) {
			if(!mapCol.ContainsKey(headName)) {
				return -1;
			}

			return mapCol[headName];

			//if(data.Count == 0) {
			//	return -1;
			//}

			//int colIdx = -1;
			////int i = 0;
			//for(int i = 0; i < data[0].Count; ++i) {
			//	if(data[0][i] == headName) {
			//		colIdx = i;
			//		break;
			//	}
			//}

			//return colIdx;
		}

		public List<int> headToCol(List<string> lstHead) {
			List<int> result = new List<int>();

			for(int i = 0; i < lstHead.Count; ++i) {
				result.Add(getColIdx(lstHead[i]));
			}
			return result;

			//Dictionary<string, int> mapHead = new Dictionary<string, int>();
			//for(int i = 0; i < lstHead.Count; ++i) {
			//	mapHead[lstHead[i]] =  -1;
			//}

			//for(int i = 0; i < data[0].Count; ++i) {
			//	if(mapHead.ContainsKey(data[0][i])){
			//		mapHead[data[0][i]] = i;
			//	}
			//}

			//for(int i = 0; i < lstHead.Count; ++i) {
			//	result.Add(mapHead[lstHead[i]]);
			//}

			//return result;
		}

	}
}
