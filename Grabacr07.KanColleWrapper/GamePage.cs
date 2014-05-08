using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Internal;
using Livet;

namespace Grabacr07.KanColleWrapper
{
	public class GamePage : NotificationObject
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


		internal GamePage(KanColleProxy proxy)
		{
			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/mapinfo")
				.Subscribe(x => this.PageName = "map");
			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/practice")
				.Subscribe(x => this.PageName = "practice");
			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/mission")
				.Subscribe(x => this.PageName = "mission");
			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/ndock")
				.Subscribe(x => this.PageName = "nyukyo");
		}
	}
}
