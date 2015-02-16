using Grabacr07.KanColleViewer.Models.Data.Xml;
using Livet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
	[Serializable]
	public class LoggerSettings : NotificationObject
	{
		#region static members

		private static readonly string filePath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"grabacr.net",
			"KanColleViewer",
			"LoggerSettings.xml");

		public static LoggerSettings Current { get; set; }

		public static void Load()
		{
			try
			{
				Current = filePath.ReadXml<LoggerSettings>();
			}
			catch (Exception ex)
			{
				Current = GetInitialSettings();
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		public static LoggerSettings GetInitialSettings()
		{
			return new LoggerSettings
			{
				EnableCreateItemLogging = true,
				EnableCreateShipLogging = true,
				EnableBattleLogging = true,
				EnableMissionLogging = true,
			};
		}

		#endregion


		#region EnableCreateItemLogging 変更通知プロパティ

		private bool _EnableCreateItemLogging;

		public bool EnableCreateItemLogging
		{
			get { return this._EnableCreateItemLogging; }
			set
			{
				if (this._EnableCreateItemLogging != value)
				{
					this._EnableCreateItemLogging = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region EnableCreateItemLogging 変更通知プロパティ

		private bool _EnableCreateShipLogging;

		public bool EnableCreateShipLogging
		{
			get { return this._EnableCreateShipLogging; }
			set
			{
				if (this._EnableCreateShipLogging != value)
				{
					this._EnableCreateShipLogging = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region EnableBattleLogging 変更通知プロパティ

		private bool _EnableBattleLogging;

		public bool EnableBattleLogging
		{
			get { return this._EnableBattleLogging; }
			set
			{
				if (this._EnableBattleLogging != value)
				{
					this._EnableBattleLogging = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region EnableMissionLogging 変更通知プロパティ

		private bool _EnableMissionLogging;

		public bool EnableMissionLogging
		{
			get { return this._EnableMissionLogging; }
			set
			{
				if (this._EnableMissionLogging != value)
				{
					this._EnableMissionLogging = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion


		public void Save()
		{
			try
			{
				this.WriteXml(filePath);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}
	}
}
