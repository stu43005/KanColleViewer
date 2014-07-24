using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CreateTools.Models
{
	public class MouseHelper
	{
		#region Dll Import

		[DllImport("user32.dll")]
		private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

		[Flags]
		public enum MouseEventFlags
		{
			LeftDown = 0x00000002,
			LeftUp = 0x00000004,
			MiddleDown = 0x00000020,
			MiddleUp = 0x00000040,
			Move = 0x00000001,
			Absolute = 0x00008000,
			RightDown = 0x00000008,
			RightUp = 0x00000010
		}

		#endregion

		public static int MainWindowTop
		{
			get { return (int)System.Windows.Application.Current.MainWindow.Top; }
		}

		public static int MainWindowLeft
		{
			get { return (int)System.Windows.Application.Current.MainWindow.Left; }
		}

		public static void MouseMoveTo(int x, int y)
		{
			Cursor.Position = new Point(x, y);
		}

		public static void MouseMoveTo(Point pt, bool offset = true)
		{
			if (offset)
				MouseMoveTo(MainWindowLeft + pt.X, MainWindowTop + pt.Y);
			else
				MouseMoveTo(pt.X, pt.Y);
		}

		public static void MouseClick()
		{
			mouse_event((int)(MouseEventFlags.LeftDown | MouseEventFlags.LeftUp), Cursor.Position.X, Cursor.Position.Y, 0, 0);
		}

		private static Point SavedCursorPosition;

		public static void SaveMousePosition()
		{
			SavedCursorPosition = new Point(Cursor.Position.X, Cursor.Position.Y);
		}

		public static void RestoreMousePosition()
		{
			MouseMoveTo(SavedCursorPosition, false);
		}

		private static Point FuelAdd1Position = new Point(361, 187);
		private static Point FuelAdd10Position = new Point(489, 173);
		private static Point FuelAdd100Position = new Point(489, 199);

		private static Point AmmunitionAdd1Position = new Point(361, 317);
		private static Point AmmunitionAdd10Position = new Point(489, 302);
		private static Point AmmunitionAdd100Position = new Point(489, 329);

		private static Point SteelAdd1Position = new Point(589, 187);
		private static Point SteelAdd10Position = new Point(717, 173);
		private static Point SteelAdd100Position = new Point(717, 199);

		private static Point BauxiteAdd1Position = new Point(589, 317);
		private static Point BauxiteAdd10Position = new Point(717, 302);
		private static Point BauxiteAdd100Position = new Point(717, 329);

		private static Point StartCreatePosition = new Point(706, 478);

		public static void DoCreate(int fuel, int ammunition, int steel, int bauxite)
		{
			// 燃料
			int fuel1 = fuel % 10;
			int fuel10 = fuel / 10 % 10;
			int fuel100 = fuel / 100 % 10;
			for (int i = 0; i < fuel1; i++)
			{
				MouseMoveTo(FuelAdd1Position);
				MouseClick();
			}
			for (int i = 0; i < fuel10; i++)
			{
				MouseMoveTo(FuelAdd10Position);
				MouseClick();
			}
			for (int i = 0; i < fuel100; i++)
			{
				MouseMoveTo(FuelAdd100Position);
				MouseClick();
			}

			// 彈藥
			int ammunition1 = ammunition % 10;
			int ammunition10 = ammunition / 10 % 10;
			int ammunition100 = ammunition / 100 % 10;
			for (int i = 0; i < ammunition1; i++)
			{
				MouseMoveTo(AmmunitionAdd1Position);
				MouseClick();
			}
			for (int i = 0; i < ammunition10; i++)
			{
				MouseMoveTo(AmmunitionAdd10Position);
				MouseClick();
			}
			for (int i = 0; i < ammunition100; i++)
			{
				MouseMoveTo(AmmunitionAdd100Position);
				MouseClick();
			}

			// 鋼材
			int steel1 = steel % 10;
			int steel10 = steel / 10 % 10;
			int steel100 = steel / 100 % 10;
			for (int i = 0; i < steel1; i++)
			{
				MouseMoveTo(SteelAdd1Position);
				MouseClick();
			}
			for (int i = 0; i < steel10; i++)
			{
				MouseMoveTo(SteelAdd10Position);
				MouseClick();
			}
			for (int i = 0; i < steel100; i++)
			{
				MouseMoveTo(SteelAdd100Position);
				MouseClick();
			}

			// 鋁土
			int bauxite1 = bauxite % 10;
			int bauxite10 = bauxite / 10 % 10;
			int bauxite100 = bauxite / 100 % 10;
			for (int i = 0; i < bauxite1; i++)
			{
				MouseMoveTo(BauxiteAdd1Position);
				MouseClick();
			}
			for (int i = 0; i < bauxite10; i++)
			{
				MouseMoveTo(BauxiteAdd10Position);
				MouseClick();
			}
			for (int i = 0; i < bauxite100; i++)
			{
				MouseMoveTo(BauxiteAdd100Position);
				MouseClick();
			}

			// 開發開始
			MouseMoveTo(StartCreatePosition);
			MouseClick();
		}
	}
}
