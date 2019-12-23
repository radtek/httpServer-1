using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp {
	public enum LinkType {
		Public, All, Private
	}

	public class ClassLink {
		public object from { get; set; } = null;
		public object to { get; set; } = null;
		public string classTargetName { get; set; } = "";
		public LinkType type { get; set; } = LinkType.Public;

		public ClassLink() {

		}

		public ClassLink(object _from, object _to, string _classTargetName = "", LinkType _type = LinkType.Public) {
			from = _from;
			to = _to;
			classTargetName = _classTargetName;
			type = _type;
		}

		public void sendTo(object _from, object _to, string _classTargetName = "", LinkType _type = LinkType.Public) {
			from = _from;
			to = _to;
			classTargetName = _classTargetName;
			type = _type;

			_sendTo(from);
		}

		public void sendTo() {
			_sendTo(from);
		}

		private BindingFlags getFlag() {
			switch(type) {
				case LinkType.Private: return BindingFlags.NonPublic | BindingFlags.Instance;
				case LinkType.All: return BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
				case LinkType.Public: default: return BindingFlags.Public | BindingFlags.Instance;
			}
		}

		private void _sendTo(object subData) {
			Type type = subData.GetType();

			BindingFlags bindFlag = getFlag();

			var arrFields = type.GetFields(bindFlag);
			var arrProps = type.GetProperties(bindFlag);
			MemberInfo[] arr = arrFields.Cast<MemberInfo>().Concat(arrProps).ToArray();

			foreach(var mi in arr) {

				if(!mi.IsDefined(typeof(Bridge), false)) {
					continue;
				}

				var arrBrg = mi.GetCustomAttributes<Bridge>();
				foreach(var brg in arrBrg) {
					if(brg.classTargetName != classTargetName) {
						continue;
					}

					string path = brg.targetPath;
					object dataTemp = to;

					string[] arrPath = path.Split('.');
					if(arrPath.Length <= 0) {
						continue;
					}

					bool nofound = false;
					for(int i = 0; i < arrPath.Length - 1; ++i) {
						string str = arrPath[i];
						var member = dataTemp.GetType().GetMember(str, bindFlag).FirstOrDefault();
						if(member == null) {
							nofound = true;
							break;
						}
						dataTemp = getMemberValue(member, dataTemp);
					}
					if(nofound) {
						continue;
					}

					var memberLast = dataTemp.GetType().GetMember(arrPath.Last(), bindFlag).FirstOrDefault();

					object val = getMemberValue(mi, subData);
					try {
						var newVal = Convert.ChangeType(val, getMemberValue(memberLast, dataTemp).GetType());
						setMemberValue(memberLast, dataTemp, newVal);
					} catch(Exception) {
						try {
							setMemberValue(memberLast, dataTemp, val);
						} catch(Exception) { }
					}

					//Debug.WriteLine("cc:" + val.GetType() + "," + path + "," + getMemberValue(mi, data));
				}
			}
		}

		public void sendBack() {
			_sendBack(from);
		}

		private void _sendBack(object subData) {
			Type type = subData.GetType();

			BindingFlags bindFlag = getFlag();

			var arrFields = type.GetFields(bindFlag);
			var arrProps = type.GetProperties(bindFlag);
			MemberInfo[] arr = arrFields.Cast<MemberInfo>().Concat(arrProps).ToArray();

			foreach(var mi in arr) {
				object val = getMemberValue(mi, subData);

				if(!mi.IsDefined(typeof(Bridge), false)) {
					continue;
				}

				//if(val.GetType().IsClass) {
				//	continue;
				//}

				var arrBrg = mi.GetCustomAttributes<Bridge>();
				foreach(var brg in arrBrg) {
					if(brg.classTargetName != classTargetName) {
						continue;
					}

					string path = brg.targetPath;
					object dataTemp = to;

					string[] arrPath = path.Split('.');
					if(arrPath.Length <= 0) {
						continue;
					}

					//MemberInfo mi = dataTemp.GetType().GetMember(arrPath[0]);

					bool nofound = false;
					for(int i = 0; i < arrPath.Length; ++i) {
						string str = arrPath[i];
						var member = dataTemp.GetType().GetMember(str, bindFlag).FirstOrDefault();
						if(member == null) {
							nofound = true;
							break;
						}
						dataTemp = getMemberValue(member, dataTemp);
					}
					if(nofound) {
						continue;
					}

					try {
						var newDataTemp = Convert.ChangeType(dataTemp, val.GetType());
						setMemberValue(mi, subData, newDataTemp);
					} catch(Exception) {
						try {
							setMemberValue(mi, subData, dataTemp);
						} catch(Exception) { }
					}
					//Debug.WriteLine("cc:" + val.GetType() + "," + path + "," + getMemberValue(mi, data));
				}

			}

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

	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
	public class Bridge : Attribute {
		public string targetPath = "";
		public string classTargetName = "";

		public Bridge(string _targetPath, string _classTargetName = "") {
			targetPath = _targetPath;
			classTargetName = _classTargetName;
		}
	}
	
}
