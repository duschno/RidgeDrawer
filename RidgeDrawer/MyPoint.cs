using System;
using System.Drawing;
using System.Diagnostics;

namespace RidgeDrawer
{
	[DebuggerDisplay("X = {X} Y = {Y}")]
	public struct MyPoint
	{
		private Point point;
		private MyPoint(Point p)
		{
			point = p;
		}
		public MyPoint(Size sz) : this(new Point(sz))
		{ }
		public MyPoint(int dw) : this(new Point(dw))
		{ }
		public MyPoint(int x, int y) : this(new Point(x, y))
		{ }

		public int X
		{
			get
			{
				return point.X;
			}

			set
			{
				point.X = value;
			}
		}
		public int Y
		{
			get
			{
				return point.Y;
			}

			set
			{
				point.Y = value;
			}
		}

		public static Point[] ToPoint(MyPoint[] coords)
		{
			return Array.ConvertAll(coords, item => (Point)item);
		}

		public static implicit operator PointF(MyPoint p)
		{
			return p.point;
		}

		public static implicit operator Point(MyPoint p)
		{
			return p.point;
		}

		public static implicit operator MyPoint(Point p)
		{
			return new MyPoint(p);
		}

		public static bool operator ==(MyPoint p1, MyPoint p2)
		{
			return p1.Equals(p2);
		}

		public static bool operator !=(MyPoint p1, MyPoint p2)
		{
			return !p1.Equals(p2);
		}
	}
}
