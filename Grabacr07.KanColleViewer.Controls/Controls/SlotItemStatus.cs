using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleViewer.Controls
{
	/// <summary>
	/// SlotItem Statusの表示をサポートします。
	/// </summary>
	public class SlotItemStatus : Control
	{
		static SlotItemStatus()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SlotItemStatus), new FrameworkPropertyMetadata(typeof(SlotItemStatus)));
		}


		#region Value 依存関係プロパティ

		public int Value
		{
			get { return (int)this.GetValue(ValueProperty); }
			set { this.SetValue(ValueProperty, value); }
		}
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(int), typeof(SlotItemStatus), new UIPropertyMetadata(0, ValuePropertyChangedCallback));

		private static void ValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var source = (SlotItemStatus)d;
			var value = (int)e.NewValue;

			source.IsNegative = value < 0;
		}

		#endregion

		#region IsNegative 依存関係プロパティ

		public bool IsNegative
		{
			get { return (bool)this.GetValue(IsNegativeProperty); }
			private set { this.SetValue(IsNegativeProperty, value); }
		}
		public static readonly DependencyProperty IsNegativeProperty =
			DependencyProperty.Register("IsNegative", typeof(bool), typeof(SlotItemStatus), new UIPropertyMetadata(false));

		#endregion


	}
}
