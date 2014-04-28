using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
		/// <summary>
		/// 艦隊の編成状況にアクセスできるようにします。
		/// </summary>
		public Organization Organization { get; private set; }

		/// <summary>
		/// 資源および資材の保有状況にアクセスできるようにします。
		/// </summary>
		public Materials Materials { get; private set; }

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
		/// <see cref="INotifyPropertyChanged.PropertyChanged"/> イベントによる変更通知をサポートします。
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

		#region SlotItems 変更通知プロパティ

		private MemberTable<SlotItem> _SlotItems;

		/// <summary>
		/// 艦隊司令部が保有しているすべての装備を取得します。
		/// <see cref="INotifyPropertyChanged.PropertyChanged"/> イベントによる変更通知をサポートします。
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

		/// <summary>
		/// 母港が所有するすべての消費アイテムを取得します。
		/// <see cref="INotifyPropertyChanged.PropertyChanged"/> イベントによる変更通知をサポートします。
		/// </summary>
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
			this.SlotItems = new MemberTable<SlotItem>();
			this.UseItems = new MemberTable<UseItem>();

			this.Materials = new Materials();
			this.Organization = new Organization(this, proxy);
			this.Repairyard = new Repairyard(this, proxy);
			this.Dockyard = new Dockyard(this, proxy);
			this.Quests = new Quests(proxy);
			this.Logger = new Logger(proxy);
			this.GamePage = new GamePage(proxy);

			proxy.api_port.TryParse<kcsapi_port>().Subscribe(x =>
			{
				this.Organization.Update(x.Data.api_ship);
				this.Repairyard.Update(x.Data.api_ndock);
				this.Organization.Update(x.Data.api_deck_port);
				this.Materials.Update(x.Data.api_material);
			});
			proxy.api_get_member_basic.TryParse<kcsapi_basic>().Subscribe(x => this.UpdateAdmiral(x.Data));
			proxy.api_get_member_slot_item.TryParse<kcsapi_slotitem[]>().Subscribe(x => this.UpdateSlotItems(x.Data));
			proxy.api_get_member_useitem.TryParse<kcsapi_useitem[]>().Subscribe(x => this.UpdateUseItems(x.Data));
			proxy.api_get_member_material.TryParse<kcsapi_material[]>().Subscribe(x => this.Materials.Update(x.Data));

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_kousyou/destroyship")
				.Select(x => { SvData<kcsapi_destroyship> result; return SvData.TryParse(x, out result) ? result : null; })
				.Where(x => x != null && x.IsSuccess)
				.Subscribe(this.DestroyShip);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_kousyou/destroyitem2")
				.Select(x => { SvData<kcsapi_destroyitem> result; return SvData.TryParse(x, out result) ? result : null; })
				.Where(x => x != null && x.IsSuccess)
				.Subscribe(this.DestroyItem);
		}


		internal void UpdateAdmiral(kcsapi_basic data)
		{
			this.Admiral = new Admiral(data);
		}

		internal void UpdateSlotItems(kcsapi_slotitem[] source)
		{
			this.SlotItems = new MemberTable<SlotItem>(source.Select(x => new SlotItem(x)));
		}

		internal void UpdateUseItems(kcsapi_useitem[] source)
		{
			this.UseItems = new MemberTable<UseItem>(source.Select(x => new UseItem(x)));
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
