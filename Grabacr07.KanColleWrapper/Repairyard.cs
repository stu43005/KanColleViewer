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
	/// 複数の入渠ドックを持つ工廠を表します。
	/// </summary>
	public class Repairyard : NotificationObject
	{
		private readonly Homeport homeport;

		#region Docks 変更通知プロパティ

		private MemberTable<RepairingDock> _Docks;

		public MemberTable<RepairingDock> Docks
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

		internal Repairyard(Homeport parent, KanColleProxy proxy)
		{
			this.homeport = parent;
			this.Docks = new MemberTable<RepairingDock>();

			proxy.api_get_member_ndock.TryParse<kcsapi_ndock[]>().Subscribe(x => this.Update(x.Data));
			proxy.api_req_nyukyo_start.TryParse().Subscribe(this.RepairShip);
		}

		private void RepairShip(SvData svdata)
		{
			int api_ndock_id = int.Parse(svdata.Request["api_ndock_id"]); // 1,2,3,4
			int api_highspeed = int.Parse(svdata.Request["api_highspeed"]); // 0,1
			int api_ship_id = int.Parse(svdata.Request["api_ship_id"]);

			if (api_highspeed == 1)
			{
				var ship = this.homeport.Organization.Ships[api_ship_id];
				if (ship != null)
				{
					ship.Repair();

					var fleet = this.homeport.Organization.GetFleet(ship.Id);
					if (ship != null) fleet.UpdateStatus();
				}
			}
		}

		internal void Update(kcsapi_ndock[] source)
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
				this.Docks = new MemberTable<RepairingDock>(source.Select(x => new RepairingDock(homeport, x)));
			}
		}

		/// <summary>
		/// 指定した ID の艦娘が現在入渠中かどうかを確認します。
		/// </summary>
		/// <param name="shipId">艦隊に所属する艦娘の ID。</param>
		public bool CheckRepairing(int shipId)
		{
			return this.Docks.Values.Where(x => x.Ship != null).Any(x => x.ShipId == shipId);
		}

		/// <summary>
		/// 指定した艦隊に、現在入渠中の艦娘がいるかどうかを確認します。
		/// </summary>
		public bool CheckRepairing(Fleet fleet)
		{
			var repairingShipIds = this.Docks.Values.Where(x => x.Ship != null).Select(x => x.Ship.Id).ToArray();
			return fleet.Ships.Any(x => repairingShipIds.Any(id => id == x.Id));
		}
	}
}
