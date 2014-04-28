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
		public bool EnableLogging { get; set; }

		private bool waitingForShip;
		private int dockid;
		private NameValueCollection createShipRequest;

		internal Logger(KanColleProxy proxy)
		{
			proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(x => this.CreateItem(x.Data, x.Request));
			proxy.api_req_kousyou_createship.TryParse<kcsapi_createship>().Subscribe(x => this.CreateShip(x.Request));
			proxy.api_get_member_kdock.TryParse<kcsapi_kdock[]>().Subscribe(x => this.KDock(x.Data));
			proxy.api_req_sortie_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => this.BattleResult(x.Data));

			proxy.api_req_mission_result
				.Select(MissionResultSerialize)
				.Where(x => x != null)
				.Subscribe(this.MissionResult);
		}

		private void CreateItem(kcsapi_createitem item, NameValueCollection req)
		{
			this.Log("Create_Item_log.csv",
				"日付,開発装備,種別,燃料,弾薬,鋼材,ボーキ,秘書艦,司令部Lv",
				@"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6},{7},{8}",
				DateTime.Now,
				item.api_create_flag == 1 ? KanColleClient.Current.Master.SlotItems[item.api_slotitem_id].Name : "失敗",
				item.api_create_flag == 1 ? KanColleClient.Current.Master.SlotItems[item.api_slotitem_id].Type : "",
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

		private static kcsapi_missionresult MissionResultSerialize(Session session)
		{
			try
			{
				var djson = DynamicJson.Parse(session.GetResponseAsJson());

				int[] api_get_material;
				if (djson.api_data.api_get_material is double)
					api_get_material = new int[] { 0, 0, 0, 0 };
				else
					api_get_material = djson.api_data.api_get_material;

				var missionresult = new kcsapi_missionresult
				{
					api_clear_result = Convert.ToInt32(djson.api_data.api_clear_result),
					api_quest_name = Convert.ToString(djson.api_data.api_quest_name),
					api_get_material = api_get_material,
					api_useitem_flag = djson.api_data.api_useitem_flag.Deserialize<int[]>(),
				};

				if (djson.api_data.IsDefined("api_get_item1"))
				{
					var list = new List<kcsapi_mission_getitem>();
					var serializer = new DataContractJsonSerializer(typeof(kcsapi_mission_getitem));

					try
					{
						list.Add(serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(djson.api_data.api_get_item1.ToString()))) as kcsapi_mission_getitem);
					}
					catch (SerializationException ex)
					{
						Debug.WriteLine(ex.Message);
					}

					// TODO: is api_get_item2 ?
					if (djson.api_data.IsDefined("api_get_item2"))
					{
						try
						{
							list.Add(serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(djson.api_data.api_get_item2.ToString()))) as kcsapi_mission_getitem);
						}
						catch (SerializationException ex)
						{
							Debug.WriteLine(ex.Message);
						}
					}

					missionresult.api_get_item = list.ToArray();
				}

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
			int repair = 0, build = 0, develop = 0, coin = 0;

			if (mission.api_get_item != null)
			{
				for (int i = 0; i < mission.api_useitem_flag.Length && i < mission.api_get_item.Length; i++)
				{
					var item = mission.api_get_item[i];
					switch (mission.api_useitem_flag[i])
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
			if (!this.EnableLogging) return;

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