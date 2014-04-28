﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;

namespace Grabacr07.KanColleWrapper
{
	/// <summary>
	/// 艦娘と艦隊の編成を表します。
	/// </summary>
	public class Organization : NotificationObject
	{
		private readonly Homeport homeport;

		#region Ships 変更通知プロパティ

		private MemberTable<Ship> _Ships;

		/// <summary>
		/// 母港に所属する艦娘のコレクションを取得します。
		/// </summary>
		public MemberTable<Ship> Ships
		{
			get { return this._Ships; }
			private set
			{
				if (this._Ships != value)
				{
					this._Ships = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Fleets 変更通知プロパティ

		private MemberTable<Fleet> _Fleets;

		/// <summary>
		/// 編成された艦隊のコレクションを取得します。
		/// </summary>
		public MemberTable<Fleet> Fleets
		{
			get { return this._Fleets; }
			private set
			{
				if (this._Fleets != value)
				{
					this._Fleets = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public Ship Secretary
		{
			get
			{
				if (this.Fleets[1] != null)
				{
					return this.Fleets[1].Ships[0];
				}
				return null;
			}
		}


		public Organization(Homeport parent, KanColleProxy proxy)
		{
			this.homeport = parent;

			this.Ships = new MemberTable<Ship>();
			this.Fleets = new MemberTable<Fleet>();

			proxy.api_get_member_ship.TryParse<kcsapi_ship2[]>().Subscribe(x => this.Update(x.Data));
			proxy.api_get_member_ship2.TryParse<kcsapi_ship2[]>().Subscribe(x =>
			{
				this.Update(x.Data);
				this.Update(x.Fleets);
			});
			proxy.api_get_member_ship3.TryParse<kcsapi_ship3>().Subscribe(x =>
			{
				this.Update(x.Data.api_ship_data);
				this.Update(x.Data.api_deck_data);
			});

			proxy.api_get_member_deck.TryParse<kcsapi_deck[]>().Subscribe(x => this.Update(x.Data));
			proxy.api_get_member_deck_port.TryParse<kcsapi_deck[]>().Subscribe(x => this.Update(x.Data));

			proxy.api_req_hensei_change.TryParse().Subscribe(this.Change);
			proxy.api_req_hokyu_charge.TryParse<kcsapi_charge>().Subscribe(x => this.Charge(x.Data));
			proxy.api_req_kaisou_powerup.TryParse<kcsapi_powerup>().Subscribe(this.Powerup);

			proxy.api_req_kousyou_getship.TryParse<kcsapi_getship>().Subscribe(x => this.GetShip(x.Data));
			proxy.api_req_kousyou_destroyship.TryParse<kcsapi_destroyship>().Subscribe(this.DestroyShip);
			proxy.api_req_kousyou_destroyitem2.TryParse<kcsapi_destroyitem>().Subscribe(this.DestroyItem);
		}


		/// <summary>
		/// 指定した ID の艦娘が所属する艦隊を取得します。
		/// </summary>
		internal Fleet GetFleet(int shipId)
		{
			return this.Fleets.Select(x => x.Value).SingleOrDefault(x => x.Ships.Any(s => s.Id == shipId));
		}


		/// <summary>
		/// 指定した <see cref="kcsapi_ship2"/> 型の配列を使用して、<see cref="Ships"/> プロパティ値を更新します。
		/// </summary>
		internal void Update(kcsapi_ship2[] source)
		{
			if (source.Length <= 1)
			{
				foreach (var ship in source)
				{
					var target = this.Ships[ship.api_id];
					if (target == null) continue;

					target.Update(ship);

					var fleet = this.GetFleet(target.Id);
					if (fleet != null) fleet.Calculate();
				}
			}
			else
			{
				this.Ships = new MemberTable<Ship>(source.Select(x => new Ship(this.homeport, x)));
			}
		}


		/// <summary>
		/// 指定した <see cref="kcsapi_deck"/> 型の配列を使用して、<see cref="Fleets"/> プロパティ値を更新します。
		/// </summary>
		internal void Update(kcsapi_deck[] source)
		{
			if (this.Fleets.Count == source.Length)
			{
				foreach (var raw in source)
				{
					var target = this.Fleets[raw.api_id];
					if (target != null) target.Update(raw);
				}
			}
			else
			{
				this.Fleets.ForEach(x => x.Value.Dispose());
				this.Fleets = new MemberTable<Fleet>(source.Select(x => new Fleet(this.homeport, x)));
			}
		}


		private void Change(SvData data)
		{
			if (data == null || !data.IsSuccess) return;

			try
			{
				var fleet = this.Fleets[int.Parse(data.Request["api_id"])];
				var index = int.Parse(data.Request["api_ship_idx"]);
				if (index == -1)
				{
					// 旗艦以外をすべて外すケース
					fleet.UnsetAll();
					return;
				}

				var ship = this.Ships[int.Parse(data.Request["api_ship_id"])];
				if (ship == null)
				{
					// 艦を外すケース
					fleet.Unset(index);
					return;
				}

				var currentFleet = this.GetFleet(ship.Id);
				if (currentFleet == null)
				{
					// ship が、現状どの艦隊にも所属していないケース
					fleet.Change(index, ship);
					return;
				}

				// ship が、現状いずれかの艦隊に所属しているケース
				var currentIndex = Array.IndexOf(currentFleet.Ships, ship);
				var old = fleet.Change(index, ship);

				// Fleet.Change(int, Ship) は、変更前の艦を返す (= old) ので、
				// ship の移動元 (currentFleet + currentIndex) に old を書き込みにいく
				currentFleet.Change(currentIndex, old);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("編成の変更に失敗しました: {0}", ex);
			}
		}

		private void Charge(kcsapi_charge source)
		{
			Fleet fleet = null;	// 補給した艦が所属している艦隊。艦隊をまたいで補給はできないので、必ず 1 つに絞れる

			foreach (var ship in source.api_ship)
			{
				var target = this.Ships[ship.api_id];
				if (target == null) continue;

				target.Charge(ship.api_fuel, ship.api_bull, ship.api_onslot);

				if (fleet == null)
				{
					fleet = this.GetFleet(target.Id);
				}
			}

			if (fleet != null) fleet.UpdateStatus();

			this.homeport.Materials.Update(source.api_material);
		}

		private void Powerup(SvData<kcsapi_powerup> svd)
		{
			try
			{
				var target = this.Ships[svd.Data.api_ship.api_id];
				if (target != null)
				{
					target.Update(svd.Data.api_ship);
				}

				var items = svd.Request["api_id_items"]
					.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(int.Parse)
					.Where(x => this.Ships.ContainsKey(x))
					.Select(x => this.Ships[x]);

				// (改修に使った艦娘のこと item って呼ぶのどうなの…)

				this.Ships = new MemberTable<Ship>(this.Ships.Select(kvp => kvp.Value).Except(items));
				this.Update(svd.Data.api_deck);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("近代化改修による更新に失敗しました: {0}", ex);
			}
		}

		private void GetShip(kcsapi_getship source)
		{
			this.homeport.Dockyard.Update(source.api_kdock);

			var slotitems = this.homeport.SlotItems.Values;
			slotitems = slotitems.Concat(source.api_slotitem.Select(s => new SlotItem(s)));
			this.homeport.SlotItems = new MemberTable<SlotItem>(slotitems);

			var ships = this.Ships.Values.ToList();
			ships.Add(new Ship(this.homeport, source.api_ship));
			this.Ships = new MemberTable<Ship>(ships);
		}

		private void DestroyShip(SvData<kcsapi_destroyship> svdata)
		{
			var ship = this.Ships[int.Parse(svdata.Request["api_ship_id"])];
			this.DeleteShips(ship);
		}

		private void DestroyItem(SvData<kcsapi_destroyitem> svdata)
		{
			var slotItems = svdata.Request["api_slotitem_ids"]
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(int.Parse)
				.Where(x => this.homeport.SlotItems.ContainsKey(x))
				.Select(x => this.homeport.SlotItems[x]);

			this.DeleteSlotItems(slotItems);
		}

		internal void DeleteShips(Ship ship)
		{
			this.DeleteShips(new[] { ship });
		}

		internal void DeleteShips(IEnumerable<Ship> ships)
		{
			this.DeleteSlotItems(ships.SelectMany(s => s.SlotItems));
			this.Ships = new MemberTable<Ship>(this.Ships.Select(kvp => kvp.Value).Except(ships));
		}

		internal void DeleteSlotItems(IEnumerable<SlotItem> slotItems)
		{
			this.homeport.SlotItems = new MemberTable<SlotItem>(this.homeport.SlotItems.Select(kvp => kvp.Value).Except(slotItems));
		}
	}
}
