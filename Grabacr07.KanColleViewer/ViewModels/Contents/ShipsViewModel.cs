using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleViewer.Properties;
using Grabacr07.KanColleWrapper;
using Livet;
using Livet.EventListeners;
using Grabacr07.KanColleViewer.Composition;

namespace Grabacr07.KanColleViewer.ViewModels.Contents
{
	public class ShipsViewModel : ViewModel
	{
		#region Count 変更通知プロパティ

		private int _Count;

		public int Count
		{
			get { return this._Count; }
			set
			{
				if (this._Count != value)
				{
					this._Count = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public ShipsViewModel()
		{
			this.CompositeDisposable.Add(new PropertyChangedEventListener(KanColleClient.Current.Homeport.Organization)
			{
				{ "Ships", (sender, args) => this.Update() }
			});

			this.CompositeDisposable.Add(new PropertyChangedEventListener(this)
			{
				{ "Count", (sender, args) => this.CountUpdate() }
			});

			this.Update();
		}

		private void Update()
		{
			this.Count = KanColleClient.Current.Homeport.Organization.Ships.Count;
		}

		private void CountUpdate()
		{
			if (this.Count >= KanColleClient.Current.Homeport.Admiral.MaxShipCount)
			{
				PluginHost.Instance.GetNotifier().Show(NotifyType.Other, Resources.Common_ShipGirl, "艦娘持有數達到最大值了。", () => App.ViewModelRoot.Activate());
			}
		}
	}
}
