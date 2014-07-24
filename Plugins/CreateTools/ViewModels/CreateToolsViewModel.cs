using CreateTools.Models;
using Livet;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace CreateTools.ViewModels
{
	public class CreateToolsViewModel : ViewModel
	{
		public ResViewModel Fuel { get; private set; }
		public ResViewModel Ammunition { get; private set; }
		public ResViewModel Steel { get; private set; }
		public ResViewModel Bauxite { get; private set; }

		public CreateToolsViewModel()
		{
			this.Fuel = new ResViewModel("燃料", 300, 10, 10);
			this.Ammunition = new ResViewModel("弾薬", 300, 10, 10);
			this.Steel = new ResViewModel("鋼材", 300, 10, 10);
			this.Bauxite = new ResViewModel("ボーキサイト", 300, 10, 10);
		}

		public void Start()
		{
			MouseHelper.SaveMousePosition();
			MouseHelper.DoCreate(this.Fuel.Offset, this.Ammunition.Offset, this.Steel.Offset, this.Bauxite.Offset);
			MouseHelper.RestoreMousePosition();
		}
	}
}
