using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
	[Export(typeof(IToolPlugin))]
	[ExportMetadata("Title", "Logger")]
	[ExportMetadata("Description", "後端紀錄檔案")]
	[ExportMetadata("Version", "1.0")]
	[ExportMetadata("Author", "@stu43005")]
	public class Logger : IToolPlugin
	{
		private readonly LoggerViewModel viewModel = new LoggerViewModel
		{
			Loggers = new ObservableCollection<LoggerBase>
			{
				new CreateItemLogger(KanColleClient.Current.Proxy),
				new CreateShipLogger(KanColleClient.Current.Proxy),
				new BattleLogger(KanColleClient.Current.Proxy),
				new MissionLogger(KanColleClient.Current.Proxy),
				new MaterialsLogger(KanColleClient.Current.Proxy),
			}
		};

		public Logger()
		{
			LoggerSettings.Load();
		}

		public string ToolName
		{
			get { return "Logger"; }
		}

		public object GetSettingsView()
		{
			return null;
		}

		public object GetToolView()
		{
			return new LoggerView { DataContext = this.viewModel, };
		}
	}
}
