using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleWrapper;
using Livet.EventListeners;

namespace Grabacr07.KanColleViewer.ViewModels.Dev
{
	public class DebugTabViewModel : TabItemViewModel
	{
		#region PageName 変更通知プロパティ

		private string _PageName;

		public string PageName
		{
			get { return this._PageName; }
			set
			{
				if (this._PageName != value)
				{
					this._PageName = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public override string Name
		{
			get { return "Debug"; }
			protected set { throw new NotImplementedException(); }
		}


		public DebugTabViewModel()
		{
			this.CompositeDisposable.Add(new PropertyChangedEventListener(KanColleClient.Current.Homeport.GamePage)
			{
				{ "PageName", (sender, args) => this.Update() },
			});
		}
		
		private void Update()
		{
			switch(KanColleClient.Current.Homeport.GamePage.PageName)
			{
				case "nyukyo":
					this.PageName = "入渠";
					break;
				case "practice":
					this.PageName = "演習";
					break;
				case "map":
					this.PageName = "出擊";
					break;
				case "mission":
					this.PageName = "遠征";
					break;
				case "kousyou":
					this.PageName = "工廠";
					break;
				default:
					this.PageName = KanColleClient.Current.Homeport.GamePage.PageName;
					break;
			}
		}
		public void Notify()
		{
			WindowsNotification.Notifier.Show("テスト", "これはテスト通知です。", () => App.ViewModelRoot.Activate());
		}

	}
}
