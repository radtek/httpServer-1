using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace csharpHelp.util {
	class SupRelayCommand : ICommand {
		private Action action;
		public SupRelayCommand(Action _action) {
			action = _action;
		}

		public bool CanExecute(object parameter) {
			return true;
		}

		public event EventHandler CanExecuteChanged {
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter) {
			action?.Invoke();
		}
	}

	class SupRelayCommand<T> : ICommand {
		private Action<T> action;
		public SupRelayCommand(Action<T> _action) {
			action = _action;
		}

		public bool CanExecute(object parameter) {
			return true;
		}

		public event EventHandler CanExecuteChanged {
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter) {
			action?.Invoke((T)parameter);
		}
	}
}
