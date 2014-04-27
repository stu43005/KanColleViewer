using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Grabacr07.KanColleWrapper.Internal;
using Livet;

namespace Grabacr07.KanColleWrapper
{
	/// <summary>
	/// 母港を表します。
	/// </summary>
	public class Homeport : NotificationObject
	{
		#region Fleets 変更通知プロパティ

		private MemberTable<Fleet> _Fleets;

		public MemberTable<Fleet> Fleets
		{
			get { return this._Fleets; }
			set
			{
				if (this._Fleets != value)
				{
					this._Fleets = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		/// <summary>
		/// 複数の建造ドックを持つ工廠を取得します。
		/// </summary>
		public Dockyard Dockyard { get; private set; }

		/// <summary>
		/// 複数の入渠ドックを持つ工廠を取得します。
		/// </summary>
		public Repairyard Repairyard { get; private set; }

		/// <summary>
		/// 任務情報を取得します。
		/// </summary>
		public Quests Quests { get; private set; }

		public GamePage GamePage { get; private set; }

		/// <summary>
		/// Logs events such as ship drops, crafts, and item developments.
		/// </summary>
		public Logger Logger { get; private set; }

		#region Admiral 変更通知プロパティ

		private Admiral _Admiral;

		/// <summary>
		/// 現在ログインしている提督を取得します。
		/// </summary>
		public Admiral Admiral
		{
			get { return this._Admiral; }
			private set
			{
				if (this._Admiral != value)
				{
					this._Admiral = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Materials 変更通知プロパティ

		private Materials _Materials;

		/// <summary>
		/// 艦隊司令部の資源および資材の保有状況にアクセスできるようにします。
		/// </summary>
		public Materials Materials
		{
			get { return this._Materials; }
			set
			{
				if (this._Materials != value)
				{
					this._Materials = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Ships 変更通知プロパティ

		private MemberTable<Ship> _Ships;

		/// <summary>
		/// 艦隊司令部に所属しているすべての艦娘を取得します。艦娘の ID を使用して添え字アクセスできます。
		/// </summary>
		public MemberTable<Ship> Ships
		{
			get { return this._Ships; }
			set
			{
				if (this._Ships != value)
				{
					this._Ships = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region SlotItems 変更通知プロパティ

		private MemberTable<SlotItem> _SlotItems;

		/// <summary>
		/// 艦隊司令部が保有しているすべての装備を取得します。装備の ID を使用して添え字アクセスできます。
		/// </summary>
		public MemberTable<SlotItem> SlotItems
		{
			get { return this._SlotItems; }
			set
			{
				if (this._SlotItems != value)
				{
					this._SlotItems = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region UseItems 変更通知プロパティ

		private MemberTable<UseItem> _UseItems;

		public MemberTable<UseItem> UseItems
		{
			get { return this._UseItems; }
			set
			{
				if (this._UseItems != value)
				{
					this._UseItems = value;
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

		internal Homeport(KanColleProxy proxy)
		{
			this.Ships = new MemberTable<Ship>();
			this.Fleets = new MemberTable<Fleet>();
			this.SlotItems = new MemberTable<SlotItem>();
			this.UseItems = new MemberTable<UseItem>();
			this.Dockyard = new Dockyard(this, proxy);
			this.Repairyard = new Repairyard(this, proxy);
			this.Logger = new Logger(proxy);
			this.Quests = new Quests(proxy);

			// ToDo: カオスってるので、あとで整理する

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/basic")
				.TryParse<kcsapi_basic>()
				.Subscribe(x => this.Admiral = new Admiral(x));

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_port/port")
				.TryParse<kcsapi_port>()
				.Subscribe(x =>
				{
					this.Ships = new MemberTable<Ship>(x.api_ship.Select(s => new Ship(this, s)));
					this.Materials = new Materials(x.api_material.Select(m => new Material(m)).ToArray());
					this.Repairyard.Update(x.api_ndock);
					this.UpdateFleets(x.api_deck_port);
				});

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_hokyu/charge")
				.TryParse<kcsapi_charge>()
				.Subscribe(this.Charge);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_hensei/change")
				.TryParse()
				.Subscribe(this.FleetChange);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_kaisou/powerup")
				.Select(x => { SvData<kcsapi_powerup> result; return SvData.TryParse(x, out result) ? result : null; })
				.Where(x => x != null && x.IsSuccess)
				.Subscribe(this.Powerup);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_kousyou/destroyship")
				.Select(x => { SvData<kcsapi_destroyship> result; return SvData.TryParse(x, out result) ? result : null; })
				.Where(x => x != null && x.IsSuccess)
				.Subscribe(this.DestroyShip);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_kousyou/destroyitem2")
				.Select(x => { SvData<kcsapi_destroyitem> result; return SvData.TryParse(x, out result) ? result : null; })
				.Where(x => x != null && x.IsSuccess)
				.Subscribe(this.DestroyItem);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/material")
				.TryParse<kcsapi_material[]>()
				.Subscribe(x => this.Materials = new Materials(x.Select(m => new Material(m)).ToArray()));

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/ship")
				.Select(x => { SvData<kcsapi_ship2[]> result; return SvData.TryParse(x, out result) ? result : null; })
				.Where(x => x != null && x.IsSuccess)
				.Subscribe(x => this.Ships = new MemberTable<Ship>(x.Data.Select(s => new Ship(this, s))));

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/ship2")
				.Select(x => { SvData<kcsapi_ship2[]> result; return SvData.TryParse(x, out result) ? result : null; })
				.Where(x => x != null && x.IsSuccess)
				.Subscribe(x =>
				{
					this.Ships = new MemberTable<Ship>(x.Data.Select(s => new Ship(this, s)));
					this.UpdateFleets(x.Fleets);
				});
			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/ship3")
				.TryParse<kcsapi_ship3>()
				.Subscribe(x =>
				{
					this.Ships = new MemberTable<Ship>(x.api_ship_data.Select(s => new Ship(this, s)));
					this.UpdateFleets(x.api_deck_data);
				});

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/slot_item")
				.TryParse<kcsapi_slotitem[]>()
				.Subscribe(x => this.SlotItems = new MemberTable<SlotItem>(x.Select(s => new SlotItem(s))));

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/useitem")
				.TryParse<kcsapi_useitem[]>()
				.Subscribe(x => this.UseItems = new MemberTable<UseItem>(x.Select(s => new UseItem(s))));

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/deck")
				.TryParse<kcsapi_deck[]>()
				.Subscribe(this.UpdateFleets);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/deck_port")
				.TryParse<kcsapi_deck[]>()
				.Subscribe(this.UpdateFleets);

			this.GamePage = new GamePage(proxy);
		}


		private void UpdateFleets(kcsapi_deck[] source)
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
				this.Fleets = new MemberTable<Fleet>(source.Select(x => new Fleet(this, x)));
			}
		}

		private void Charge(kcsapi_charge charge)
		{
			if (charge == null) return;
			
			foreach (var ship in charge.api_ship)
			{
				var target = this.Ships[ship.api_id];
				if (target == null) continue;

				target.Charge(ship.api_fuel, ship.api_bull, ship.api_onslot);
			}

			foreach (var f in Fleets.Values) f.UpdateShips();
		}

		private void FleetChange(SvData svdata)
		{
			int api_id = Int32.Parse(svdata.RequestBody["api_id"]); // 1,2,3,4
			int api_ship_idx = Int32.Parse(svdata.RequestBody["api_ship_idx"]); // 0,1,2,3,4,5
			int api_ship_id = Int32.Parse(svdata.RequestBody["api_ship_id"]);
			var fleet = this.Fleets[api_id];

			if (api_ship_id == -1)
			{
				fleet.RemoveShip(api_ship_idx);
			}
			else
			{
				var lastFleet = this.Fleets.Values.FirstOrDefault(f => f.Ships.Any(s => s.Id == api_ship_id));
				if (lastFleet != null)
				{
					var lastIndex = lastFleet.Ships.Select((s, i) => s.Id == api_ship_id ? i : -1).First(i => i >= 0);

					if (fleet.Ships.Length > api_ship_idx)
					{
						var lastShip = fleet.Ships[api_ship_idx];
						lastFleet.AddShip(lastIndex, lastShip);
					}
					else
					{
						lastFleet.RemoveShip(lastIndex);
					}
				}
				fleet.AddShip(api_ship_idx, this.Ships[api_ship_id]);
			}
		}

		private void Powerup(SvData<kcsapi_powerup> svdata)
		{
			this.UpdateFleets(svdata.RawData.api_data.api_deck);

			var api_ship = svdata.RawData.api_data.api_ship;
			var api_id_items = svdata.RequestBody["api_id_items"].Split(',').Select(Int32.Parse);

			var itemsShips = api_id_items.Select(id => this.Ships[id]);
			this.DeleteShips(itemsShips);

			var ships = this.Ships.Values.ToList();
			ships.Remove(this.Ships[api_ship.api_id]);
			ships.Add(new Ship(this, api_ship));
			this.Ships = new MemberTable<Ship>(ships);
			// TODO: update ship
		}

		private void DestroyShip(SvData<kcsapi_destroyship> svdata)
		{
			var api_ship_id = Int32.Parse(svdata.RequestBody["api_ship_id"]);
			this.DeleteShip(this.Ships[api_ship_id]);
		}

		private void DestroyItem(SvData<kcsapi_destroyitem> svdata)
		{
			var api_slotitem_ids = svdata.RequestBody["api_slotitem_ids"].Split(',').Select(Int32.Parse);
			var slotItems = api_slotitem_ids.Select(i => this.SlotItems[i]);

			this.DeleteSlotItems(api_slotitem_ids.Select(i => this.SlotItems[i]));
		}

		internal void DeleteShip(Ship ship)
		{
			this.DeleteShips(new Ship[] { ship });
		}

		internal void DeleteShips(IEnumerable<Ship> ships)
		{
			var slotItems = ships.SelectMany(s => s.SlotItems);
			this.DeleteSlotItems(slotItems);

			var temp = this.Ships.Values.ToList();
			ships.ForEach(i => temp.Remove(i));
			this.Ships = new MemberTable<Ship>(temp);
		}

		internal void DeleteSlotItems(IEnumerable<SlotItem> slotItems)
		{
			var temp = this.SlotItems.Values.ToList();
			slotItems.ForEach(i => temp.Remove(i));
			this.SlotItems = new MemberTable<SlotItem>(temp);
		}
	}
}
