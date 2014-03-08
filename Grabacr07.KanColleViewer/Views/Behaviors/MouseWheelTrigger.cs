using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Grabacr07.KanColleViewer.Views.Behaviors
{
	class MouseWheelTrigger : TriggerBase<UIElement>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			this.AssociatedObject.MouseWheel += OnMouseWheel;
		}

		protected override void OnDetaching()
		{
			this.AssociatedObject.MouseWheel -= OnMouseWheel;
			base.OnDetaching();
		}

		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			this.InvokeActions(e.Delta);
		}
	}
}
