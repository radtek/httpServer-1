using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace csharpHelp.util {
	/// <summary>
	/// Xml操作
	/// </summary>
	public class XmlCtl {
		public XmlDocument root = new XmlDocument();
		//private XmlNode firstDoc = null;
		public XmlNode doc = null;
		public string path = "";

		public XmlCtl() { }

		public XmlCtl load(string _path, Encoding encoding = null) {
			Encoding loadEncoding = (encoding == null ? Encoding.UTF8 : encoding);

			path = _path;
			doc = null;

			if(!File.Exists(path)) {
				root = new XmlDocument();
				return this;
			}

			//root = new XmlDocument();
			//root.Load(path);
			loadXml(File.ReadAllText(path, loadEncoding));

			//root.SelectSingleNode("");

			//XmlNodeList lst = root.ChildNodes;
			//for(int i = 0; i < lst.Count; ++i) {
			//	if(lst[i].NodeType == XmlNodeType.Element) {
			//		doc = lst[i];
			//		//firstDoc = lst[i];
			//		break;
			//	}
			//}

			//doc = data.FirstChild;

			return this;
		}

		public void save(string _path, Encoding encoding) {
			string savePath = _path == "" ? path : _path;
			Encoding saveEncoding = encoding == null ? Encoding.UTF8 : encoding;
			//Encoding saveEncoding = encoding == null ? new UTF8Encoding() : encoding;

			string dir = Path.GetDirectoryName(savePath);
			if(dir != "" && !Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.OmitXmlDeclaration = true;
			settings.NewLineChars = "\r\n";
			settings.NewLineHandling = NewLineHandling.Replace;
			using(MemoryStream memstream = new MemoryStream())
			using(StreamWriter sr = new StreamWriter(memstream, saveEncoding))
			using(XmlWriter writer = XmlWriter.Create(sr, settings))
			using(FileStream fileWriter = new FileStream(savePath, FileMode.Create)) {
				//Debug.WriteLine(root.ChildNodes[0].NodeType);
				if(root.ChildNodes.Count > 0 && root.ChildNodes[0] is XmlProcessingInstruction) {
					root.RemoveChild(root.ChildNodes[0]);
				}
				// save xml to XmlWriter made on encoding-specified text writer
				root.Save(writer);
				// Flush the streams (not sure if this is really needed for pure mem operations)
				writer.Flush();
				// Write the underlying stream of the XmlWriter to file.
				fileWriter.Write(memstream.GetBuffer(), 0, (Int32)memstream.Length);
			}

			//root.Save(path);
		}

		public void save() {
			save(path, Encoding.UTF8);
		}

		public void save(string _path) {
			save(_path);
		}

		public void save(Encoding encoding) {
			save(path, encoding);
		}

		public XmlCtl loadXml(string xml) {
			root = new XmlDocument();
			root.LoadXml(xml);
			doc = null;
			//doc = data.FirstChild;

			//XmlNodeList lst = root.ChildNodes;
			//for(int i = 0; i < lst.Count; ++i) {
			//	if(lst[i].NodeType == XmlNodeType.Element) {
			//		doc = lst[i];
			//		//firstDoc = lst[i];
			//		break;
			//	}
			//}

			return this;
		}

		public List<XmlCtl> children(string strNode) {
			List<XmlCtl> result = new List<XmlCtl>();
			int idx = strNode.LastIndexOf(".");
			XmlCtl xml = this;
			if(idx >= 0) {
				xml = child(strNode.Substring(0, idx));
			}

			string name = strNode.Substring(idx + 1);
			XmlNodeList xLst = xml.doc.SelectNodes(name);

			for(int i = 0; i < xLst.Count; ++i) {
				XmlCtl temp = copySelf();
				temp.doc = xLst[i];
				result.Add(temp);
			}

			return result;

			//XmlNodeList lis = doc.GetElementsByTagName("Name");

			//return this;
		}

		public XmlCtl child(string strNode) {
			if(strNode == "") {
				return this;
			}

			string[] arr = strNode.Split('.');
			List<string> lstNode = new List<string>(arr);

			XmlNode temp = null;
			if(doc == null) {
				temp = root.SelectSingleNode(lstNode[0]);
				if(temp == null) {
					XmlElement ele = root.CreateElement(lstNode[0]);
					root.AppendChild(ele);
					temp = ele;
				}
				lstNode.RemoveAt(0);
			} else {
				temp = doc;
			}

			for(int i = 0; i < lstNode.Count; ++i) {
				string strOneNode = lstNode[i].Trim();
				if(strOneNode == "") {
					return this;
				}

				if(strOneNode[strOneNode.Length - 1] == ']') {
					int idx = strOneNode.IndexOf('[');
					if(idx <= 0) {
						return this;
					}

					//get name
					string eleName = strOneNode.Substring(0, idx);

					//get index
					int nodeIdx = 0;
					if(!Int32.TryParse(strOneNode.Substring(idx + 1, strOneNode.Length - 1 - idx - 1), out nodeIdx)) {
						return this;
					}

					if(nodeIdx < 0) {
						return this;
					}

					//get lst
					XmlNodeList xLst = temp.SelectNodes(eleName);
					if(xLst == null || nodeIdx >= xLst.Count) {
						int newIdx = 0;
						if(xLst == null || xLst.Count <= 0) {
							newIdx = 0;
						} else {
							newIdx = xLst.Count;
						}
						for(int j = newIdx; j < nodeIdx + 1; ++j) {
							XmlElement ele = root.CreateElement(eleName);
							temp.AppendChild(ele);
							if(j == nodeIdx) {
								temp = ele;
							}
						}
						//return this;
					} else {
						temp = xLst[nodeIdx];
					}
				} else {
					XmlNodeList xLst = temp.SelectNodes(strOneNode);
					if(xLst == null || xLst.Count <= 0) {
						XmlElement ele = root.CreateElement(strOneNode);
						temp.AppendChild(ele);
						temp = ele;
						//return this;
					} else {
						temp = xLst[0];
					}

				}
			}

			if(temp == doc) {
				return this;
			}

			XmlCtl result = copySelf();
			result.doc = temp;
			return result;

			//XmlNodeList lis = doc.GetElementsByTagName("Name");

			//return this;
		}

		//public XmlCtl next() {
		//	return this;
		//}

		public string name(string strNode = "") {
			if(strNode == "") {
				try {
					XmlElement eleNode = (XmlElement)doc;
					return eleNode.Name;
				} catch(Exception) {

				}

				return "";
			} else {
				return child(strNode).name();
			}
		}

		public string value(string strNode = "", string initData = "") {
			string data = "";
			if(strNode == "") {
				data = doc.InnerText;
			} else {
				data = child(strNode).value("", initData);
			}

			if(data == "") {
				setValue(strNode, initData);
				return initData;
			}

			return data;

			//return doc.InnerText;

			//try {
			//	return doc.InnerText;
			//} catch(Exception) {

			//}

			//return "";
		}

		public double valueDouble(string strNode, double initData = 0) {
			double result = 0;
			bool isOk = double.TryParse(value(strNode), out result);
			if(!isOk) {
				return initData;
			}
			return result;
		}

		public int valueInt(string strNode, int initData = 0) {
			int result = 0;
			bool isOk = int.TryParse(value(strNode), out result);
			if(!isOk) {
				return initData;
			}
			return result;
		}

		public Boolean valueBool(string strNode, bool initData = false) {
			string data = value(strNode);
			if(data == "") {
				return initData;
			}
			return data == "true";
		}

		public string attr(string strNode, string initData = "") {
			string result = "";
			try {
				var idx = strNode.LastIndexOf('.');
				if(idx < 0) {
					if(doc != null && doc.NodeType == XmlNodeType.Element) {
						XmlElement eleNode = (XmlElement)doc;
						if(!eleNode.HasAttribute(strNode)) {
							return initData;
						} else {
							result = eleNode.GetAttribute(strNode);
						}
						return result;
					}
				} else {
					XmlCtl ele = child(strNode.Substring(0, idx));
					result = ele.attr(strNode.Substring(idx + 1), initData);
				}
			} catch(Exception) {
				setAttr(strNode, initData);
				return initData;
			}

			if(result == "") {
				return initData;
			}
			return result;
		}

		public double attrDouble(string strNode, double initData = 0) {
			double result = 0;
			bool isOk = double.TryParse(attr(strNode), out result);
			if(!isOk) {
				return initData;
			}
			return result;
		}

		public int attrInt(string strNode, int initData = 0) {
			int result = 0;
			bool isOk = int.TryParse(attr(strNode), out result);
			if(!isOk) {
				return initData;
			}
			return result;
		}

		public Boolean attrBool(string strNode, bool initData = false) {
			string data = attr(strNode);
			if(data == "") {
				return initData;
			}
			return data == "true";
		}

		public void setAttr(string strNode, string attr) {
			try {
				var idx = strNode.LastIndexOf('.');
				if(idx < 0) {
					XmlElement eleNode = (XmlElement)doc;
					if(eleNode.HasAttribute(strNode)) {
						eleNode.SetAttribute(strNode, attr);
					} else {
						XmlAttribute attrNode = root.CreateAttribute(strNode);
						attrNode.Value = attr;
						eleNode.SetAttributeNode(attrNode);
					}
				} else {
					//createChild(strNode.Substring(0, idx));
					child(strNode.Substring(0, idx)).setAttr(strNode.Substring(idx + 1), attr);
				}
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void setAttrDouble(string strNode, double attr) {
			setAttr(strNode, attr.ToString());
		}

		public void setAttrInt(string strNode, int attr) {
			setAttr(strNode, attr.ToString());
		}

		public void setAttrBool(string strNode, bool attr) {
			setAttr(strNode, attr ? "true" : "false");
		}

		//public void setValue(string attr) {
		//	try {
		//		doc.InnerText = attr;
		//	} catch(Exception ex) {
		//		Debug.WriteLine(ex.ToString());
		//	}
		//}

		public void setValue(string strNode, string attr) {
			try {

				if(strNode == "") {
					//Debug.WriteLine(doc.InnerXml);
					//doc.InnerText = attr;
					doc.InnerXml = null;
					if(attr == "") {
					} else {
						doc.InnerText = attr;
					}
				} else {
					//createChild(strNode);
					child(strNode).setValue("", attr);
				}

				//var idx = strNode.LastIndexOf('.');
				//if(idx < 0) {
				//	XmlElement eleNode = (XmlElement)doc;
				//	eleNode.Value = attr;
				//	if(eleNode.HasAttribute(strNode)) {
				//		eleNode.SetAttribute(strNode, attr);
				//	} else {
				//		XmlAttribute attrNode = root.CreateAttribute(strNode);
				//		attrNode.Value = attr;
				//		eleNode.SetAttributeNode(attrNode);
				//	}
				//} else {
				//	createChild(strNode.Substring(0, idx));
				//	child(strNode.Substring(0, idx)).setAttr(strNode.Substring(idx + 1), attr);
				//}
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public XmlCtl removeInTarget(string strNode, int removeIdx = 0, int removeCount = 1) {
			try {
				int idx = strNode.LastIndexOf(".");
				XmlCtl xml = this;
				if(idx >= 0) {
					xml = child(strNode.Substring(0, idx));
				}

				string name = strNode.Substring(idx + 1);
				XmlNodeList xLst = xml.doc.SelectNodes(name);

				if(removeIdx < 0 || removeIdx >= xLst.Count) {
					return this;
				}

				int count = Math.Max(xLst.Count, removeIdx + removeCount);
				for(int i = removeIdx; i < count; ++i) {
					xml.doc.RemoveChild(xLst[i]);
				}
				//xml.doc.RemoveChild(xLst[removeIdx]);
			} catch(Exception) {

			}
			return this;
		}

		public XmlCtl removeInAllChild(string strNode, int removeIdx = 0, int removeCount = 1) {
			try {
				XmlCtl ele = child(strNode);
				int idx = 0;
				List<XmlNode> lstRemove = new List<XmlNode>();
				for(int i = 0; i < ele.doc.ChildNodes.Count; ++i) {
					if(ele.doc.ChildNodes[i].NodeType != XmlNodeType.Element) {
						continue;
					}

					if(idx >= removeIdx && idx < removeIdx + removeCount) {
						lstRemove.Add(ele.doc.ChildNodes[i]);
					}

					//if(idx == removeIdx) {
					//	ele.doc.RemoveChild(ele.doc.ChildNodes[i]);
					//	break;
					//}
					++idx;
				}
				for(int i = 0; i < lstRemove.Count; ++i) {
					ele.doc.RemoveChild(lstRemove[i]);
				}
			} catch(Exception) {

			}
			return this;
		}

		private void createChild(string strNode) {
			child(strNode);
			//string[] arr = strNode.Split('.');
			//List<string> lstNode = new List<string>(arr);

			//XmlCtl xml = this;
			//int idx = 0;
			//for(idx = 0; idx < lstNode.Count; ++idx) {
			//	XmlCtl temp = xml.child(lstNode[idx]);
			//	if(temp == xml) {
			//		break;
			//	}
			//	xml = temp;
			//}

			//XmlNode node = xml.doc;
			//for(; idx < lstNode.Count; ++idx) {
			//	XmlElement ele = root.CreateElement(lstNode[idx]);
			//	if(node == null) {
			//		root.AppendChild(ele);
			//	} else {
			//		node.AppendChild(ele);
			//	}
			//	node = ele;
			//}
		}

		public XmlCtl each(string strNode, Action<int, XmlCtl> fun) {
			try {
				List<XmlCtl> lstEle = children(strNode);
				//XmlCtl ele = child(strNode);
				for(int i = 0; i < lstEle.Count; ++i) {
					fun?.Invoke(i, lstEle[i]);
				}
			} catch(Exception) {

			}
			return this;
		}

		public XmlCtl eachAllChild(string strNode, Action<int, XmlCtl> fun) {
			try {
				XmlCtl ele = child(strNode);
				for(int i = 0; i < ele.doc.ChildNodes.Count; ++i) {
					if(ele.doc.ChildNodes[i].NodeType != XmlNodeType.Element) {
						continue;
					}

					XmlCtl eleChild = copySelf();
					eleChild.doc = ele.doc.ChildNodes[i];
					//Debug.WriteLine(ele.doc.ChildNodes[i].NodeType);
					fun?.Invoke(i, eleChild);
				}
			} catch(Exception) {

			}
			return this;
		}

		private XmlCtl copySelf() {
			XmlCtl xml = new XmlCtl();
			xml.doc = doc;
			xml.path = path;
			xml.root = root;
			//xml.firstDoc = firstDoc;

			return xml;
		}

	}
}
