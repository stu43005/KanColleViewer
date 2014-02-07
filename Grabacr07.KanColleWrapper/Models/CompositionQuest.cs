using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models
{
	public class CompositionQuest : IIdentifiable
	{
		public int Id { get; set; }
		public int[] ShipIds { get; set; }

		public static readonly IReadOnlyList<CompositionQuest> List = new List<CompositionQuest>
		{
			new CompositionQuest {
				Id = 108, //「天龍」型軽巡姉妹の全２艦を編成せよ！
				ShipIds = new int[] {51, 52}
			},
			new CompositionQuest {
				Id = 111, //「扶桑」型戦艦姉妹の全２隻を編成せよ！
				ShipIds = new int[] {26, 27}
			},
			new CompositionQuest {
				Id = 112, //「伊勢」型戦艦姉妹の全２隻を編成せよ！
				ShipIds = new int[] {77, 87}
			},
			new CompositionQuest {
				Id = 114, //「南雲機動部隊」を編成せよ！
				ShipIds = new int[] {83, 84, 90, 91}
			},
			new CompositionQuest {
				Id = 118, //「金剛」型による高速戦艦部隊を編成せよ！
				ShipIds = new int[] {78, 79, 85, 86}
			},
			new CompositionQuest {
				Id = 119, //「三川艦隊」を編成せよ！
				ShipIds = new int[] {51, 59, 60, 61, 69}
			},
			new CompositionQuest {
				Id = 128, //「第六戦隊」を編成せよ！
				ShipIds = new int[] {59, 60, 61, 123}
			}
		};
	}
}
