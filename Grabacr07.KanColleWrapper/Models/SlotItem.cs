﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleWrapper.Models
{
	public class SlotItem : RawDataWrapper<kcsapi_slotitem>, IIdentifiable
	{
		public int Id
		{
			get { return this.RawData.api_id; }
		}

		public SlotItemInfo Info { get; private set; }

		public int Level
		{
			get { return this.RawData.api_level; }
		}

		public string LevelText
		{
			get { return this.Level >= 10 ? "★max" : this.Level >= 1 ? ("★+" + this.Level) : ""; }
		}

		public string NameWithLevel
		{
			get { return string.Format("{0}{1}", this.Info.Name, this.Level >= 1 ? (" " + this.LevelText) : ""); }
		}

		internal SlotItem(kcsapi_slotitem rawData)
			: base(rawData)
		{
			this.Info = KanColleClient.Current.Master.SlotItems[this.RawData.api_slotitem_id] ?? SlotItemInfo.Dummy;
		}

		public override string ToString()
		{
			return string.Format("ID = {0}, Name = \"{1}\", Level = {2}", this.Id, this.Info.Name, this.Level);
		}

		#region static members

		private static readonly SlotItem empty = new SlotItem(new kcsapi_slotitem()
		{
			api_id = -1,
		})
		{
			Info = SlotItemInfo.Empty,
		};

		public static SlotItem Empty
		{
			get { return empty; }
		}

		#endregion
	}
}
