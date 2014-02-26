using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models.Raw
{
	// ReSharper disable InconsistentNaming
	public class kcsapi_master_slotitem
	{
		public int api_id { get; set; }
		public int api_sortno { get; set; }
		public string api_name { get; set; }
		public int[] api_type { get; set; }
		public int api_taik { get; set; }
		public int api_souk { get; set; } // 装甲
		public int api_houg { get; set; } // 火力
		public int api_raig { get; set; } // 雷装
		public int api_soku { get; set; }
		public int api_baku { get; set; } // 爆装
		public int api_tyku { get; set; } // 対空
		public int api_tais { get; set; } // 対潜
		public int api_atap { get; set; }
		public int api_houm { get; set; } // 命中
		public int api_raim { get; set; } // 
		public int api_houk { get; set; } // 回避
		public int api_raik { get; set; }
		public int api_bakk { get; set; }
		public int api_saku { get; set; } // 索敵
		public int api_sakb { get; set; }
		public int api_luck { get; set; }
		public int api_leng { get; set; } // 射程: -,短,中,長,超長
		public int api_rare { get; set; } // 
		public int[] api_broken { get; set; }
		public string api_info { get; set; }
		public string api_usebull { get; set; }
	}
	// ReSharper restore InconsistentNaming
}
