using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interactivity;
using Grabacr07.KanColleViewer.ViewModels.Contents.Fleets;

namespace Grabacr07.KanColleViewer.Views.Behaviors
{
	class MouseWheelFleetsAction : TargetedTriggerAction<FleetsViewModel>
	{
		public bool IsCyclic { get; set; }

		protected override void Invoke(object parameter)
		{
			if (!(parameter is int))
				return;

			var Delta = (int)parameter;
			if (Delta < 0)
			{
				Next();
			}
			else
			{
				Previous();
			}
		}

		private void Next()
		{
			if (this.Target == null)
				return;

			var selectedIndex = Array.IndexOf(this.Target.Fleets, this.Target.SelectedFleet);
			if (selectedIndex < this.Target.Fleets.Length - 1)
				this.Target.SelectedFleet = this.Target.Fleets[selectedIndex + 1];
			else if (this.IsCyclic)
				this.Target.SelectedFleet = this.Target.Fleets.First();
		}

		private void Previous()
		{
			if (this.Target == null)
				return;

			var selectedIndex = Array.IndexOf(this.Target.Fleets, this.Target.SelectedFleet);
			if (selectedIndex > 0)
				this.Target.SelectedFleet = this.Target.Fleets[selectedIndex - 1];
			else if (this.IsCyclic)
				this.Target.SelectedFleet = this.Target.Fleets.Last();
		}
	}
}
