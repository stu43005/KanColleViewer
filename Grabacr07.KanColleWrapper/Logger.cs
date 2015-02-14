using Codeplex.Data;
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper
{
	public class Logger : NotificationObject
	{
		private bool waitingForShip;
		private int dockid;
		private NameValueCollection createShipRequest;

		internal Logger(KanColleProxy proxy)
		{
			proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(x => this.CreateItem(x.Data, x.Request));
			proxy.api_req_kousyou_createship.TryParse<kcsapi_createship>().Subscribe(x => this.CreateShip(x.Request));
			proxy.api_get_member_kdock.TryParse<kcsapi_kdock[]>().Subscribe(x => this.KDock(x.Data));
			proxy.api_req_sortie_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => this.BattleResult(x.Data));
			proxy.api_req_combined_battle_battleresult.TryParse<kcsapi_combined_battle_battleresult>().Subscribe(x => this.BattleResult(x.Data));

			proxy.api_req_mission_result.TryParse<kcsapi_mission_result>().Subscribe(x => this.MissionResult(x.Data));
		}

		private void CreateItem(kcsapi_createitem item, NameValueCollection req)
		{
			var slotitem_id = item.api_slotitem_id;
			if (slotitem_id == 0 && item.api_slot_item != null)
			{
				slotitem_id = item.api_slot_item.api_slotitem_id;
			}

			this.Log("Create_Item_log.csv",
				"日付,開発装備,種別,燃料,弾薬,鋼材,ボーキ,秘書艦,司令部Lv",
				@"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6},{7},{8}",
				DateTime.Now,
				item.api_create_flag == 1 ? KanColleClient.Current.Master.SlotItems[slotitem_id].Name : "失敗",
				item.api_create_flag == 1 ? KanColleClient.Current.Master.SlotItems[slotitem_id].IconTypeName : "",
				req["api_item1"], req["api_item2"], req["api_item3"], req["api_item4"],
				KanColleClient.Current.Homeport.Organization.Secretary == null ? "" : string.Format("{0}(Lv{1})", KanColleClient.Current.Homeport.Organization.Secretary.Info.Name, KanColleClient.Current.Homeport.Organization.Secretary.Level),
				KanColleClient.Current.Homeport.Admiral.Level);
		}

		private void CreateShip(NameValueCollection req)
		{
			this.waitingForShip = true;
			this.dockid = int.Parse(req["api_kdock_id"]);
			this.createShipRequest = req;
		}

		private void KDock(kcsapi_kdock[] docks)
		{
			foreach (var dock in docks.Where(dock => this.waitingForShip && dock.api_id == this.dockid))
			{
				this.Log("Create_Ship_log.csv",
					"日付,種類,名前,艦種,燃料,弾薬,鋼材,ボーキ,開発資材,空きドック,秘書艦,司令部Lv",
					@"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
					DateTime.Now,
					this.createShipRequest["api_large_flag"] == "1" ? "大型艦建造" : "通常艦建造",
					KanColleClient.Current.Master.Ships[dock.api_created_ship_id].Name,
					KanColleClient.Current.Master.Ships[dock.api_created_ship_id].ShipType.Name,
					dock.api_item1, dock.api_item2, dock.api_item3, dock.api_item4, dock.api_item5,
					docks.Where(d => d.api_state == 0).Count(),
					KanColleClient.Current.Homeport.Organization.Secretary == null ? "" : string.Format("{0}(Lv{1})", KanColleClient.Current.Homeport.Organization.Secretary.Info.Name, KanColleClient.Current.Homeport.Organization.Secretary.Level),
					KanColleClient.Current.Homeport.Admiral.Level);
				this.waitingForShip = false;
			}
		}

		private void BattleResult(kcsapi_battleresult br)
		{
			this.Log("Battle_log.csv",
				"日付,海域,ランク,敵艦隊,ドロップ艦種,ドロップ艦娘",
				@"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5}",
				DateTime.Now,
				br.api_quest_name, br.api_win_rank,
				br.api_enemy_info != null ? br.api_enemy_info.api_deck_name : "",
				br.api_get_ship != null ? br.api_get_ship.api_ship_type : "",
				br.api_get_ship != null ? br.api_get_ship.api_ship_name : "");
		}

		private void BattleResult(kcsapi_combined_battle_battleresult br)
		{
			this.Log("Battle_log.csv",
				"日付,海域,ランク,敵艦隊,ドロップ艦種,ドロップ艦娘",
				@"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5}",
				DateTime.Now,
				br.api_quest_name, br.api_win_rank,
				br.api_enemy_info != null ? br.api_enemy_info.api_deck_name : "",
				br.api_get_ship != null ? br.api_get_ship.api_ship_type : "",
				br.api_get_ship != null ? br.api_get_ship.api_ship_name : "");
		}

		private void MissionResult(kcsapi_mission_result mission)
		{
			int repair = 0, build = 0, develop = 0, coin = 0;

			if (mission.api_get_item1 != null)
			{
				var item = mission.api_get_item1;
				switch (mission.api_useitem_flag[0])
				{
					case 1: // 高速修復材
						repair += item.api_useitem_count;
						break;
					case 2: // 高速建造材
						build += item.api_useitem_count;
						break;
					case 3: // 開発資材
						develop += item.api_useitem_count;
						break;
					case 4: // 家具箱
						switch (item.api_useitem_id)
						{
							case 10: // 家具箱（小）200
								coin += 200 * item.api_useitem_count;
								break;
							case 11: // 家具箱（中）400
								coin += 400 * item.api_useitem_count;
								break;
							case 12: // 家具箱（大）700
								coin += 700 * item.api_useitem_count;
								break;
						}
						break;
				}
			}

			this.Log("Mission_log.csv",
				"日付,結果,遠征,燃料,弾薬,鋼材,ボーキ,高速修復材,高速建造材,開発資材,家具コイン",
				@"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
				DateTime.Now,
				mission.api_clear_result == 0 ? "失敗" : mission.api_clear_result == 2 ? "大成功" : "成功",
				mission.api_quest_name,
				mission.api_get_material[0], mission.api_get_material[1], mission.api_get_material[2], mission.api_get_material[3],
				repair, build, develop, coin);
		}

		private void Log(string filename, string header, string format, params object[] args)
		{
			if (!KanColleClient.Current.Settings.EnableLogging) return;

			const string path = @".\logs\";
			string file = Path.Combine(path, filename);
			Directory.CreateDirectory(path);
			FileInfo f = new FileInfo(file);

			string oldData = "";
			if (f.Exists && f.Length > 3)
			{
				string oldHeader;
				using (StreamReader sr = f.OpenText())
				{
					oldHeader = sr.ReadLine();
					// check file header
					if (!oldHeader.Equals(header))
					{
						oldData = sr.ReadToEnd();
					}
				}
				if (!oldData.Equals(""))
				{
					f.Delete(); // header is change, delete old file
					f.Refresh();
				}
			}

			if (!f.Exists || f.Length <= 3)
			{
				// write file header
				using (FileStream fs = f.Create())
				{
					using (StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(true)))
					{
						sw.WriteLine(header);
						sw.Write(oldData); // if header is change, write old data.
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