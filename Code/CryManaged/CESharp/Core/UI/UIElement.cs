// Copyright 2001-2016 Crytek GmbH / Crytek Group. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CryEngine.UI.Components;
using System.Reflection;
using CryEngine.Common;
using System.Runtime.Serialization;

namespace CryEngine.UI
{
	/// <summary>
	/// Rectangle for use in UI System.
	/// </summary>
	public class Rect
	{
		public float x;
		public float y;
		public float w;
		public float h;

		public Rect()
		{
		}

		public Rect(float _x, float _y, float _w, float _h) 
		{
			x = _x;
			y = _y;
			w = _w;
			h = _h;
		}

		/// <summary>
		/// Checks if point is inside this Rect.
		/// </summary>
		/// <param name="v">The point to be checked.</param>
		public bool Contains(Point v)
		{
			return Contains (v.x, v.y);
		}

		/// <summary>
		/// Checks if coordinates are inside this Rect.
		/// </summary>
		/// <param name="_x">The x coordinate.</param>
		/// <param name="_y">The y coordinate.</param>
		public bool Contains(float _x, float _y)
		{
			return _x >= x && _x < x + w && _y >= y && _y < y + h;
		}

		/// <summary>
		/// Returns new rectangle, modified by the delta of 's'.
		/// </summary>
		/// <param name="s">The rect to be used for delta movement.</param>
		public Rect Pad(Rect s)
		{
			return new Rect (x + s.x, y + s.y, w - s.w - s.x, h - s.h - s.y);
		}

		/// <summary>
		/// Returns new rectangle, which includes 'r' and 's'.
		/// </summary>
		/// <param name="r">First Rect to be included.</param>
		/// <param name="s">Second Rect to be included.</param>
		public static Rect operator&(Rect r, Rect s)
		{
			if (s == null)
				return null;
			var x1 = Math.Max (r.x, s.x);
			var y1 = Math.Max (r.y, s.y);
			var x2 = Math.Max(0, Math.Min (r.x + r.w, s.x + s.w));
			var y2 = Math.Max(0, Math.Min (r.y + r.h, s.y + s.h));
			return new Rect (x1, y1, x2 - x1, y2 - y1);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="CryEngine.UI.Rect"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="CryEngine.UI.Rect"/>.</returns>
		public override string ToString()
		{
			return x.ToString("0") + "," + y.ToString("0") + "," + w.ToString("0") + "," + h.ToString("0");
		}
	}

	/// <summary>
	/// Point for use in UI System.
	/// </summary>
	public class Point
	{
		public float x, y;

		public Point()
		{
		}

		public Point(float _x, float _y) 
		{
			x = _x;
			y = _y;
		}

		public static Point operator+(Point u, Point v)
		{
			return new Point (u.x + v.x, u.y + v.y);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="CryEngine.UI.Point"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="CryEngine.UI.Point"/>.</returns>
		public override string ToString()
		{
			return x.ToString("0") + "," + y.ToString("0");
		}
	}

	/// <summary>
	/// Determines a buffer which is applied to make a rect smaller.
	/// </summary>
	[DataContract]
	public class Padding
	{
		[DataMember(Name="L")]
		public float Left; ///< The left buffer.
		[DataMember(Name="T")]
		public float Top; ///< The top buffer.
		[DataMember(Name="R")]
		public float Right; ///< The right buffer.
		[DataMember(Name="B")]
		public float Bottom; ///< The bottom buffer.

		public Point TopLeft { get { return new Point (Left, Top); } set { Left = value.x;Top = value.y; } } ///< The upper left buffer.

		/// <summary>
		/// Constructs Padding with one default value for all buffers.
		/// </summary>
		/// <param name="def">Default buffer value.</param>
		public Padding(float def=0)
		{
			Top = Left = Right = Bottom = def;
		}

		/// <summary>
		/// Only initializes upper left buffer. lower right will be 0.
		/// </summary>
		/// <param name="left">Left buffer value.</param>
		/// <param name="top">Top buffer value.</param>
		public Padding(float left, float top)
		{
			Top = top;Left = left;
		}

		/// <summary>
		/// Initializes Padding with specific values for all buffers.
		/// </summary>
		/// <param name="left">Left buffer value.</param>
		/// <param name="top">Top buffer value.</param>
		/// <param name="right">Right buffer value.</param>
		/// <param name="bottom">Bottom buffer value.</param>
		public Padding(float left, float top, float right, float bottom)
		{
			Top = top;Left = left;Right = right;Bottom = bottom;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="CryEngine.UI.Padding"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="CryEngine.UI.Padding"/>.</returns>
		public override string ToString()
		{
			return Left.ToString("0") + "," + Top.ToString("0") + "," + Right.ToString("0") + "," + Bottom.ToString("0");
		}
	}

	/// <summary>
	/// Alignment for Layout computation of UIElements. UIElements will be centered around an alignment target.
	/// </summary>
	public enum Alignment
	{
		Center,
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		BottomLeft,
		Left,
		TopLeft,
		TopHStretch,
		//CenterHStretch,
		BottomHStretch,
		//LeftVStretch,
		//CenterVStretch,
		RightVStretch,
		Stretch
	}

	/// <summary>
	/// Base class for all UI elements. Provides RectTransform for Layout computation.
	/// </summary>
	[DataContract]
	public class UIElement : SceneObject
	{		
		public RectTransform RectTransform { get; private set; } ///< Defines and computes layout in "d space, for this UIElement.

		/// <summary>
		/// Creates this element along with a RectTransform.
		/// </summary>
		public UIElement()
		{
			RectTransform = AddComponent<RectTransform> ();
		}

		/// <summary>
		/// Returns ths hierarchically predecessing Canvas object for this element.
		/// </summary>
		/// <returns>The parent Canvas.</returns>
		public Canvas FindParentCanvas()
		{
			if (this is Canvas)
				return this as Canvas;
			return Parent is UIElement ? (Parent as UIElement).FindParentCanvas () : null;
		}
	}
}
