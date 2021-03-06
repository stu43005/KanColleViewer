﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models
{
	public enum SlotItemIconType
	{
		Empty = -1,
		Unknown = 0,

		/// <summary>
		/// 小口径主砲。
		/// </summary>
		MainCanonLight = 1,

		/// <summary>
		/// 中口径主砲。
		/// </summary>
		MainCanonMedium = 2,

		/// <summary>
		/// 大口径主砲。
		/// </summary>
		MainCanonHeavy = 3,

		/// <summary>
		/// 副砲。
		/// </summary>
		SecondaryCanon = 4,

		/// <summary>
		/// 魚雷。
		/// </summary>
		Torpedo = 5,

		/// <summary>
		/// 艦戦。
		/// </summary>
		Fighter = 6,

		/// <summary>
		/// 艦爆。
		/// </summary>
		DiveBomber = 7,

		/// <summary>
		/// 艦攻。
		/// </summary>
		TorpedoBomber = 8,

		/// <summary>
		/// 偵察機。
		/// </summary>
		ReconPlane = 9,

		/// <summary>
		/// 水上機。
		/// </summary>
		ReconSeaplane = 10,

		/// <summary>
		/// 電探。
		/// </summary>
		Rader = 11,

		/// <summary>
		/// 三式弾。
		/// </summary>
		AAShell = 12,

		/// <summary>
		/// 徹甲弾。
		/// </summary>
		APShell = 13,

		/// <summary>
		/// ダメコン。
		/// </summary>
		DamageControl = 14,

		/// <summary>
		/// 機銃。
		/// </summary>
		AAGun = 15,

		/// <summary>
		/// 高角砲。
		/// </summary>
		HighAngleGun = 16,

		/// <summary>
		/// 爆雷投射機。
		/// </summary>
		ASW = 17,

		/// <summary>
		/// ソナー。
		/// </summary>
		Soner = 18,

		/// <summary>
		/// 機関部強化。
		/// </summary>
		EngineImprovement = 19,

		/// <summary>
		/// 上陸用舟艇。
		/// </summary>
		LandingCraft = 20,

		/// <summary>
		/// オートジャイロ。
		/// </summary>
		Autogyro = 21,

		/// <summary>
		/// 指揮連絡機。
		/// </summary>
		ArtillerySpotter = 22,

		/// <summary>
		/// 増設バルジ。
		/// </summary>
		AntiTorpedoBulge = 23,

		/// <summary>
		/// 探照灯。
		/// </summary>
		Searchlight = 24,

		/// <summary>
		/// ドラム缶。
		/// </summary>
		DrumCanister = 25,

		/// <summary>
		/// 施設。
		/// </summary>
		Facility = 26,

		/// <summary>
		/// 照明弾。
		/// </summary>
		Flare = 27,

		/// <summary>
		/// 司令部施設。
		/// </summary>
		FleetCommandFacility = 28,

		/// <summary>
		/// 航空要員。
		/// </summary>
		MaintenancePersonnel = 29,

        /// <summary>
        /// 高射砲。
        /// </summary>
        AntiAircraftFireDirector = 30,

        /// <summary>
        /// ロケットランチャー。
        /// </summary>
        RocketLauncher = 31,

		/// <summary>
		/// 水上艦要員。
		/// </summary>
		SurfaceShipPersonnel = 32,

		/// <summary>
		/// 大型飛行艇。
		/// </summary>
		FlyingBoat = 33,
	}

	public class SlotItemIconTypeName
	{
		public static readonly IReadOnlyDictionary<SlotItemIconType, string> ItemType = new Dictionary<SlotItemIconType, string>
		{
			{SlotItemIconType.Empty, "Empty"},
			{SlotItemIconType.Unknown, "Unknown"},
			{SlotItemIconType.MainCanonLight, "小口径主砲"},
			{SlotItemIconType.MainCanonMedium, "中口径主砲"},
			{SlotItemIconType.MainCanonHeavy, "大口径主砲"},
			{SlotItemIconType.SecondaryCanon, "副砲"},
			{SlotItemIconType.Torpedo, "魚雷"},
			{SlotItemIconType.Fighter, "艦上戦闘機"},
			{SlotItemIconType.DiveBomber, "艦上爆撃機"},
			{SlotItemIconType.TorpedoBomber, "艦上攻撃機"},
			{SlotItemIconType.ReconPlane, "艦上偵察機"},
			{SlotItemIconType.ReconSeaplane, "水上偵察機"},
			{SlotItemIconType.Rader, "電波探信儀"},
			{SlotItemIconType.AAShell, "対空強化弾"},
			{SlotItemIconType.APShell, "徹甲弾"},
			{SlotItemIconType.DamageControl, "ダメコン"},
			{SlotItemIconType.AAGun, "機銃"},
			{SlotItemIconType.HighAngleGun, "高角砲"},
			{SlotItemIconType.ASW, "爆雷投射機"},
			{SlotItemIconType.Soner, "ソナー"},
			{SlotItemIconType.EngineImprovement, "機関部強化"},
			{SlotItemIconType.LandingCraft, "上陸用舟艇"},
			{SlotItemIconType.Autogyro, "オートジャイロ"},
			{SlotItemIconType.ArtillerySpotter, "指揮連絡機"},
			{SlotItemIconType.AntiTorpedoBulge, "増設バルジ"},
			{SlotItemIconType.Searchlight, "探照灯"},
			{SlotItemIconType.DrumCanister, "ドラム缶"},
		};
	}
}
