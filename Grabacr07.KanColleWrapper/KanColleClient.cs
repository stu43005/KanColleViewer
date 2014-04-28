﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;

namespace Grabacr07.KanColleWrapper
{
	public class KanColleClient : NotificationObject
	{
		#region singleton

		private static readonly KanColleClient current = new KanColleClient();

		public static KanColleClient Current
		{
			get { return current; }
		}

		#endregion


		/// <summary>
		/// 艦これの通信をフックするプロキシを取得します。
		/// </summary>
		public KanColleProxy Proxy { get; private set; }

		/// <summary>
		/// ユーザーに依存しないマスター情報を取得します。
		/// </summary>
		public Master Master { get; private set; }

		/// <summary>
		/// 母港の情報を取得します。
		/// </summary>
		public Homeport Homeport { get; private set; }


		#region IsStarted 変更通知プロパティ

		private bool _IsStarted;

		/// <summary>
		/// 艦これが開始されているかどうかを示す値を取得します。
		/// </summary>
		public bool IsStarted
		{
			get { return this._IsStarted; }
			set
			{
				if (this._IsStarted != value)
				{
					this._IsStarted = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region EnableLogging

		private bool _EnableLogging;

		public bool EnableLogging
		{
			get { return this._EnableLogging; }
			set
			{
				if (this._EnableLogging != value)
				{
					this._EnableLogging = value;
					if (this.Homeport != null) this.Homeport.Logger.EnableLogging = this._EnableLogging;
				}
			}
		}

		#endregion


		private KanColleClient()
		{
			var proxy = new KanColleProxy();
			var basic = proxy.api_get_member_basic.TryParse<kcsapi_basic>().FirstAsync().ToTask();
			var kdock = proxy.api_get_member_kdock.TryParse<kcsapi_kdock[]>().FirstAsync().ToTask();
			var sitem = proxy.api_get_member_slot_item.TryParse<kcsapi_slotitem[]>().FirstAsync().ToTask();
			var timeout = Task.Delay(TimeSpan.FromSeconds(20));

			proxy.api_start2.FirstAsync().Subscribe(async session =>
			{
				var canInitialize = await Task.WhenAny(new Task[] { basic, kdock, sitem }.WhenAll(), timeout) != timeout;

				// タイムアウト仕掛けてるのは、今後のアップデートで basic, kdock, slot_item のいずれかが来なくなったときに
				// 起動できなくなる (IsStarted を true にできなくなる) のを防ぐため
				// -----
				// ま、そんな規模の変更があったらそもそもまともに動作せんだろうがな ☝(◞‸◟)☝ 野良ツールはつらいよ

				SvData<kcsapi_start2> svd;
				if (!SvData.TryParse(session, out svd)) return;

				this.Master = new Master(svd.Data);
				this.Homeport = new Homeport(proxy);

				if (canInitialize)
				{
					this.Homeport.UpdateAdmiral((await basic).Data);
					this.Homeport.UpdateSlotItems((await sitem).Data);
					this.Homeport.Dockyard.Update((await kdock).Data);
				}

				this.IsStarted = true;
			});

			this.Proxy = proxy;
		}
	}
}
