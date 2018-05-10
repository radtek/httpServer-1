using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.ui {

	public abstract class FlowOneWay<T1, T2> where T2 : new() {
		public abstract void flow(T1 src, T2 dst);

		public void lstAdd(IList<T1> src, IList<T2> dst, T1 srcOne) {
			src.Add(srcOne);

			T2 dstOne = new T2();
			flow(srcOne, dstOne);
			dst.Add(dstOne);
		}

		public void lstInsert(IList<T1> src, IList<T2> dst, T1 srcOne, int idx) {
			src.Insert(idx, srcOne);

			T2 dstOne = new T2();
			flow(srcOne, dstOne);
			dst.Insert(idx, dstOne);
		}

		public void lstAdd(T1 srcOne, IList<T2> dst) {
			T2 dstOne = new T2();
			flow(srcOne, dstOne);
			dst.Add(dstOne);
		}

		public void lstInsert(T1 srcOne, IList<T2> dst, int idx) {
			T2 dstOne = new T2();
			flow(srcOne, dstOne);
			dst.Insert(idx, dstOne);
		}

		public void lstRemoveAt(IList<T1> src, IList<T2> dst, int idx) {
			src.RemoveAt(idx);
			dst.RemoveAt(idx);
		}

		public void lstRemoveAt(IList<T2> src, IList<T1> dst, int idx) {
			src.RemoveAt(idx);
			dst.RemoveAt(idx);
		}

		public virtual void lstFlowOpt(int idx, IList<T1> src, IList<T2> dst) {

		}

		public void lstFlow(IList<T1> src, IList<T2> dst) {
			int count = Math.Min(src.Count, dst.Count);
			for(int i = 0; i < count; ++i) {
				flow(src[i], dst[i]);
				lstFlowOpt(i, src, dst);
			}
		}

		public void lstInit(IList<T1> src, IList<T2> dst) {
			dst.Clear();
			for(int i = 0; i < src.Count; ++i) {
				T2 t2 = new T2();
				flow(src[i], t2);
				dst.Add(t2);
				lstFlowOpt(i, src, dst);
			}
		}
	}

	public abstract class FlowTwoWay<T1, T2> : FlowOneWay<T1, T2> where T1 : new() where T2 : new() {
		//public virtual void flow(T1 src, T2 dst) { }
		public abstract void flow(T2 src, T1 dst);

		//public void lstAdd(List<T1> src, List<T2> dst, T1 srcOne) {
		//	src.Add(srcOne);

		//	T2 dstOne = new T2();
		//	flow(srcOne, dstOne);
		//	dst.Add(dstOne);
		//}

		public void lstAdd(IList<T2> src, IList<T1> dst, T2 srcOne) {
			src.Add(srcOne);

			T1 dstOne = new T1();
			flow(srcOne, dstOne);
			dst.Add(dstOne);
		}

		public void lstAdd(T2 srcOne, IList<T1> dst) {
			T1 dstOne = new T1();
			flow(srcOne, dstOne);
			dst.Add(dstOne);
		}

		//public void lstRemoveAt(List<T1> src, List<T2> dst, int idx) {
		//	src.RemoveAt(idx);
		//	dst.RemoveAt(idx);
		//}

		//public void lstRemoveAt(List<T2> src, List<T1> dst, int idx) {
		//	src.RemoveAt(idx);
		//	dst.RemoveAt(idx);
		//}

		//public virtual void lstFlowOpt(int idx, List<T1> src, List<T2> dst) {

		//}

		public virtual void lstFlowOpt(int idx, IList<T2> src, IList<T1> dst) {

		}

		//public void lstFlow(List<T1> src, List<T2> dst) {
		//	int count = Math.Min(src.Count, dst.Count);
		//	for (int i = 0; i < count; ++i) {
		//		flow(src[i], dst[i]);
		//		lstFlowOpt(i, src, dst);
		//	}
		//}

		public void lstFlow(IList<T2> src, IList<T1> dst) {
			int count = Math.Min(src.Count, dst.Count);
			for(int i = 0; i < count; ++i) {
				flow(src[i], dst[i]);
				lstFlowOpt(i, src, dst);
			}
		}

		//public void lstInit(List<T1> src, List<T2> dst) {
		//	dst.Clear();
		//	for (int i = 0; i < src.Count; ++i) {
		//		T2 t2 = new T2();
		//		flow(src[i], t2);
		//		dst.Add(t2);
		//		lstFlowOpt(i, src, dst);
		//	}
		//}

		public void lstInit(IList<T2> src, IList<T1> dst) {
			dst.Clear();
			for(int i = 0; i < src.Count; ++i) {
				T1 t1 = new T1();
				flow(src[i], t1);
				dst.Add(t1);
				lstFlowOpt(i, src, dst);
			}
		}

	}

	//class VFlowModel<T1, T2> : VFlowModelBase {
	//	public IList<T1> src;
	//	public IList<T2> dst;
	//	public T1 srcOne;
	//	public Action<T1, T2> fun;
	//}

	//class VFlowModelBase  {

	//}

	//class VList<TSrc, TDst> {
	//	Func<TSrc, TDst> fun = null;
	//	public VList(Func<TSrc, TDst> _fun) {
	//		fun = _fun;
	//	}

	//	public void toList() {

	//	}
	//}

	//public class VFLowCtl {
	//	List<VFlowModelBase> lstAction = new List<VFlowModelBase>();
	//	public void lstInit<TSrc, TDst>(IList<TSrc> src, IList<TDst> dst, TSrc srcOne, Action<TSrc, TDst> fun) where TDst : new() {
	//		VFlowModel<TSrc, TDst> md = new VFlowModel<TSrc, TDst>();
	//		md.src = src;
	//		md.dst = dst;
	//		md.srcOne = srcOne;
	//		md.fun = fun;
	//		lstAction.Add(md);
	//	}

	//	public void run() {

	//	}
	//}

	/// <summary>重复过滤器</summary>
	public class RepetFilter {
		public static RepetFilter<TKey, TModel> create<TKey, TModel>(Func<TModel, TKey> _fun) {
			return new RepetFilter<TKey, TModel>(_fun);
		}
		public static Func<TModel, bool> filter<TKey, TModel>(Func<TModel, TKey> _fun) {
			return create(_fun).filter;
		}
	}

	/// <summary>重复过滤器</summary>
	public class RepetFilter<TKey, TModel> {
		public HashSet<TKey> data = new HashSet<TKey>();
		public Func<TModel, TKey> fun;

		public RepetFilter(Func<TModel, TKey> _fun) {
			fun = _fun;
		}

		public bool filter(TModel t) {
			if(data.Contains(fun(t))) {
				return true;
			}

			data.Add(fun(t));
			return false;
		}

		public void clear() {
			data.Clear();
		}
	}

	public static class FlowCtl {

		public static void lstAdd<TSrc, TDst>(IList<TSrc> src, IList<TDst> dst, TSrc srcOne, Action<TSrc, TDst> fun) where TDst : new() {
			src.Add(srcOne);

			TDst dstOne = new TDst();
			fun(srcOne, dstOne);
			dst.Add(dstOne);
		}

		public static void lstInsert<TSrc, TDst>(IList<TSrc> src, IList<TDst> dst, TSrc srcOne, int idx, Action<TSrc, TDst> fun) where TDst : new() {
			src.Insert(idx, srcOne);

			TDst dstOne = new TDst();
			fun(srcOne, dstOne);
			dst.Insert(idx, dstOne);
		}

		public static void lstInsert<TSrc, TDst>(TSrc srcOne, IList<TDst> dst, int idx, Action<TSrc, TDst> fun) where TDst : new() {
			TDst dstOne = new TDst();
			fun(srcOne, dstOne);
			dst.Insert(idx, dstOne);
		}

		public static void lstInit<TSrc, TDst>(IList<TSrc> src, IList<TDst> dst, Action<TSrc, TDst> fun, Func<TSrc, bool> filter = null) where TDst : new() {
			dst.Clear();
			for(int i = 0; i < src.Count; ++i) {
				if(filter != null && filter(src[i])) {
					continue;
				}
				TDst dstOne = new TDst();
				fun(src[i], dstOne);
				dst.Add(dstOne);
			}
		}

		public static List<TDst> lstInit<TSrc, TDst>(IList<TSrc> src, Action<TSrc, TDst> fun, Func<TSrc, bool> filter = null) where TDst : new() {
			List<TDst> dst = new List<TDst>();

			dst.Clear();
			for(int i = 0; i < src.Count; ++i) {
				if(filter != null && filter(src[i])) {
					continue;
				}
				TDst dstOne = new TDst();
				fun(src[i], dstOne);
				dst.Add(dstOne);
			}
			return dst;
		}

		public static void lstFlow<TSrc, TDst>(IList<TSrc> src, IList<TDst> dst, Action<TSrc, TDst> fun) {
			int count = Math.Min(src.Count, dst.Count);
			for(int i = 0; i < count; ++i) {
				fun(src[i], dst[i]);
			}
		}

		public static void lstAdd<TSrc, TDst>(TSrc srcOne, IList<TDst> dst, Action<TSrc, TDst> fun) where TDst : new() {
			TDst dstOne = new TDst();
			fun(srcOne, dstOne);
			dst.Add(dstOne);
		}

		public static void idxInit<TSrc, TDst>(List<TSrc> src, IDictionary<TDst, TSrc> map, Func<TSrc, TDst> fun) {
			map.Clear();
			for(int i = 0; i < src.Count; ++i) {
				map[fun(src[i])] = src[i];
			}
		}

		public static Dictionary<TKey, TModel> idxInit<TModel, TKey>(IList<TModel> src, Func<TModel, TKey> fun) {
			Dictionary<TKey, TModel> map = new Dictionary<TKey, TModel>();
			for(int i = 0; i < src.Count; ++i) {
				map[fun(src[i])] = src[i];
			}
			return map;
		}

		public static void idxFlow<TKey, TModel>(IList<TModel> src, IDictionary<TKey, TModel> map, Func<TModel, TKey> fun) {
			for(int i = 0; i < src.Count; ++i) {
				map[fun(src[i])] = src[i];
			}
		}

		public static void hashInit<TKey, TModel>(List<TModel> src, ISet<TKey> map, Func<TModel, TKey> fun) {
			map.Clear();
			for(int i = 0; i < src.Count; ++i) {
				map.Add(fun(src[i]));
			}
		}

		public static HashSet<TKey> hashInit<TKey, TModel>(List<TModel> src, Func<TModel, TKey> fun) {
			HashSet<TKey> map = new HashSet<TKey>();
			for(int i = 0; i < src.Count; ++i) {
				map.Add(fun(src[i]));
			}
			return map;
		}

		//public static List<TTarget> lstSub<TKey, TTarget, TTemplate>(IList<TTarget> target, IList<TTemplate> template, Func<TTarget, TKey> funTarget, Func<TTemplate, TKey> funTemplate) {
		//	List<TTarget> rst = new List<TTarget>();
		//	Dictionary<TKey, TTemplate> mapTemplate = idxInit(template, funTemplate);
		//	for(int i = 0; i < target.Count; ++i) {
		//		if(!mapTemplate.ContainsKey(funTarget(target[i])){
		//			continue;
		//		}
		//		rst.Add(target[i]);
		//	}
		//	return rst;
		//}

		public static List<TDst> lstInit<TSrc, TDst>(IDictionary<TSrc, TDst> map, Func<TDst, bool> filter = null) {
			List<TDst> rst = new List<TDst>();
			foreach(TSrc t in map.Keys) {
				if(filter != null && filter(map[t])) {
					continue;
				}
				rst.Add(map[t]);
			}
			return rst;
		}
	}

	public class FlowSet {
		public static FlowSet<TKey, TFirst, TLast> create<TKey, TFirst, TLast>(IList<TFirst> _lstA, IList<TLast> _lstB, Func<TFirst, TKey> _funA, Func<TLast, TKey> _funB) {
			return new FlowSet<TKey, TFirst, TLast>(_lstA, _lstB, _funA, _funB);
		}

		public static FlowSet<TModel, TModel, TModel> create<TModel>(IList<TModel> _lstA, IList<TModel> _lstB) {
			return new FlowSet<TModel, TModel, TModel>(_lstA, _lstB, md => md, md => md);
		}

		/// <summary>交集</summary>
		public static List<TFirst> intersectionFirst<TKey, TFirst, TLast>(IList<TFirst> _lstA, IList<TLast> _lstB, Func<TFirst, TKey> _funA, Func<TLast, TKey> _funB) {
			return create(_lstA, _lstB, _funA, _funB).intersectionFirst();
		}

		/// <summary>交集</summary>
		public static List<TLast> intersectionLast<TKey, TFirst, TLast>(IList<TFirst> _lstA, IList<TLast> _lstB, Func<TFirst, TKey> _funA, Func<TLast, TKey> _funB) {
			return create(_lstA, _lstB, _funA, _funB).intersectionLast();
		}

		/// <summary>差集</summary>
		public static List<TFirst> diffFirst<TKey, TFirst, TLast>(IList<TFirst> _lstA, IList<TLast> _lstB, Func<TFirst, TKey> _funA, Func<TLast, TKey> _funB) {
			return create(_lstA, _lstB, _funA, _funB).diffFirst();
		}

		/// <summary>差集</summary>
		public static List<TLast> diffLast<TKey, TFirst, TLast>(IList<TFirst> _lstA, IList<TLast> _lstB, Func<TFirst, TKey> _funA, Func<TLast, TKey> _funB) {
			return create(_lstA, _lstB, _funA, _funB).diffLast();
		}
	}

	public class FlowSet<TKey, TFirst, TLast> {
		IList<TFirst> lstA;
		IList<TLast> lstB;
		Func<TFirst, TKey> funA;
		Func<TLast, TKey> funB;
		Dictionary<TKey, TFirst> mapA = new Dictionary<TKey, TFirst>();
		Dictionary<TKey, TLast> mapB = new Dictionary<TKey, TLast>();

		public FlowSet(IList<TFirst> _lstA, IList<TLast> _lstB, Func<TFirst, TKey> _funA, Func<TLast, TKey> _funB) {
			lstA = _lstA;
			lstB = _lstB;
			funA = _funA;
			funB = _funB;
		}

		/// <summary>交集</summary>
		public List<TFirst> intersectionFirst() {
			initMapA();
			List<TFirst> lstRst = new List<TFirst>();
			for(int i = 0; i < lstB.Count; ++i) {
				TKey key = funB(lstB[i]);
				if(mapA.ContainsKey(key)){
					lstRst.Add(mapA[key]);
				}
			}
			return lstRst;
		}

		/// <summary>交集</summary>
		public List<TLast> intersectionLast() {
			initMapA();
			List<TLast> lstRst = new List<TLast>();
			for(int i = 0; i < lstB.Count; ++i) {
				TKey key = funB(lstB[i]);
				if(mapA.ContainsKey(key)) {
					lstRst.Add(lstB[i]);
				}
			}
			return lstRst;
		}

		/// <summary>差集</summary>
		public List<TFirst> diffFirst() {
			initMapB();
			List<TFirst> lstRst = new List<TFirst>();
			for(int i = 0; i < lstA.Count; ++i) {
				TKey key = funA(lstA[i]);
				if(!mapB.ContainsKey(key)) {
					lstRst.Add(lstA[i]);
				}
			}
			return lstRst;
		}

		/// <summary>差集</summary>
		public List<TLast> diffLast() {
			initMapA();
			List<TLast> lstRst = new List<TLast>();
			for(int i = 0; i < lstB.Count; ++i) {
				TKey key = funB(lstB[i]);
				if(!mapA.ContainsKey(key)) {
					lstRst.Add(lstB[i]);
				}
			}
			return lstRst;
		}

		private void initMapA() {
			if(mapA.Count > 0) {
				return;
			}
			for(int i = 0; i < lstA.Count; ++i) {
				mapA.Add(funA(lstA[i]), lstA[i]);
			}
		}

		private void initMapB() {
			if(mapB.Count > 0) {
				return;
			}
			for(int i = 0; i < lstB.Count; ++i) {
				mapB.Add(funB(lstB[i]), lstB[i]);
			}
		}
	}

}
