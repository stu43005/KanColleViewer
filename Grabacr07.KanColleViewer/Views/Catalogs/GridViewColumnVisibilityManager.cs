﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Grabacr07.KanColleViewer.Views.Catalogs
{
	class GridViewColumnVisibilityManager
	{
		static Dictionary<GridViewColumn, double> originalColumnWidths = new Dictionary<GridViewColumn, double>();

		public static bool GetIsVisible(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsVisibleProperty);
		}

		public static void SetIsVisible(DependencyObject obj, bool value)
		{
			obj.SetValue(IsVisibleProperty, value);
		}

		public static readonly DependencyProperty IsVisibleProperty =
			DependencyProperty.RegisterAttached("IsVisible", typeof(bool), typeof(GridViewColumnVisibilityManager), new UIPropertyMetadata(true, OnIsVisibleChanged));

		private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			GridViewColumn gc = d as GridViewColumn;
			if (gc == null)
				return;

			if (GetIsVisible(gc) == false)
			{
				originalColumnWidths[gc] = gc.Width;
				gc.Width = 0;
			}
			else
			{
				if (gc.Width == 0)
					gc.Width = originalColumnWidths[gc];
			}
		}
	}
}
