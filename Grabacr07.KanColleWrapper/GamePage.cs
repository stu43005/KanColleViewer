using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Internal;
using Livet;

namespace Grabacr07.KanColleWrapper
{
	public class GamePage : NotificationObject
	{
		private static int recordCount = 6;
		private List<string> requestSequence = new List<string>(recordCount);

		#region PageName 変更通知プロパティ

		private string _PageName;

		public string PageName
		{
			get { return this._PageName; }
			set
			{
				if (this._PageName != value)
				{
					this._PageName = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion
		
		#region preventKousyouAction 変更通知プロパティ

		private bool _preventKousyouAction;

		public bool preventKousyouAction
		{
			get { return this._preventKousyouAction; }
			private set
			{
				if (this._preventKousyouAction != value)
				{
					this._preventKousyouAction = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion


		internal GamePage(KanColleProxy proxy)
		{
			proxy.ApiSessionSource.Subscribe(x =>
			{
				this.requestSequence.Insert(0, x.PathAndQuery);
				if (this.requestSequence.Count > recordCount)
					this.requestSequence.RemoveAt(recordCount);
			});

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/useitem")
				.Subscribe(x =>
				{
					int index = 0;
					if (this.requestSequence[index] == "/kcsapi/api_get_member/useitem")
						index++;
					if (this.requestSequence[index++] == "/kcsapi/api_get_member/ship2" &&
						this.requestSequence[index++] == "/kcsapi/api_get_member/ndock")
					{
						// ただし、高速修復材を使用したときも同じシーケンスを見せる
						// したがって、[3],[4],[5]が高速修復材を使用したときの振る舞いをするなら
						// orで判定し厳しめにこれを除外する
						if (this.requestSequence[index++] == "/kcsapi/api_get_member/ndock" ||
							this.requestSequence[index++] == "/kcsapi/api_get_member/material" ||
							this.requestSequence[index++] == "/kcsapi/api_get_member/ship2")
						{
							return;
						}
						this.PageName = "nyukyo";
					}
				});
			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/practice")
				.Subscribe(x => this.PageName = "practice");
			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_master/mapinfo")
				.Subscribe(x => this.PageName = "map");
			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_master/mission")
				.Subscribe(x => this.PageName = "mission");

			// 工廠画面への遷移が「record」単体なのに対して
			// 任務画面への遷移は「record→questlist」である。
			// 同様に、様々なケースでapi_get_member/recordが呼ばれる
			// ということで、直後に生成される(かもしれない)
			// 工廠画面遷移以外のフラグを見て
			// 判定する必要がある(´；ω；｀)ﾌﾞﾜｯ
			/*proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/record")
				//.Subscribe(x => this.PageName = "kousyou");
				.Do(_ => this.preventKousyouAction = false)
				.Throttle(TimeSpan.FromMilliseconds(300))
				.Subscribe(x =>
				{
					if (this.preventKousyouAction)
					{
						this.preventKousyouAction = false;
					}
					else
					{
						this.PageName = "kousyou";
					}
				});

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/questlist")
				.Subscribe(x => this.preventKousyouAction = true);
			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_master/mapinfo")
				.Subscribe(x => this.preventKousyouAction = true);*/
		}
	}
}
