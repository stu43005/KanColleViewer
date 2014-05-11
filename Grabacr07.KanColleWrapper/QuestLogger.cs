using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper
{
	public class QuestLogger : NotificationObject
	{
		private Quests quests;

		private QuestLog _Log;
		public QuestLog Log
		{
			get { return this._Log ?? (this._Log = this.Load()); }
		}

		private string filename
		{
			get { return string.Format(@".\questlog\{0}.json", KanColleClient.Current.Homeport.Admiral.MemberId); }
		}


		internal QuestLogger(Quests parent, KanColleProxy proxy)
		{
			this.quests = parent;

			proxy.api_req_sortie_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => this.BattleResult(x.Data));
		}

		private bool HasQuest(int id)
		{
			return this.quests.Current.FirstOrDefault(q => q.Id == id) != null;
		}

		private void BattleResult(kcsapi_battleresult br)
		{
			if (this.HasQuest(201)) // 敵艦隊を撃破せよ！
			{
				if (br.api_win_rank == "S" || br.api_win_rank == "A" || br.api_win_rank == "B")
				{
					this.Log.daily_b1++;
				}
			}
			if (this.HasQuest(216)) // 敵艦隊主力を撃滅せよ！
			{
				this.Log.daily_b2++;
			}
			if (this.HasQuest(210)) // 敵艦隊を10回邀撃せよ！
			{
				this.Log.daily_b3++;
			}
			if (this.HasQuest(226)) // 南西諸島海域の制海権を握れ！
			{
				string[][] quests = new[] {
					new[] {"カムラン半島","敵主力艦隊"},
					new[] {"バシー島沖", "敵通商破壊艦隊"},
					new[] {"東部オリョール海", "敵主力打撃群"},
					new[] {"沖ノ島海域", "敵侵攻中核艦隊"},
				};
				if (quests.Any(quest => quest[0] == br.api_quest_name && quest[1] == br.api_enemy_info.api_deck_name))
				{
					if (br.api_win_rank == "S" || br.api_win_rank == "A" || br.api_win_rank == "B")
					{
						this.Log.daily_b7++;
					}
				}
			}

			if (this.HasQuest(214)) // あ号作戦
			{

			}
		}

		private QuestLog Load()
		{
			try
			{
				var serializer = new DataContractJsonSerializer(typeof(QuestLog));
				using (var stream = File.OpenRead(filename))
				{
					var result = serializer.ReadObject(stream) as QuestLog;
					return result;
				}
			}
			catch (FileNotFoundException)
			{
				return new QuestLog();
			}
		}

		private void Save()
		{
			var serializer = new DataContractJsonSerializer(typeof(QuestLog));
			using (var stream = new FileStream(filename, FileMode.Create))
			{
				serializer.WriteObject(stream, this.Log);
			}
		}
	}
}
