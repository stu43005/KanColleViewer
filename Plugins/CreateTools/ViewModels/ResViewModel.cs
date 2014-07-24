using Livet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateTools.ViewModels
{
	public class ResViewModel : ViewModel
	{
		public string Name { get; private set; }

		#region Value 變更通知屬性

		private int _Value;

		public int Value
		{
			get { return this._Value; }
			set
			{
				if (this._Value != value)
				{
					if (value < this.Min)
						this._Value = this.Min;
					else if (value > this.Max)
						this._Value = this.Max;
					else
						this._Value = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public int Offset
		{
			get { return this.Value - this.Default; }
		}

		private int Max { get; set; }
		private int Min { get; set; }
		private int Default { get; set; }

		public ResViewModel(string name, int max, int min, int def)
		{
			this.Name = name;
			this.Max = max;
			this.Min = min;
			this.Default = def;
			this.Value = def;
		}

		public void Add1()
		{
			Value += 1;
		}

		public void Sub1()
		{
			Value -= 1;
		}

		public void Add10()
		{
			Value += 10;
		}

		public void Sub10()
		{
			Value -= 10;
		}

		public void Add100()
		{
			Value += 100;
		}

		public void Sub100()
		{
			Value -= 100;
		}
	}
}
