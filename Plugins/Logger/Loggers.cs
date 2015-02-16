using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
	public abstract class LoggerBase : NotificationObject
	{
		public abstract string Title { get; }
		public abstract string Description { get; }
		public abstract string FileName { get; }
		public abstract string Header { get; }
		public abstract string Format { get; }
		public abstract bool Enabled { get; set; }

		public void Log(params object[] args)
		{
			if (!this.Enabled) return;

			const string path = @".\logs\";
			string file = Path.Combine(path, this.FileName);
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
					if (!oldHeader.Equals(this.Header))
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
						sw.WriteLine(this.Header);
						sw.Write(oldData); // if header is change, write old data.
					}
				}
			}

			using (StreamWriter sw = f.AppendText())
			{
				sw.WriteLine(this.Format, args);
			}
		}
	}

	public class CreateItemLogger : LoggerBase
	{
		public override string Title
		{
			get { return "Create Item"; }
		}

		public override string Description
		{
			get { return "開發裝備結果紀錄"; }
		}

		public override string FileName
		{
			get { return "Create_Item_log.csv"; }
		}

		public override string Header
		{
			get { return "日付,開発装備,種別,燃料,弾薬,鋼材,ボーキ,秘書艦,司令部Lv"; }
		}

		public override string Format
		{
			get { return @"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6},{7},{8}"; }
		}

		public override bool Enabled
		{
			get { return LoggerSettings.Current.EnableCreateItemLogging; }
			set
			{
				LoggerSettings.Current.EnableCreateItemLogging = value;
				LoggerSettings.Current.Save();
			}
		}

		public CreateItemLogger(KanColleProxy proxy)
		{
			proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(x => this.CreateItem(x.Data, x.Request));
		}

		private void CreateItem(kcsapi_createitem item, NameValueCollection req)
		{
			var slotitem_id = item.api_slotitem_id;
			if (slotitem_id == 0 && item.api_slot_item != null)
			{
				slotitem_id = item.api_slot_item.api_slotitem_id;
			}

			this.Log(DateTime.Now,
				item.api_create_flag == 1 ? KanColleClient.Current.Master.SlotItems[slotitem_id].Name : "失敗",
				item.api_create_flag == 1 ? KanColleClient.Current.Master.SlotItems[slotitem_id].IconTypeName : "",
				req["api_item1"], req["api_item2"], req["api_item3"], req["api_item4"],
				KanColleClient.Current.Homeport.Organization.Secretary == null ? "" : string.Format("{0}(Lv{1})", KanColleClient.Current.Homeport.Organization.Secretary.Info.Name, KanColleClient.Current.Homeport.Organization.Secretary.Level),
				KanColleClient.Current.Homeport.Admiral.Level);
		}
	}

	public class CreateShipLogger : LoggerBase
	{
		public override string Title
		{
			get { return "Create Ship"; }
		}

		public override string Description
		{
			get { return "建造艦娘結果紀錄"; }
		}

		public override string FileName
		{
			get { return "Create_Ship_log.csv"; }
		}

		public override string Header
		{
			get { return "日付,種類,名前,艦種,燃料,弾薬,鋼材,ボーキ,開発資材,空きドック,秘書艦,司令部Lv"; }
		}

		public override string Format
		{
			get { return @"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}"; }
		}

		public override bool Enabled
		{
			get { return LoggerSettings.Current.EnableCreateItemLogging; }
			set
			{
				LoggerSettings.Current.EnableCreateItemLogging = value;
				LoggerSettings.Current.Save();
			}
		}

		private bool waitingForShip;
		private int dockid;
		private NameValueCollection createShipRequest;

		public CreateShipLogger(KanColleProxy proxy)
		{
			proxy.api_req_kousyou_createship.TryParse<kcsapi_createship>().Subscribe(x => this.CreateShip(x.Request));
			proxy.api_get_member_kdock.TryParse<kcsapi_kdock[]>().Subscribe(x => this.KDock(x.Data));
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
				this.Log(DateTime.Now,
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
	}

	public class BattleLogger : LoggerBase
	{
		public override string Title
		{
			get { return "Battle Results"; }
		}

		public override string Description
		{
			get { return "出擊掉落獲得艦娘紀錄"; }
		}

		public override string FileName
		{
			get { return "Battle_log.csv"; }
		}

		public override string Header
		{
			get { return "日付,海域,ランク,敵艦隊,ドロップ艦種,ドロップ艦娘"; }
		}

		public override string Format
		{
			get { return @"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5}"; }
		}

		public override bool Enabled
		{
			get { return LoggerSettings.Current.EnableBattleLogging; }
			set
			{
				LoggerSettings.Current.EnableBattleLogging = value;
				LoggerSettings.Current.Save();
			}
		}

		public BattleLogger(KanColleProxy proxy)
		{
			proxy.api_req_sortie_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => this.BattleResult(x.Data));
			proxy.api_req_combined_battle_battleresult.TryParse<kcsapi_combined_battle_battleresult>().Subscribe(x => this.BattleResult(x.Data));
		}

		private void BattleResult(kcsapi_battleresult br)
		{
			this.Log(DateTime.Now,
				br.api_quest_name, br.api_win_rank,
				br.api_enemy_info != null ? br.api_enemy_info.api_deck_name : "",
				br.api_get_ship != null ? br.api_get_ship.api_ship_type : "",
				br.api_get_ship != null ? br.api_get_ship.api_ship_name : "");
		}

		private void BattleResult(kcsapi_combined_battle_battleresult br)
		{
			this.Log(DateTime.Now,
				br.api_quest_name, br.api_win_rank,
				br.api_enemy_info != null ? br.api_enemy_info.api_deck_name : "",
				br.api_get_ship != null ? br.api_get_ship.api_ship_type : "",
				br.api_get_ship != null ? br.api_get_ship.api_ship_name : "");
		}
	}

	public class MissionLogger : LoggerBase
	{
		public override string Title
		{
			get { return "Mission"; }
		}

		public override string Description
		{
			get { return "遠征紀錄"; }
		}

		public override string FileName
		{
			get { return "Mission_log.csv"; }
		}

		public override string Header
		{
			get { return "日付,結果,遠征,燃料,弾薬,鋼材,ボーキ,高速修復材,高速建造材,開発資材,家具コイン"; }
		}

		public override string Format
		{
			get { return @"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}"; }
		}

		public override bool Enabled
		{
			get { return LoggerSettings.Current.EnableCreateItemLogging; }
			set
			{
				LoggerSettings.Current.EnableCreateItemLogging = value;
				LoggerSettings.Current.Save();
			}
		}

		public MissionLogger(KanColleProxy proxy)
		{
			proxy.api_req_mission_result.TryParse<kcsapi_mission_result>().Subscribe(x => this.MissionResult(x.Data));
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

			this.Log(DateTime.Now,
				mission.api_clear_result == 0 ? "失敗" : mission.api_clear_result == 2 ? "大成功" : "成功",
				mission.api_quest_name,
				mission.api_get_material[0], mission.api_get_material[1], mission.api_get_material[2], mission.api_get_material[3],
				repair, build, develop, coin);
		}
	}

	public class MaterialsLogger : LoggerBase
	{
		public override string Title
		{
			get { return "Materials"; }
		}

		public override string Description
		{
			get { return "資材開支紀錄"; }
		}

		public override string FileName
		{
			get { return "Materials_log.csv"; }
		}

		public override string Header
		{
			get { return "日付,燃料,弾薬,鋼材,ボーキ,高速建造材,高速修復材,開発資材,改修資材"; }
		}

		public override string Format
		{
			get { return @"{0:yyyy-MM-dd HH\:mm\:ss},{1},{2},{3},{4},{5},{6},{7},{8}"; }
		}

		public override bool Enabled
		{
			get { return LoggerSettings.Current.EnableCreateItemLogging; }
			set
			{
				LoggerSettings.Current.EnableCreateItemLogging = value;
				LoggerSettings.Current.Save();
			}
		}

		public MaterialsLogger(KanColleProxy proxy)
		{
			//proxy.api_port.TryParse<kcsapi_port>().Subscribe(x => this.MaterialsHistory(x.Data.api_material));
			proxy.api_get_member_material.TryParse<kcsapi_material[]>().Subscribe(x => this.MaterialsHistory(x.Data));
			proxy.api_req_hokyu_charge.TryParse<kcsapi_charge>().Subscribe(x => this.MaterialsHistory(x.Data.api_material));
			proxy.api_req_kousyou_destroyship.TryParse<kcsapi_destroyship>().Subscribe(x => this.MaterialsHistory(x.Data.api_material));
		}

		private void MaterialsHistory(kcsapi_material[] source)
		{
			if (source != null && source.Length >= 7)
			{
				this.Log(DateTime.Now,
					source[0].api_value, source[1].api_value, source[2].api_value, source[3].api_value,
					source[4].api_value, source[5].api_value, source[6].api_value,
					source.Length > 7 ? source[7].api_value : KanColleClient.Current.Homeport.Materials.RevampingMaterials);
			}
		}

		private void MaterialsHistory(int[] source)
		{
			if (source != null && source.Length == 4)
			{
				this.Log(DateTime.Now,
					source[0], source[1], source[2], source[3],
					KanColleClient.Current.Homeport.Materials.InstantBuildMaterials,
					KanColleClient.Current.Homeport.Materials.InstantRepairMaterials,
					KanColleClient.Current.Homeport.Materials.DevelopmentMaterials,
					KanColleClient.Current.Homeport.Materials.RevampingMaterials);
			}
		}
	}
}
