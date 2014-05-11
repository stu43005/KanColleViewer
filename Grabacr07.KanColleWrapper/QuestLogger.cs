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
