﻿using Codeplex.Data;
using Fiddler;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper
{
	public class Logger : NotificationObject
	{
		private bool waitingForKShip = false;
		private int kdockid = 0;
		private NameValueCollection createShipRequest;

		internal Logger(KanColleProxy proxy)
		{
			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_kousyou/createitem")
				.Select(x => { SvData<kcsapi_createitem> result; return SvData.TryParse(x, out result) ? result : null; })
				.Where(x => x != null && x.IsSuccess)
				.Subscribe(x => this.CreateItem(x.Data, x.RequestBody));

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_kousyou/createship")
				.TryParse()
				.Subscribe(this.CreateShip);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/kdock")
				.TryParse<kcsapi_kdock[]>()
				.Subscribe(this.KDock);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_sortie/battleresult")
				.TryParse<kcsapi_battleresult>()
				.Subscribe(this.BattleResult);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_mission/result")
				.Select(MissionResultSerialize)
				.Where(x => x != null)
				.Subscribe(this.MissionResult);
		}

		private void CreateItem(kcsapi_createitem item, NameValueCollection req)
		{
			Log("Create_Item_log.csv",
				"日付,開発装備,種別,燃料,弾薬,鋼材,ボーキ,秘書艦,司令部Lv",
				@"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6},{7},{8}",
				DateTime.Now,
				item.api_create_flag == 1 ? KanColleClient.Current.Master.SlotItems[item.api_slotitem_id].Name : "失敗",
				item.api_create_flag == 1 ? KanColleClient.Current.Master.SlotItems[item.api_slotitem_id].Type : "",
				req["api_item1"], req["api_item2"], req["api_item3"], req["api_item4"],
				KanColleClient.Current.Homeport.Secretary == null ? "" : string.Format("{0}(Lv{1})", KanColleClient.Current.Homeport.Secretary.Info.Name, KanColleClient.Current.Homeport.Secretary.Level),
				KanColleClient.Current.Homeport.Admiral.Level);
		}

		private void CreateShip(SvData data)
		{
			this.waitingForKShip = true;
			this.kdockid = Int32.Parse(data.RequestBody["api_kdock_id"]);
			this.createShipRequest = data.RequestBody;
		}

		private void KDock(kcsapi_kdock[] kdocks)
		{
			if (this.waitingForKShip)
			{
				int freecount = 0;
				foreach (var kdock in kdocks)
				{
					if (kdock.api_state == 0)
						freecount++;
				}

				foreach (var kdock in kdocks)
				{
					if (kdock.api_id == this.kdockid)
					{
						Log("Create_Ship_log.csv",
							"日付,種類,名前,艦種,燃料,弾薬,鋼材,ボーキ,開発資材,空きドック,秘書艦,司令部Lv",
							@"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
							DateTime.Now,
							this.createShipRequest["api_large_flag"] == "1" ? "大型艦建造" : "通常艦建造",
							KanColleClient.Current.Master.Ships[kdock.api_created_ship_id].Name,
							KanColleClient.Current.Master.Ships[kdock.api_created_ship_id].ShipType.Name,
							kdock.api_item1, kdock.api_item2, kdock.api_item3, kdock.api_item4, kdock.api_item5, freecount,
							KanColleClient.Current.Homeport.Secretary == null ? "" : string.Format("{0}(Lv{1})", KanColleClient.Current.Homeport.Secretary.Info.Name, KanColleClient.Current.Homeport.Secretary.Level),
							KanColleClient.Current.Homeport.Admiral.Level);
						this.waitingForKShip = false;
						break;
					}
				}
			}
		}

		private void BattleResult(kcsapi_battleresult br)
		{
			Log("Battle_log.csv",
				"日付,海域,ランク,敵艦隊,ドロップ艦種,ドロップ艦娘",
				@"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5}",
				DateTime.Now,
				br.api_quest_name, br.api_win_rank,
				br.api_enemy_info != null ? br.api_enemy_info.api_deck_name : "",
				br.api_get_ship != null ? br.api_get_ship.api_ship_type : "",
				br.api_get_ship != null ? br.api_get_ship.api_ship_name : "");
		}

		private static kcsapi_missionresult MissionResultSerialize(Session session)
		{
			try
			{
				var djson = DynamicJson.Parse(session.GetResponseAsJson());

				int[] api_get_material;
				if (Object.ReferenceEquals(djson.api_data.api_get_material.GetType(), (0.0).GetType()))
					api_get_material = new int[] { 0, 0, 0, 0 };
				else
					api_get_material = djson.api_data.api_get_material;

				var missionresult = new kcsapi_missionresult
				{
					api_clear_result = Convert.ToInt32(djson.api_data.api_clear_result),
					api_quest_name = Convert.ToString(djson.api_data.api_quest_name),
					api_get_material = api_get_material,
				};

				return missionresult;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return null;
			}
		}

		private void MissionResult(kcsapi_missionresult mission)
		{
			Log("Mission_log.csv",
				"日付,結果,遠征,燃料,弾薬,鋼材,ボーキ",
				@"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6}",
				DateTime.Now,
				mission.api_clear_result == 0 ? "失敗" : mission.api_clear_result == 2 ? "大成功" : "成功",
				mission.api_quest_name,
				mission.api_get_material[0], mission.api_get_material[1], mission.api_get_material[2], mission.api_get_material[3]);
		}

		private void Log(string filename, string header, string format, params object[] args)
		{
			const string path = @".\logs\";
			string file = Path.Combine(path, filename);
			Directory.CreateDirectory(path);
			FileInfo f = new FileInfo(file);
			
			if (!f.Exists || f.Length <= 0)
			{
				using (FileStream fs = f.Create())
				{
					using (StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(true)))
					{
						sw.WriteLine(header);
					}
				}
			}

			using (StreamWriter sw = f.AppendText())
			{
				sw.WriteLine(format, args);
			}
		}
	}
}