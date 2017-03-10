/*
Copyright (C) 2017  Prism Framework Team

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


using System;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Media;

using APath = Android.Graphics.Path;

namespace Prism.Android.UI.Shapes
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativePath"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativePath))]
    public class Path : global::Android.Views.View, INativePath
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;
        
        /// <summary>
        /// Occurs when the system loses track of the pointer for some reason.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerCanceled;
        
        /// <summary>
        /// Occurs when the pointer has moved while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerMoved;

        /// <summary>
        /// Occurs when the pointer has been pressed while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerPressed;

        /// <summary>
        /// Occurs when the pointer has been released while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerReleased;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Gets or sets a value indicating whether animations are enabled for this instance.
        /// </summary>
        public bool AreAnimationsEnabled
        {
            get { return areAnimationsEnabled; }
            set
            {
                if (value != areAnimationsEnabled)
                {
                    areAnimationsEnabled = value;
                    OnPropertyChanged(Visual.AreAnimationsEnabledProperty);
                }
            }
        }
        private bool areAnimationsEnabled;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests an arrangement of its children.
        /// </summary>
        public ArrangeRequestHandler ArrangeRequest { get; set; }

        /// <summary>
        /// Gets or sets the background for the control.
        /// </summary>
        public Brush Fill
        {
            get { return fill; }
            set
            {
                if (value != fill)
                {
                    (fill as ImageBrush).ClearImageHandler(OnImageLoaded);
                    
                    fill = value;
                    FillPaint.SetBrush(fill, Width, Height, OnImageLoaded);
                    OnPropertyChanged(Prism.UI.Shapes.Shape.FillProperty);
                    Invalidate();
                }
            }
        }
        private Brush fill;
        
        /// <summary>
        /// Gets or sets the rule to use for determining the interior fill of the shape.
        /// </summary>
        public FillRule FillRule
        {
            get
            {
                var fillType = path.GetFillType();
                return fillType == APath.FillType.EvenOdd || fillType == APath.FillType.InverseEvenOdd ? FillRule.EvenOdd : FillRule.Nonzero;
            }
            set
            {
                var fillType = value == FillRule.EvenOdd ? APath.FillType.EvenOdd : APath.FillType.Winding;
                if (fillType != path.GetFillType())
                {
                    path.SetFillType(fillType);
                    OnPropertyChanged(Prism.UI.Shapes.Path.FillRuleProperty);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get
            {
                return new Rectangle(Left / Device.Current.DisplayScale, Top / Device.Current.DisplayScale,
                    Width / Device.Current.DisplayScale, Height / Device.Current.DisplayScale);
            }
            set
            {
                Left = (int)(value.Left * Device.Current.DisplayScale);
                Top = (int)(value.Top * Device.Current.DisplayScale);
                Right = (int)(value.Right * Device.Current.DisplayScale);
                Bottom = (int)(value.Bottom * Device.Current.DisplayScale);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return isHitTestVisible; }
            set
            {
                if (value != isHitTestVisible)
                {
                    isHitTestVisible = value;
                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
                }
            }
        }
        private bool isHitTestVisible = true;

        /// <summary>
        /// Gets a value indicating whether this instance has been loaded and is ready for rendering.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the level of opacity for the element.
        /// </summary>
        public double Opacity
        {
            get { return Alpha; }
            set
            {
                if (value != Alpha)
                {
                    Alpha = (float)value;
                    OnPropertyChanged(Element.OpacityProperty);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the method to invoke when this instance requests information for the path being drawn.
        /// </summary>
        public PathInfoRequestHandler PathInfoRequest { get; set; }
        
        /// <summary>
        /// Gets or sets transformation information that affects the rendering position of this instance.
        /// </summary>
        public INativeTransform RenderTransform
        {
            get { return renderTransform; }
            set
            {
                if (value != renderTransform)
                {
                    (renderTransform as Media.Transform)?.RemoveView(this);
                    renderTransform = value;
                    
                    var transform = renderTransform as Media.Transform;
                    if (transform == null)
                    {
                        Animation = renderTransform as global::Android.Views.Animations.Animation;
                    }
                    else
                    {
                        transform.AddView(this);
                    }

                    OnPropertyChanged(Visual.RenderTransformProperty);
                }
            }
        }
        private INativeTransform renderTransform;

        /// <summary>
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Theme RequestedTheme { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the outline of the shape.
        /// </summary>
        public Brush Stroke
        {
            get { return stroke; }
            set
            {
                if (value != stroke)
                {
                    (stroke as ImageBrush).ClearImageHandler(OnImageLoaded);
                    
                    stroke = value;
                    StrokePaint.SetBrush(stroke, Width, Height, OnImageLoaded);
                    OnPropertyChanged(Prism.UI.Shapes.Shape.StrokeProperty);
                    Invalidate();
                }
            }
        }
        private Brush stroke;

        /// <summary>
        /// Gets or sets the manner in which the ends of a line are drawn.
        /// </summary>
        public LineCap StrokeLineCap
        {
            get { return StrokePaint.StrokeCap.GetLineCap(); }
            set
            {
                var cap = value.GetPaintCap();
                if (cap != StrokePaint.StrokeCap)
                {
                    StrokePaint.StrokeCap = cap;

                    OnPropertyChanged(Prism.UI.Shapes.Shape.StrokeLineCapProperty);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the manner in which the connections between different lines are drawn.
        /// </summary>
        public LineJoin StrokeLineJoin
        {
            get { return StrokePaint.StrokeJoin.GetLineJoin(); }
            set
            {
                var join = value.GetPaintJoin();
                if (join != StrokePaint.StrokeJoin)
                {
                    StrokePaint.StrokeJoin = join;

                    OnPropertyChanged(Prism.UI.Shapes.Shape.StrokeLineJoinProperty);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the miter limit for connecting lines.
        /// </summary>
        public double StrokeMiterLimit
        {
            get { return StrokePaint.StrokeMiter; }
            set
            {
                if (value != StrokePaint.StrokeMiter)
                {
                    StrokePaint.StrokeMiter = (float)value;

                    OnPropertyChanged(Prism.UI.Shapes.Shape.StrokeMiterLimitProperty);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the shape's outline.
        /// </summary>
        public double StrokeThickness
        {
            get { return StrokePaint.StrokeWidth.GetScaledDouble(); }
            set
            {
                float thickness = value.GetScaledFloat();
                if (thickness != StrokePaint.StrokeWidth)
                {
                    StrokePaint.StrokeWidth = thickness;

                    OnPropertyChanged(Prism.UI.Shapes.Shape.StrokeThicknessProperty);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the display state of the element.
        /// </summary>
        public new Visibility Visibility
        {
            get { return base.Visibility.GetVisibility(); }
            set
            {
                var visibility = value.GetViewStates();
                if (visibility != base.Visibility)
                {
                    base.Visibility = visibility;
                    OnPropertyChanged(Element.VisibilityProperty);
                }
            }
        }
        
        /// <summary>
        /// Gets the <see cref="Paint"/> object used to render the shape interior.
        /// </summary>
        protected Paint FillPaint { get; } = new Paint();

        /// <summary>
        /// Gets the <see cref="Paint"/> object used to render the shape outline.
        /// </summary>
        protected Paint StrokePaint { get; } = new Paint();
        
        private readonly APath path = new APath();

        /// <summary>
        /// Initializes a new instance of the <see cref="Ellipse"/> class.
        /// </summary>
        public Path()
            : base(Application.MainActivity)
        {
            SetWillNotDraw(false);
            
            FillPaint.AntiAlias = true;
            FillPaint.SetStyle(Paint.Style.Fill);
            
            StrokePaint.AntiAlias = true;
            StrokePaint.SetStyle(Paint.Style.Stroke);
        }

        /// <summary></summary>
        /// <param name="e"></param>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            var parent = Parent as ITouchDispatcher;
            if (parent == null || parent.IsDispatching)
            {
                return base.DispatchTouchEvent(e);
            }

            return false;
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            RequestLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            RequestLayout();
        }
        
        /// <summary>
        /// Signals that the path needs to be rebuilt before it is drawn again.
        /// </summary>
        public void InvalidatePathInfo()
        {
            path.Reset();
            Invalidate();
        }

        /// <summary>
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            return constraints;
        }
        
        /// <summary></summary>
        /// <param name="e"></param>
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!isHitTestVisible)
            {
                return false;
            }
            
            if (e.Action == MotionEventActions.Cancel)
            {
                PointerCanceled(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Down)
            {
                PointerPressed(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Move)
            {
                PointerMoved(this, e.GetPointerEventArgs(this));
            }
            if (e.Action == MotionEventActions.Up)
            {
                PointerReleased(this, e.GetPointerEventArgs(this));
            }
            return base.OnTouchEvent(e);
        }
        
        /// <summary>
        /// Sets the dash pattern to be used when drawing the outline of the shape.
        /// </summary>
        /// <param name="pattern">An array of values that defines the dash pattern.  Each value represents the length of a dash, alternating between "on" and "off".</param>
        /// <param name="offset">The distance within the dash pattern where dashes begin.</param>
        public void SetStrokeDashPattern(double[] pattern, double offset)
        {
            if (pattern == null)
            {
                StrokePaint.SetPathEffect(null);
            }
            else
            {
                var array = new float[pattern.Length];
                for (int i = 0; i < pattern.Length; i++)
                {
                    array[i] = pattern[i].GetScaledFloat();
                }
                
                StrokePaint.SetPathEffect(new DashPathEffect(array, offset.GetScaledFloat()));
            }
            
            Invalidate();
        }

        /// <summary>
        /// This is called when the view is attached to a window.
        /// </summary>
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            OnLoaded();
        }

        /// <summary>
        /// This is called when the view is detached from a window.
        /// </summary>
        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            OnUnloaded();
        }

        /// <summary>
        /// Implement this to do your drawing.
        /// </summary>
        /// <param name="canvas"></param>
        protected override void OnDraw(global::Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);
            
            if (path.IsEmpty)
            {
                BuildPath();
            }
            
            if (fill != null)
            {
                canvas.DrawPath(path, FillPaint);
            }
            
            canvas.DrawPath(path, StrokePaint);
        }

        /// <summary>
        /// Called from layout when this view should assign a size and position to each of its children.
        /// </summary>
        /// <param name="changed"></param>
        /// <param name="left">Left position, relative to parent.</param>
        /// <param name="top">Top position, relative to parent.</param>
        /// <param name="right">Right position, relative to parent.</param>
        /// <param name="bottom">Bottom position, relative to parent.</param>
        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            ArrangeRequest(false, null);
        }

        /// <summary>
        /// Measure the view and its content to determine the measured width and the measured height.
        /// </summary>
        /// <param name="widthMeasureSpec">Horizontal space requirements as imposed by the parent.</param>
        /// <param name="heightMeasureSpec">Vertical space requirements as imposed by the parent.</param>
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            MeasureRequest(false, null);
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }
        
        /// <summary>
        /// This is called during layout when the size of this view has changed.
        /// </summary>
        /// <param name="w">Current width of this view.</param>
        /// <param name="h">Current height of this view.</param>
        /// <param name="oldw">Old width of this view.</param>
        /// <param name="oldh">Old height of this view.</param>
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            StrokePaint.SetBrush(stroke, w, h, null);
        }
        
        private void BuildPath()
        {
            var pathInfos = PathInfoRequest();

            for (int i = 0; i < pathInfos.Count; i++)
            {
                var figure = pathInfos[i];
                var subpath = new APath();

                subpath.MoveTo(figure.StartPoint.X.GetScaledFloat(), figure.StartPoint.Y.GetScaledFloat());

                for (int j = 0; j < figure.Segments.Count; j++)
                {
                    var segment = figure.Segments[j];

                    var line = segment as LineSegment;
                    if (line != null)
                    {
                        subpath.LineTo(line.EndPoint.X.GetScaledFloat(), line.EndPoint.Y.GetScaledFloat());
                        continue;
                    }
                    
                    var arc = segment as ArcSegment;
                    if (arc != null)
                    {
                        var startPoint = j == 0 ? figure.StartPoint : figure.Segments[j - 1].EndPoint;
                        var endPoint = arc.EndPoint;
                        var trueSize = arc.Size;
                        
                        if (trueSize.Width == 0 || trueSize.Height == 0)
                        {
                            subpath.LineTo(endPoint.X.GetScaledFloat(), endPoint.Y.GetScaledFloat());
                            continue;
                        }
            
                        double rise = Math.Round(Math.Abs(endPoint.Y - startPoint.Y), 1);
                        double run = Math.Round(Math.Abs(endPoint.X - startPoint.X), 1);
                        if (rise == 0 && run == 0)
                        {
                            continue;
                        }

                        Point center = new Point(double.NaN, double.NaN);
                        
                        double scale = Math.Max(run / (trueSize.Width * 2), rise / (trueSize.Height * 2));
                        if (scale > 1)
                        {
                            center.X = (startPoint.X + endPoint.X) / 2;
                            center.Y = (startPoint.Y + endPoint.Y) / 2;
            
                            double diffX = run / 2;
                            double diffY = rise / 2;
            
                            var angle = Math.Atan2(diffY / trueSize.Height, diffX / trueSize.Width);
                            var cos = Math.Cos(angle) * trueSize.Width;
                            var sin = Math.Sin(angle) * trueSize.Height;
            
                            scale = Math.Sqrt(diffX * diffX + diffY * diffY) / Math.Sqrt(cos * cos + sin * sin);
                            trueSize.Width *= scale;
                            trueSize.Height *= scale;
                        }

                        startPoint.X /= trueSize.Width;
                        startPoint.Y /= trueSize.Height;
                        endPoint.X /= trueSize.Width;
                        endPoint.Y /= trueSize.Height;
                        center.X /= trueSize.Width;
                        center.Y /= trueSize.Height;

                        if (double.IsNaN(center.X) || double.IsNaN(center.Y))
                        {
                            var midPoint = new Point((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);
                            var perpAngle = Math.Atan2(startPoint.Y - endPoint.Y, endPoint.X - startPoint.X);
                            
                            double diffX = startPoint.X - midPoint.X;
                            double diffY = startPoint.Y - midPoint.Y;
                            double distance = Math.Sqrt(diffX * diffX + diffY * diffY);
    
                            distance = Math.Sqrt(1 - distance * distance);
    
                            if ((arc.IsLargeArc && arc.SweepDirection == SweepDirection.Counterclockwise) ||
                                (!arc.IsLargeArc && arc.SweepDirection == SweepDirection.Clockwise))
                            {
                                center = new Point(midPoint.X + Math.Sin(perpAngle) * distance, midPoint.Y + Math.Cos(perpAngle) * distance);
                            }
                            else
                            {
                                center = new Point(midPoint.X - Math.Sin(perpAngle) * distance, midPoint.Y - Math.Cos(perpAngle) * distance);
                            }
                        }
            
                        double twoPi = Math.PI * 2;
                        double startAngle = Math.Atan2(startPoint.Y - center.Y, startPoint.X - center.X);
                        if (startAngle < 0)
                        {
                            startAngle += twoPi;
                        }
            
                        double endAngle = Math.Atan2(endPoint.Y - center.Y, endPoint.X - center.X);
                        if (endAngle < 0)
                        {
                            endAngle += twoPi;
                        }
                        
                        double arcAngle = Math.Abs(startAngle - endAngle);
                        if ((arcAngle < Math.PI && arc.IsLargeArc) || (arcAngle > Math.PI && !arc.IsLargeArc))
                        {
                            arcAngle = twoPi - arcAngle;
                        }
                        
                        if (arc.SweepDirection == SweepDirection.Counterclockwise)
                        {
                            arcAngle = -arcAngle;
                        }
                        
                        center.X *= trueSize.Width;
                        center.Y *= trueSize.Height;
                        
                        subpath.ArcTo(new RectF((center.X - trueSize.Width).GetScaledFloat(), (center.Y - trueSize.Height).GetScaledFloat(),
                            (center.X + trueSize.Width).GetScaledFloat(), (center.Y + trueSize.Height).GetScaledFloat()),
                            (float)(startAngle * (180 / Math.PI)), (float)(arcAngle * (180 / Math.PI)));
                    }

                    var bezier = segment as BezierSegment;
                    if (bezier != null)
                    {
                        subpath.CubicTo(bezier.ControlPoint1.X.GetScaledFloat(), bezier.ControlPoint1.Y.GetScaledFloat(),
                            bezier.ControlPoint2.X.GetScaledFloat(), bezier.ControlPoint2.Y.GetScaledFloat(),
                            bezier.EndPoint.X.GetScaledFloat(), bezier.EndPoint.Y.GetScaledFloat());

                        continue;
                    }

                    var quad = segment as QuadraticBezierSegment;
                    if (quad != null)
                    {
                        subpath.QuadTo(quad.ControlPoint.X.GetScaledFloat(), quad.ControlPoint.Y.GetScaledFloat(),
                            quad.EndPoint.X.GetScaledFloat(), quad.EndPoint.Y.GetScaledFloat());

                        continue;
                    }
                }
                
                if (figure.IsClosed)
                {
                    subpath.Close();
                }
                
                path.AddPath(subpath);
            }
        }

        private void OnImageLoaded(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }
        }

        private void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }
    }
}

