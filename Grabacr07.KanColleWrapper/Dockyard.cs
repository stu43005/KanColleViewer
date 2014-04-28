﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;

namespace Grabacr07.KanColleWrapper
{
	/// <summary>
	/// 複数の建造ドックを持つ工廠を表します。
	/// </summary>
	public class Dockyard : NotificationObject
	{
		private readonly Homeport homeport;

		#region Dock 変更通知プロパティ

		private MemberTable<BuildingDock> _Docks;

		public MemberTable<BuildingDock> Docks
		{
			get { return this._Docks; }
			set
			{
				if (this._Docks != value)
				{
					this._Docks = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		internal Dockyard(Homeport parent, KanColleProxy proxy)
		{
			this.homeport = parent;
			this.Docks = new MemberTable<BuildingDock>();

			proxy.api_get_member_kdock.TryParse<kcsapi_kdock[]>().Subscribe(x => this.Update(x.Data));
		}

		internal void Update(kcsapi_kdock[] source)
		{
			if (this.Docks.Count == source.Length)
			{
				foreach (var raw in source)
				{
					var target = this.Docks[raw.api_id];
					if (target != null) target.Update(raw);
				}
			}
			else
			{
				this.Docks.ForEach(x => x.Value.Dispose());
				this.Docks = new MemberTable<BuildingDock>(source.Select(x => new BuildingDock(x)));
			}
		}
	}
}
