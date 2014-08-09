using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using CreateTools.ViewModels;
using CreateTools.Views;

namespace CreateTools
{
	[Export(typeof(IToolPlugin))]
	[ExportMetadata("Title", "CreateTools")]
	[ExportMetadata("Description", "開發補助工具")]
	[ExportMetadata("Version", "1.0")]
	[ExportMetadata("Author", "@stu43005")]
	public class CreateTools : IToolPlugin
    {
		public string ToolName
		{
			get { return "CreateTools"; }
		}

		public object GetSettingsView()
		{
			return null;
		}

		public object GetToolView()
		{
			var vm = new CreateToolsViewModel();
			return new CreateToolsView { DataContext = vm };
		}
	}
}
