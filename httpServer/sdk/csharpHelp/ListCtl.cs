using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	public class ListCtl<T> {
		public List<T> data = new List<T>();

		public ListCtl(){

		}

		public ListCtl(List<T> _data) {
			data = _data;
		}

		public ListCtl(int count, T _initData) {
			initData(count, _initData);
		}

		public ListCtl<T> setData(List<T> _data) {
			data = _data;
			return this;
		}

		public ListCtl<T> initData(int count, T _initData) {
			data = new List<T>();
			for(int i = 0; i < count; ++i) {
				data.Add(_initData);
			}

			return this;
		}

		public int Count {
			get { return data.Count; }
		}

		public static implicit operator ListCtl<T>(List<T> _data) {
			return new ListCtl<T>().setData(_data);
		}

		public T this[int index] {
			get { return data[index]; }
			set { data[index] = value; }
		}

		public List<ListCtl<T>> split(int count) {
			List<ListCtl<T>> result = new List<ListCtl<T>>();
			for(int i = 0; i < data.Count; i += count) {
				result.Add(child(i, count));
			}

			return result;
		}

		public void Add(T _data) {
			data.Add(_data);
		}

		public void Add(List<T> lst) {
			for(int i = 0; i < lst.Count; ++i) {
				data.Add(lst[i]);
			}
		}

		public static ListCtl<T> operator +(ListCtl<T> c1, ListCtl<T> c2) {
			ListCtl<T> result = new ListCtl<T>();

			//result.lstData = c1.lstData;
			for(int i = 0; i < c1.data.Count; ++i) {
				result.data.Add(c1.data[i]);
			}
			for(int i = 0; i < c2.data.Count; ++i) {
				result.data.Add(c2.data[i]);
			}

			return result;
		}

		public ListCtl<T> child(int startIdx, int count = -1) {
			List<T> temp = new List<T>();

			//
			int len = 0;
			if(count < 0) {
				len = data.Count;
			} else {
				len = Math.Min(startIdx + count, data.Count);
			}

			//
			for(int i = startIdx; i < len; ++i) {
				temp.Add(data[i]);
			}

			//
			ListCtl<T> result = new ListCtl<T>();
			result.data = temp;

			return result;
		}

		public string toString(string gap) {
			string result = "";
			for(int i = 0; i < data.Count; ++i) {
				result += (i == 0 ? "" : gap) + data[i].ToString();
			}

			return result;
		}

	}
}
