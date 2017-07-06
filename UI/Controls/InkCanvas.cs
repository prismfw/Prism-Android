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
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Media.Inking;

namespace Prism.Android.UI.Controls
{
    /// <summary>
    /// Represents an Android implementation for an <see cref="INativeInkCanvas"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeInkCanvas))]
    public class InkCanvas : global::Android.Views.View, INativeInkCanvas
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
        /// Occurs when a property value changes.
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
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame { get; set; }

        /// <summary>
        /// Gets or sets how the ink canvas handles input.
        /// </summary>
        public InkInputMode InputMode
        {
            get { return inputMode; }
            set
            {
                if (value != inputMode)
                {
                    inputMode = value;
                    OnPropertyChanged(Prism.UI.Controls.InkCanvas.InputModeProperty);
                }
            }
        }
        private InkInputMode inputMode;

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
        /// Gets the ink strokes that are on the canvas.
        /// </summary>
        public IEnumerable<INativeInkStroke> Strokes
        {
            get { return strokes.OfType<INativeInkStroke>(); }
        }
        private List<Media.Inking.InkStroke> strokes = new List<Media.Inking.InkStroke>();

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
        
        private Bitmap canvasImage;
        private Paint canvasImagePaint = new Paint(PaintFlags.AntiAlias);
        private Media.Inking.InkStroke currentStroke;
        private Paint defaultPaint;
        private bool dryInk;
        private bool forceDraw;
        private int pointIndex;
        private PointF[] points;

        /// <summary>
        /// Initializes a new instance of the <see cref="InkCanvas"/> class.
        /// </summary>
        public InkCanvas()
            : base(Application.MainActivity)
        {
            defaultPaint = new Paint
            {
                Color = global::Android.Graphics.Color.Black,
                StrokeCap = Paint.Cap.Round
            };
            
            points = new PointF[5];
            
            SetBackgroundColor(global::Android.Graphics.Color.Transparent);
            SetWillNotDraw(false);
        }

        /// <summary>
        /// Adds the specified ink stroke to the canvas.
        /// </summary>
        /// <param name="stroke">The ink stroke to add.</param>
        public void AddStroke(INativeInkStroke stroke)
        {
            var inkStroke = stroke as Media.Inking.InkStroke;
            if (inkStroke != null)
            {
                strokes.Add(inkStroke);
                inkStroke.NeedsDrawing = true;
                inkStroke.Parent = this;
                Invalidate();
            }
        }

        /// <summary>
        /// Adds the specified ink strokes to the canvas.
        /// </summary>
        /// <param name="strokes">The ink strokes to add.</param>
        public void AddStrokes(IEnumerable<INativeInkStroke> strokes)
        {
            foreach (var stroke in strokes)
            {
                var inkStroke = stroke as Media.Inking.InkStroke;
                if (inkStroke != null)
                {
                    this.strokes.Add(inkStroke);
                    inkStroke.NeedsDrawing = true;
                    inkStroke.Parent = this;
                }
            }
            
            Invalidate();
        }

        /// <summary>
        /// Removes all ink strokes from the canvas.
        /// </summary>
        public void ClearStrokes()
        {
            foreach (var stroke in strokes)
            {
                if (stroke.Parent == this)
                {
                    stroke.Parent = null;
                }
            }
            strokes.Clear();
            
            canvasImage = null;
            Invalidate();
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
                base.OnTouchEvent(e);
                
                pointIndex = 0;
                currentStroke = null;
                
                return true;
            }
            if (e.Action == MotionEventActions.Down)
            {
                PointerPressed(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                
                if (inputMode == InkInputMode.Erasing)
                {
                    var point = new PointF(e.GetX(), e.GetY());
                    for (int i = strokes.Count - 1; i >= 0; i--)
                    {
                        if (StrokeContainsPoint(strokes[i], point))
                        {
                            strokes.RemoveAt(i);
                            
                            canvasImage = null;
                            forceDraw = true;
                            Invalidate();
                        }
                    }
                }
                else
                {
                    pointIndex = 0;
                    points[0] = new PointF(e.GetX(), e.GetY());
                    
                    currentStroke = new Media.Inking.InkStroke();
                    currentStroke.Parent = this;
                    currentStroke.Paint.Color = defaultPaint.Color;
                    currentStroke.Paint.StrokeCap = defaultPaint.StrokeCap;
                    currentStroke.Paint.StrokeWidth = defaultPaint.StrokeWidth;
                    strokes.Add(currentStroke);
                    
                    Invalidate();
                }
                
                return true;
            }
            if (e.Action == MotionEventActions.Move)
            {
                PointerMoved(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                
                if (inputMode == InkInputMode.Erasing)
                {
                    var point = new PointF(e.GetX(), e.GetY());
                    for (int i = strokes.Count - 1; i >= 0; i--)
                    {
                        if (StrokeContainsPoint(strokes[i], point))
                        {
                            strokes.RemoveAt(i);
                            
                            canvasImage = null;
                            forceDraw = true;
                            Invalidate();
                        }
                    }
                }
                else
                {
                    points[++pointIndex] = new PointF(e.GetX(), e.GetY());
                    if (pointIndex == 4)
                    {
                        var point1 = points[2];
                        var point2 = points[4];
                        var endPoint = new PointF((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);
                        var startPoint = points[0];
                        var cp1 = points[1];
                        
                        currentStroke.MoveTo(startPoint.X, startPoint.Y);
                        currentStroke.CubicTo(cp1.X, cp1.Y, point1.X, point1.Y, endPoint.X, endPoint.Y);
                        
                        points[0] = endPoint;
                        points[1] = point2;
                        pointIndex = 1;
                        
                        Invalidate();
                    }
                }
                
                return true;
            }
            if (e.Action == MotionEventActions.Up)
            {
                PointerReleased(this, e.GetPointerEventArgs(this));
                base.OnTouchEvent(e);
                
                dryInk = true;
                pointIndex = 0;
                currentStroke = null;
                
                Invalidate();
                
                return true;
            }
            return base.OnTouchEvent(e);
        }
        
        /// <summary>
        /// Updates the drawing attributes to apply to new ink strokes on the canvas.
        /// </summary>
        /// <param name="attributes">The drawing attributes to apply to new ink strokes.</param>
        public void UpdateDrawingAttributes(InkDrawingAttributes attributes)
        {
            defaultPaint.Color = attributes.Color.GetColor();
            defaultPaint.StrokeCap = attributes.PenTip == PenTipShape.Circle ? Paint.Cap.Round : Paint.Cap.Butt;
            defaultPaint.StrokeWidth = (float)(attributes.Size * Device.Current.DisplayScale);
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
            
            if (dryInk || forceDraw)
            {
                var bitmap = Bitmap.CreateBitmap(canvas.Width, canvas.Height, Bitmap.Config.Argb8888);
                var bitmapCanvas = new global::Android.Graphics.Canvas(bitmap);
                
                if (canvasImage != null)
                {
                    bitmapCanvas.DrawBitmap(canvasImage, 0, 0, canvasImagePaint);
                }
                
                for (int i = 0; i < strokes.Count; i++)
                {
                    var stroke = strokes[i];
                    if (stroke.NeedsDrawing || forceDraw)
                    {
                        bitmapCanvas.DrawPath(stroke, stroke.Paint);
                        stroke.NeedsDrawing = false;
                    }
                }
                
                canvasImage = bitmap;
                canvas.DrawBitmap(canvasImage, 0, 0, canvasImagePaint);
                
                dryInk = false;
                forceDraw = false;
            }
            else
            {
                if (canvasImage != null)
                {
                    canvas.DrawBitmap(canvasImage, 0, 0, canvasImagePaint);
                }
                
                for (int i = 0; i < strokes.Count; i++)
                {
                    var stroke = strokes[i];
                    if (stroke.NeedsDrawing)
                    {
                        canvas.DrawPath(stroke, stroke.Paint);
                    }
                }
            }
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

            Left = (int)Math.Ceiling(Frame.Left * Device.Current.DisplayScale);
            Top = (int)Math.Ceiling(Frame.Top * Device.Current.DisplayScale);
            Right = (int)Math.Ceiling(Frame.Right * Device.Current.DisplayScale);
            Bottom = (int)Math.Ceiling(Frame.Bottom * Device.Current.DisplayScale);

            base.OnLayout(changed, Left, Top, Right, Bottom);
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
        
        private bool StrokeContainsPoint(Media.Inking.InkStroke stroke, PointF point)
        {
            var pointEnumerator = stroke.Points.GetEnumerator();
            if (pointEnumerator.MoveNext())
            {
                while (true)
                {
                    var point1 = new Point(pointEnumerator.Current.X * Device.Current.DisplayScale,
                        pointEnumerator.Current.Y * Device.Current.DisplayScale);
                    
                    if (!pointEnumerator.MoveNext())
                    {
                        break;
                    }
                    
                    var point2 = new Point(pointEnumerator.Current.X * Device.Current.DisplayScale,
                        pointEnumerator.Current.Y * Device.Current.DisplayScale);
                    
                    double width = stroke.Paint.StrokeWidth / 2;
                    double slope = -Math.Atan(1 / ((point2.Y - point1.Y) / (point2.X - point1.X)));
                    double cos = width * Math.Cos(slope);
                    double sin = width * Math.Sin(slope);
                    
                    var corners = new Point[]
                    {
                        new Point(point1.X + cos, point1.Y + sin),
                        new Point(point2.X + cos, point2.Y + sin),
                        new Point(point2.X - cos, point2.Y - sin),
                        new Point(point1.X - cos, point1.Y - sin)
                    };
                    
                    bool retVal = false;
                    for (int i = 0, j = corners.Length - 1; i < corners.Length; j = i++)
                    {
                        if ((corners[i].Y < point.Y && corners[j].Y >= point.Y) || (corners[j].Y < point.Y && corners[i].Y >= point.Y))
                        {
                            if (corners[i].X + (point.Y - corners[i].Y) / (corners[j].Y - corners[i].Y) * (corners[j].X - corners[i].X) < point.X)
                            {
                                retVal = !retVal;
                            }
                        }
                    }
                    
                    if (retVal)
                    {
                        return retVal;
                    }
                }
            }
            
            return false;
        }
    }
}
