/*
Copyright (C) 2018  Prism Framework Team

This file is part of the Prism Framework.

The Prism Framework is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

The Prism Framework is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/


using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Prism.Native;
using Prism.UI.Media.Inking;

namespace Prism.Android.UI.Media.Inking
{
    /// <summary>
    /// Represents an Android implementation for an <see cref="INativeInkStroke"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeInkStroke))]
    public class InkStroke : Path, INativeInkStroke
    {
        /// <summary>
        /// Gets a rectangle that encompasses all of the points in the ink stroke.
        /// </summary>
        public Rectangle BoundingBox
        {
            get
            {
                var bounds = new RectF();
                ComputeBounds(bounds, true);
                return bounds.GetRectangle();
            }
        }

        /// <summary>
        /// Gets a collection of points that make up the ink stroke.
        /// </summary>
        public IEnumerable<Point> Points
        {
            get { return points; }
        }
        private List<Point> points = new List<Point>();
        
        internal bool NeedsDrawing = true;
        internal Paint Paint;
        internal View Parent;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="InkStroke"/> class.
        /// </summary>
        public InkStroke()
        {
            Paint = new Paint()
            {
                AntiAlias = true,
                Color = Color.Black,
                Dither = true,
                StrokeCap = Paint.Cap.Round,
                StrokeJoin = Paint.Join.Round
            };
            
            Paint.SetStyle(Paint.Style.Stroke);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InkStroke"/> class.
        /// </summary>
        /// <param name="points">A collection of <see cref="Point"/> objects that defines the shape of the stroke.</param>
        public InkStroke(IEnumerable<Point> points)
            : this()
        {
            var array = points.ToArray();
            if (array.Length > 0)
            {
                base.MoveTo(array[0].X.GetScaledFloat(), array[0].Y.GetScaledFloat());
                this.points.Add(array[0]);
                
                for (int i = 1; i < array.Length - 2;)
                {
                    var point1 = array[i++];
                    var point2 = array[i++];
                    var point3 = array[i++];
                    
                    this.points.Add(point1);
                    this.points.Add(point2);
                    this.points.Add(point3);
                    
                    base.CubicTo(point1.X.GetScaledFloat(), point1.Y.GetScaledFloat(),
                        point2.X.GetScaledFloat(), point2.Y.GetScaledFloat(),
                        point3.X.GetScaledFloat(), point3.Y.GetScaledFloat());
                        
                    base.MoveTo(point3.X.GetScaledFloat(), point3.Y.GetScaledFloat());
                }
            }
        }

        /// <summary>
        /// Returns a deep-copy clone of this instance.
        /// </summary>
        public new INativeInkStroke Clone()
        {
            var clone = new InkStroke(points);
            clone.Paint.Color = Paint.Color;
            clone.Paint.StrokeCap = Paint.StrokeCap;
            clone.Paint.StrokeWidth = Paint.StrokeWidth;
            return clone;
        }

        /// <summary>
        /// Returns a copy of the ink stroke's drawing attributes.
        /// </summary>
        public InkDrawingAttributes CopyDrawingAttributes()
        {
            return new InkDrawingAttributes
            {
                Color = Paint.Color.GetColor(),
                Size = Paint.StrokeWidth.GetScaledDouble(),
                PenTip = Paint.StrokeCap == Paint.Cap.Round ? PenTipShape.Circle : PenTipShape.Square
            };
        }
        
        /// <summary>Add a cubic bezier from the last point, approaching control points (x1,y1) and (x2,y2), and ending at (x3,y3).</summary>
        /// <param name="x1">The x-coordinate of the 1st control point on a cubic curve.</param>
        /// <param name="y1">The y-coordinate of the 1st control point on a cubic curve.</param>
        /// <param name="x2">The x-coordinate of the 2nd control point on a cubic curve.</param>
        /// <param name="y2">The y-coordinate of the 2nd control point on a cubic curve.</param>
        /// <param name="x3">The x-coordinate of the end point on a cubic curve.</param>
        /// <param name="y3">The y-coordinate of the end point on a cubic curve.</param>
        public override void CubicTo(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            base.CubicTo(x1, y1, x2, y2, x3, y3);
            points.Add(new Point(x1.GetScaledDouble(), y1.GetScaledDouble()));
            points.Add(new Point(x2.GetScaledDouble(), y2.GetScaledDouble()));
            points.Add(new Point(x3.GetScaledDouble(), y3.GetScaledDouble()));
        }
        
        /// <summary>
        /// Set the beginning of the next contour to the point (x,y).
        /// </summary>
        /// <param name="x">The x-coordinate of the start of a new contour.</param>
        /// <param name="y">The y-coordinate of the start of a new contour.</param>
        public override void MoveTo(float x, float y)
        {
            base.MoveTo(x, y);
            
            var point = new Point(x.GetScaledDouble(), y.GetScaledDouble());
            if (points.Count == 0 || points.Last() != point)
            {
                points.Add(point);
            }
        }

        /// <summary>
        /// Updates the drawing attributes of the ink stroke.
        /// </summary>
        /// <param name="attributes">The drawing attributes to apply to the ink stroke.</param>
        public void UpdateDrawingAttributes(InkDrawingAttributes attributes)
        {
            Paint.Color = attributes.Color.GetColor();
            Paint.StrokeWidth = attributes.Size.GetScaledFloat();
            Paint.StrokeCap = attributes.PenTip == PenTipShape.Square ? Paint.Cap.Butt : Paint.Cap.Round;
            
            NeedsDrawing = true;
            Parent?.Invalidate();
        }
    }
}
