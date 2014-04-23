﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleWrapper.Models
{
	/// <summary>
	/// 装備アイテムの種類に基づく情報を表します。
	/// </summary>
	public class SlotItemInfo : RawDataWrapper<kcsapi_mst_slotitem>, IIdentifiable
	{
		private SlotItemIconType? iconType;
		private int? categoryId;

		public int Id
		{
			get { return this.RawData.api_id; }
		}

		public string Name
		{
			get { return this.RawData.api_name; }
		}

		public string Type
		{
			get { return SlotItemType.ItemType[this.IconType]; }
		}

		public SlotItemIconType IconType
		{
			get { return this.iconType ?? (SlotItemIconType)(this.iconType = (SlotItemIconType)(this.RawData.api_type.Get(3) ?? 0)); }
		}

		public int CategoryId
		{
			get { return this.categoryId ?? (int)(this.categoryId = this.RawData.api_type.Get(2) ?? int.MaxValue); }
		}

		/// <summary>
		/// 火力値を取得します。
		/// </summary>
		public int Firepower
		{
			get { return this.RawData.api_houg; }
		}

		/// <summary>
		/// 雷装値を取得します。
		/// </summary>
		public int Torpedo
		{
			get { return this.RawData.api_raig; }
		}

		/// <summary>
		/// 爆装値を取得します。
		/// </summary>
		public int DiveBomb
		{
			get { return this.RawData.api_baku; }
		}

		/// <summary>
		/// 対空値を取得します。
		/// </summary>
		public int AA
		{
			get { return this.RawData.api_tyku; }
		}

		/// <summary>
		/// 対潜値を取得します。
		/// </summary>
		public int AS
		{
			get { return this.RawData.api_tais; }
		}

		/// <summary>
		/// 索敵値を取得します。
		/// </summary>
		public int SightRange
		{
			get { return this.RawData.api_saku; }
		}

		/// <summary>
		/// 命中値を取得します。
		/// </summary>
		public int Accuracy
		{
			get { return this.RawData.api_houm; }
		}

		/// <summary>
		/// 回避値を取得します。
		/// </summary>
		public int Evasion
		{
			get { return this.RawData.api_houk; }
		}

		/// <summary>
		/// 装甲値を取得します。
		/// </summary>
		public int Armor
		{
			get { return this.RawData.api_souk; }
		}

		/// <summary>
		/// 射程値を取得します。
		/// </summary>
		public int AttackRange
		{
			get { return this.RawData.api_leng; }
		}

		/// <summary>
		/// この装備アイテムが艦載機かどうかを示す値を取得します。
		/// </summary>
		public bool IsAircraft
		{
			get
			{
				return this.IconType == SlotItemIconType.Fighter ||
					   this.IconType == SlotItemIconType.TorpedoBomber ||
					   this.IconType == SlotItemIconType.DiveBomber ||
					   this.IconType == SlotItemIconType.ReconPlane;
			}
		}

		/// <summary>
		/// この装備アイテムが水上機かどうかを示す値を取得します。
		/// </summary>
		public bool IsSeaplane
		{
			get { return this.IconType == SlotItemIconType.ReconSeaplane; }
		}

		internal SlotItemInfo(kcsapi_mst_slotitem rawData) : base(rawData) { }

		public override string ToString()
		{
			return string.Format("ID = {0}, Name = \"{1}\", Type = {{{2}}}", this.Id, this.Name, this.RawData.api_type.ToString(", "));
		}

		#region static members

		private static readonly SlotItemInfo dummy = new SlotItemInfo(new kcsapi_mst_slotitem()
		{
			api_id = 0,
			api_name = "？？？",
		});

		public static SlotItemInfo Dummy
		{
			get { return dummy; }
		}

		private static readonly SlotItemInfo empty = new SlotItemInfo(new kcsapi_mst_slotitem()
		{
			api_id = -1,
			api_name = "Empty",
		})
		{
			iconType = SlotItemIconType.Empty,
		};

		public static SlotItemInfo Empty
		{
			get { return empty; }
		}

		#endregion
	}
}
