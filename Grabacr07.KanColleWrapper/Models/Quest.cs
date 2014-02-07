using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleWrapper.Models
{
	public class Quest : RawDataWrapper<kcsapi_quest>, IIdentifiable
	{
		public int Id
		{
			get { return this.RawData.api_no; }
		}

		/// <summary>
		/// 任務のカテゴリ (編成、出撃、演習 など) を取得します。
		/// </summary>
		public QuestCategory Category
		{
			get { return (QuestCategory)this.RawData.api_category; }
		}

		/// <summary>
		/// 任務の種類 (1 回のみ、デイリー、ウィークリー) を取得します。
		/// </summary>
		public QuestType Type
		{
			get { return (QuestType)this.RawData.api_type; }
		}

		/// <summary>
		/// 任務の状態を取得します。
		/// </summary>
		public QuestState State
		{
			get { return (QuestState)this.RawData.api_state; }
		}

		/// <summary>
		/// 任務の進捗状況を取得します。
		/// </summary>
		public QuestProgress Progress
		{
			get { return (QuestProgress)this.RawData.api_progress_flag; }
		}

		/// <summary>
		/// 任務名を取得します。
		/// </summary>
		public string Title
		{
			get { return this.RawData.api_title; }
		}

		/// <summary>
		/// 任務の詳細を取得します。
		/// </summary>
		public string Detail
		{
			get { return this.RawData.api_detail; }
		}


		public Quest(kcsapi_quest rawData) : base(rawData)
		{
			if ((QuestCategory)rawData.api_category == QuestCategory.Composition && (QuestState)rawData.api_state == QuestState.TakeOn)
			{
				CompositionQuest q = KanColleClient.Current.Master.CompositionQuests[rawData.api_no];
				if (q != null)
				{
					ShipInfo[] shipinfo = KanColleClient.Current.Homeport.Ships.Values.Select(x => x.Info).Distinct(x => x.Id).ToArray();
					int i;
					for (i = 0; i < q.ShipIds.Length; i++)
					{
						int j;
						for (j = 0; j < shipinfo.Length; j++)
						{
							if (shipinfo[j].Id == q.ShipIds[i])
							{
								break;
							}
						}
						if (shipinfo.Length == j)
							break;
					}
					if (i == q.ShipIds.Length)
					{
						rawData.api_state = (int)QuestState.Accomplished;
					}
				}
			}
		}


		public override string ToString()
		{
			return string.Format("ID = {0}, Category = {1}, Title = \"{2}\", Type = {3}, State = {4}", this.Id, this.Category, this.Title, this.Type, this.State);
		}
	}
}
