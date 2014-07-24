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
