using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace csharpHelp.ui {
	/// <summary>Behavior</summary>
	public static class BehaviorBox {
		static Dictionary<DependencyObject, ListBehavior> mapBhv = new Dictionary<DependencyObject, ListBehavior>();
		static ListBehavior _Behaviors = new ListBehavior();

		public static ListBehavior GetList(DependencyObject obj) {
			if(!mapBhv.ContainsKey(obj)) {
				ListBehavior lst = new ListBehavior();
				lst.obj = obj;
				mapBhv[obj] = lst;
			}
			return mapBhv[obj];
			//return _Behaviors;
		}
	}

	public class ListBehavior : ObservableCollection<BaseBehavior>, INotifyCollectionChanged {
		public DependencyObject obj = null;

		public ListBehavior() {
			CollectionChanged += ListBehavior_CollectionChanged;
		}

		public new void Clear() {
			while(Count > 0) {
				RemoveAt(Count - 1);
			}
		}

		private void ListBehavior_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			IList lstItem = null;

			switch(e.Action) {
			case NotifyCollectionChangedAction.Add: lstItem = e.NewItems as IList;break;
			case NotifyCollectionChangedAction.Remove: lstItem = e.OldItems as IList;break;
			}

			if(lstItem == null) {
				return;
			}

			for(int i = 0; i < lstItem.Count; ++i) {
				BaseBehavior bhv = lstItem[i] as BaseBehavior;
				if(bhv == null) {
					continue;
				}

				switch(e.Action) {
				case NotifyCollectionChangedAction.Add: bhv._element = obj; bhv.onAttached(); break;
				case NotifyCollectionChangedAction.Remove: bhv.onDetaching(); break;
				}
			}
			
		}

	}
}
