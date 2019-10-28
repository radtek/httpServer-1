using csharpHelp.util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp {

	public class XmlSerialize {
		//public string rootXmlName = "";
		public XmlCtl _xml { get; set; } = null;
		public object data { get; set; } = null;

		//Dictionary<string, Func<string, object>> mapConvert = new Dictionary<string, Func<string, object>>();

		public XmlSerialize() {

		}

		public XmlSerialize(object _data, XmlCtl __xml = null) {
			data = _data;
			_xml = __xml;
		}

		public XmlSerialize(object _data, string xmlPath, Encoding encoding = null) {
			data = _data;
			_xml = new XmlCtl();
			_xml.load(xmlPath, encoding);
		}

		public XmlCtl xml {
			get { return _xml; }
			set { _xml = value; }
		}

		//private void addConvert<T>(Func<string, object> fun) {
		//	mapConvert[typeof(T).ToString()] = fun;
		//}

		//private void addConvert<TType>(Func<string, TType> fun) {
		//	mapConvert[typeof(TType).ToString()] = (string str) => fun(str);
		//}
		public void load(object _data, XmlCtl __xml = null) {
			data = _data;
			_xml = __xml;

			_load(data, "");
		}
		//}
		public void load(object _data, string xmlPath, Encoding encoding = null) {
			data = _data;
			_xml = new XmlCtl();
			_xml.load(xmlPath, encoding);

			_load(data, "");
		}

		/// <summary></summary>
		/// <param name="xml"></param>
		public void loadFromXml() {
			_load(data, "");
		}

		private void _load(object subData, string rootPath) {
			Type type = subData.GetType();

			//if(subData == data && type.IsDefined(typeof(XmlRoot))) {
			//	rootPath = type.GetCustomAttribute<XmlRoot>().path;
			//	//rootPath = path;
			//}
			if(type.IsDefined(typeof(XmlRoot))) {
				rootPath = formatPath(rootPath, type.GetCustomAttribute<XmlRoot>().path);
			}

			var arrFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			var arrProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			MemberInfo[] arr = arrFields.Cast<MemberInfo>().Concat(arrProps).ToArray();

			foreach(var mi in arr) {
				object val = getMemberValue(mi, subData);
				//Debug.WriteLine(pi.GetType() + "," + pi.Name);

				if(mi.IsDefined(typeof(XmlAttr), false)) {
					//attr
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlAttr>().path);
					setData(subData, mi, _xml.attr, path);
				} else if(mi.IsDefined(typeof(XmlValue), false)) {
					//value
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlValue>().path);
					setData(subData, mi, _xml.value, path);
				} else if(mi.IsDefined(typeof(XmlChild), false)) {
					//object
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlChild>().path);
					Type childType = getMemberType(mi);
					if(val == null) {
						val = Activator.CreateInstance(childType);
					}
					
					_load(val, path);
				} else if(mi.IsDefined(typeof(XmlListAttr), false)) {
					//list attr
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlListAttr>().path);
					setListAttr(subData, mi, path);
				} else if(mi.IsDefined(typeof(XmlListValue), false)) {
					//list value
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlListValue>().path);
					setListValue(subData, mi, path);
				} else if(mi.IsDefined(typeof(XmlListChild), false)) {
					//list value
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlListChild>().path);
					setListChild(subData, mi, path);
				} else if(mi.IsDefined(typeof(XmlMap), false)) {
					//map
					setMap(subData, mi, rootPath);
				}
			}
		}

		private string formatPath(string rootPath, string subPath) {
			if(rootPath != "" && subPath != "") {
				return rootPath + "." + subPath;
			}

			return rootPath + subPath;
		}

		private void setMemberValue(MemberInfo mi, object data, object value) {
			if(mi.MemberType == MemberTypes.Field) {
				(mi as FieldInfo).SetValue(data, value);
			} else if(mi.MemberType == MemberTypes.Property) {
				(mi as PropertyInfo).SetValue(data, value);
			}
		}

		private object getMemberValue(MemberInfo mi, object data) {
			object rst = null;
			if(mi.MemberType == MemberTypes.Field) {
				rst = (mi as FieldInfo).GetValue(data);
			} else if(mi.MemberType == MemberTypes.Property) {
				rst = (mi as PropertyInfo).GetValue(data);
			} else {
				return null;
			}

			return rst;
		}

		private Type getMemberType(MemberInfo mi) {
			Type rst = null;
			if(mi.MemberType == MemberTypes.Field) {
				rst = (mi as FieldInfo).FieldType;
			} else if(mi.MemberType == MemberTypes.Property) {
				rst = (mi as PropertyInfo).PropertyType;
			} else {
				return null;
			}

			return rst;
		}

		private object getData(Func<string, string, string> getDataFun, string path, Type type, string defValue = "") {
			string strValue = getDataFun(path, defValue);
			return convertData(strValue, type);
		}

		private void setListAttr(object data, MemberInfo mi, string path) {
			object val = getMemberValue(mi, data);

			Type type = getMemberType(mi);
			Type argType = getArgType(type);

			//parse root
			int idx = path.LastIndexOf(".");
			string rootPath = "";
			string subPath = path;
			if(idx >= 0) {
				rootPath = path.Substring(0, idx);
				subPath = path.Substring(idx + 1);
			}

			//IList<T>
			object lst = val;
			if(lst == null) {
				lst = Activator.CreateInstance(type);
				setMemberValue(mi, data, lst);
			}

			//clear
			var mtChear = type.GetMethod("Clear");
			mtChear.Invoke(lst, new object[0]);

			var mtAdd = type.GetMethod("Add", new Type[] { argType });

			//Debug.WriteLine("aa:" + rootPath + "," + subPath + "," + attrName);
			_xml.each(rootPath, (idx2, xml) => {
				var strValue = xml.attr(subPath);
				//Debug.WriteLine("111:" + xml.name() + "," + xml.attr(attrName));
				object obj = convertData(strValue, argType);
				//Debug.WriteLine("aa:" + obj.GetType());

				mtAdd.Invoke(lst, new object[] { obj });
			});
		}

		private void setListValue(object data, MemberInfo mi, string path) {
			object val = getMemberValue(mi, data);

			Type type = getMemberType(mi);
			Type argType = getArgType(type);

			//IList<T>
			object lst = val;
			if(lst == null) {
				lst = Activator.CreateInstance(type);
				setMemberValue(mi, data, lst);
			}

			//clear
			var mtChear = type.GetMethod("Clear");
			mtChear.Invoke(lst, new object[0]);

			var mtAdd = type.GetMethod("Add", new Type[] { argType });
			
			_xml.each(path, (idx2, xml) => {
				var strValue = xml.value();
				object obj = convertData(strValue, argType);

				mtAdd.Invoke(lst, new object[] { obj });
			});
		}

		private void setListChild(object data, MemberInfo mi, string path) {
			object val = getMemberValue(mi, data);

			Type type = getMemberType(mi);
			Type argType = getArgType(type);

			//IList<T>
			object lst = val;
			if(lst == null) {
				lst = Activator.CreateInstance(type);
				setMemberValue(mi, data, lst);
			}

			//clear
			var mtChear = type.GetMethod("Clear");
			mtChear.Invoke(lst, new object[0]);

			var mtAdd = type.GetMethod("Add", new Type[] { argType });

			List<XmlCtl> lstChild = _xml.children(path);
			for(int i = 0; i < lstChild.Count; ++i) {
				object item = Activator.CreateInstance(argType);
				_load(item, $"{path}[{i}]");
				mtAdd.Invoke(lst, new object[] { item });
			}
		}

		private void setMap(object data, MemberInfo mi, string rootPath) {
			XmlMap xmlAttr = mi.GetCustomAttribute<XmlMap>();

			object val = getMemberValue(mi, data);

			Type type = getMemberType(mi);
			Type keyType = null;
			Type valueType = null;
			if(type.IsGenericType) {
				//泛型
				Type[] arrType = type.GetGenericArguments();
				keyType = arrType[0];
				valueType = arrType[1];
			} else {
				var mtTemp = type.GetMethod("Add");
				keyType = mtTemp.GetParameters()[0].ParameterType;
				valueType = mtTemp.GetParameters()[1].ParameterType;
			}
			
			//IDictionary<T>
			object map = val;
			if(map == null) {
				map = Activator.CreateInstance(type);
				setMemberValue(mi, data, map);
			}

			//clear
			var mtChear = type.GetMethod("Clear");
			mtChear.Invoke(map, new object[0]);

			var mtAdd = type.GetMethod("Add", new Type[] { keyType, valueType });
			string path = formatPath(rootPath, xmlAttr.rootPath);

			List<XmlCtl> lstChild = _xml.children(formatPath(rootPath, xmlAttr.rootPath));
			for(int i = 0; i < lstChild.Count; ++i) {
				//key
				object objKey = null;
				string keyPath = formatPath(path + $"[{i}]", xmlAttr.keyPath);
				switch(xmlAttr.keyType) {
					case XmlKeyType.Attr: objKey = getData(xml.attr, keyPath, keyType); break;
					case XmlKeyType.Value: objKey = getData(xml.value, keyPath, keyType); break;
				}

				//value
				object objValue = null;
				string valuePath = formatPath(path + $"[{i}]", xmlAttr.valuePath);
				switch(xmlAttr.valueType) {
					case XmlValueType.Attr: objValue = getData(xml.attr, valuePath, valueType); break;
					case XmlValueType.Value: objValue = getData(xml.value, valuePath, valueType); break;
					case XmlValueType.Child: {
						objValue = Activator.CreateInstance(valueType);
						_load(objValue, valuePath);
						break;
					}
				}
				
				mtAdd.Invoke(map, new object[] { objKey, objValue });
			}
		}

		private Type getArgType(Type classType) {
			Type argType = null;
			if(classType.IsGenericType) {
				//泛型
				Type[] arrType = classType.GetGenericArguments();
				argType = arrType[0];
			} else {
				var mtTemp = classType.GetMethod("Add");
				argType = mtTemp.GetParameters()[0].ParameterType;
			}

			return argType;
		}

		private void setData(object data, MemberInfo mi, Func<string, string, string> getDataFun, string path) {
			Type type = getMemberType(mi);
			object val = getMemberValue(mi, data);
			if(val == null) {
				try {
					val = Activator.CreateInstance(type);
				} catch(Exception) { }
			}

			var strValue = getDataFun(path, val.ToString());

			//string strType = val.GetType().ToString();
			object rst = convertData(strValue, type, val);
			setMemberValue(mi, data, rst);
		}

		private object convertData(string data, Type type, object defValue = null) {
			if(type.IsEnum) {
				try {
					return Enum.Parse(type, data);
				} catch(Exception) { }
				return defValue;
			}

			return Convert.ChangeType(data, type);
		}

		public void saveToXml() {
			_saveToXml(data, "");
		}

		public void save() {
			_saveToXml(data, "");
			_xml.save();
		}

		private void _saveToXml(object subData, string rootPath) {
			Type type = subData.GetType();

			var arrFields = type.GetMembers(BindingFlags.Instance | BindingFlags.Public);
			var arrProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			MemberInfo[] arr = arrFields.Cast<MemberInfo>().Concat(arrProps).ToArray();

			//string rootPath = "";
			//if(subData == data && type.IsDefined(typeof(XmlRoot))) {
			//	//var attr = type.GetCustomAttribute<XmlPath>();
			//	rootPath = type.GetCustomAttribute<XmlRoot>().path;
			//	//rootPath = path;
			//}
			if(type.IsDefined(typeof(XmlRoot))) {
				rootPath = formatPath(rootPath, type.GetCustomAttribute<XmlRoot>().path);
			}

			foreach(var mi in arr) {
				//string name = pi.Name;
				object val = getMemberValue(mi, subData);
				//object val = pi.GetValue(subData);
				//Debug.WriteLine("bb:" + name + "," + val);

				if(mi.IsDefined(typeof(XmlAttr), false)) {
					//attr
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlAttr>().path);
					_xml.setAttr(path, val.ToString());
				} else if(mi.IsDefined(typeof(XmlValue), false)) {
					//value
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlValue>().path);
					_xml.setValue(path, val.ToString());
				} else if(mi.IsDefined(typeof(XmlChild), false)) {
					//child
					if(val != null) {
						string path = formatPath(rootPath, mi.GetCustomAttribute<XmlChild>().path);
						_saveToXml(val, path);
					}
				} else if(mi.IsDefined(typeof(XmlListAttr), false)) {
					//list attr
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlListAttr>().path);

					if(val != null && val is IEnumerable) {
						int idx = path.LastIndexOf(".");
						string rootPathTemp = "";
						string subPath = path;
						if(idx >= 0) {
							rootPathTemp = path.Substring(0, idx);
							subPath = path.Substring(idx + 1);
						}
						
						int i = 0;
						foreach(var item in val as IEnumerable) {
							string pathTemp = rootPathTemp + $"[{i}].{subPath}";
							_xml.setAttr(pathTemp, item.ToString());
							++i;
						}
						//remove out of range
						List<XmlCtl> lst = _xml.children(rootPathTemp);
						if(i < lst.Count) {
							for(int j = i; j < lst.Count; ++j) {
								_xml.removeInTarget(rootPathTemp, i);
							}
						}

					}
				} else if(mi.IsDefined(typeof(XmlListValue), false)) {
					//list value
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlListValue>().path);

					if(val != null && val is IEnumerable) {
						int i = 0;
						foreach(var item in val as IEnumerable) {
							string pathTemp = path + $"[{i}]";
							_xml.setValue(pathTemp, item.ToString());
							++i;
						}
						//remove out of range
						List<XmlCtl> lst = _xml.children(path);
						if(i < lst.Count) {
							for(int j = i; j < lst.Count; ++j) {
								_xml.removeInTarget(path, i);
							}
						}
					}
				} else if(mi.IsDefined(typeof(XmlListChild), false)) {
					//list child
					string path = formatPath(rootPath, mi.GetCustomAttribute<XmlListChild>().path);

					if(val != null && val is IEnumerable) {
						int i = 0;
						foreach(var item in val as IEnumerable) {
							string pathTemp = path + $"[{i}]";
							_saveToXml(item, pathTemp);
							++i;
						}
						//remove out of range
						List<XmlCtl> lst = _xml.children(path);
						if(i < lst.Count) {
							for(int j = i; j < lst.Count; ++j) {
								_xml.removeInTarget(path, i);
							}
						}
					}
				}
			}
		}

	}

	public class XmlBaseAttribute : Attribute {
		public string path = "";

		public XmlBaseAttribute(string _path) {
			path = _path;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class XmlRoot : XmlBaseAttribute {
		public XmlRoot(string _path = "") : base(_path) { }
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XmlAttr : XmlBaseAttribute {
		public XmlAttr(string _path) : base(_path) { }
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XmlValue : XmlBaseAttribute {
		public XmlValue(string _path) : base(_path) { }
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XmlChild : XmlBaseAttribute {
		public XmlChild(string _path = "") : base(_path) { }
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XmlListAttr : XmlBaseAttribute {
		public XmlListAttr(string _path) : base(_path) { }
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XmlListValue : XmlBaseAttribute {
		public XmlListValue(string _path) : base(_path) { }
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XmlListChild : XmlBaseAttribute {
		public XmlListChild(string _path = "") : base(_path) { }
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XmlMap : Attribute {
		public string rootPath = "";

		public XmlKeyType keyType = XmlKeyType.Attr;
		public string keyPath = "";

		public XmlValueType valueType = XmlValueType.Attr;
		public string valuePath = "";

		public XmlMap(string _rootPath, XmlKeyType _keyType, string _keyPath, XmlValueType _valueType, string _valuePath) {
			rootPath = _rootPath;
			keyType = _keyType;
			keyPath = _keyPath;
			valueType = _valueType;
			valuePath = _valuePath;
		}
	}

	public enum XmlKeyType { Attr, Value };
	public enum XmlValueType { Attr, Value, Child };

	//public class XmlUnknownTypeException : Exception {
	//	public Type unkownType = null;

	//	public XmlUnknownTypeException() { }

	//	public XmlUnknownTypeException(Type _type) : base("unkown type " + _type.ToString()) {
	//		unkownType = _type;
	//	}
	//}
}
