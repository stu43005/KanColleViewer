using Livet;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace Logger
{
	public class LoggerViewModel : ViewModel
	{
		#region Loggers 変更通知プロパティ

		private ObservableCollection<LoggerBase> _Loggers;

		public ObservableCollection<LoggerBase> Loggers
		{
			get { return this._Loggers; }
			set
			{
				if (this._Loggers != value)
				{
					this._Loggers = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public LoggerViewModel()
		{
		}
	}
}
