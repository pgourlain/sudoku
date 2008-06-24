using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace SilverlightSudoku
{
    /// <summary>
    /// it works like a standard viewbox, but in only one mode 'uniform'
    /// </summary>
    //at the begining i would like to make myviewbox with a FrameworkElement, but it constructor is internal !!!
    //[ContentProperty("Child")]
    public class MyViewBox1 : Panel
    {

        //public static readonly DependencyProperty ChildProperty = DependencyProperty.Register("Child", typeof(UIElement), typeof(MyViewBox1), null);

        //public UIElement Child 
        //{
        //    get { return (UIElement)this.GetValue(ChildProperty); }
        //    set { this.SetValue(ChildProperty, value); } 
        //}

        public UIElement Child
        {
            get
            {
                if (this.Children.Count <= 0)
                    return null;
                return this.Children[0];
            }
        }

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(MyViewBox1), null);

        public Stretch Stretch
        {
            get { return (Stretch)this.GetValue(StretchProperty); }
            set { this.SetValue(StretchProperty, value); }
        }

        public MyViewBox1()
        {
            this.Stretch = Stretch.Uniform;
            this.RenderTransform = _scale;
        }
        
        ScaleTransform _scale = new ScaleTransform();
        protected override Size ArrangeOverride(Size finalSize)
        {
            UIElement internalChild = this.Child;
            if (internalChild != null)
            {
                Size desiredSize = internalChild.DesiredSize;
                Size factor = ComputeScaleFactor(finalSize, desiredSize, this.Stretch);
                this._scale.ScaleX = factor.Width;
                this._scale.ScaleY = factor.Height;
                internalChild.Arrange(new Rect(new Point(), new Point(desiredSize.Width, desiredSize.Height)));
                finalSize.Width = factor.Width * desiredSize.Width;
                finalSize.Height = factor.Height * desiredSize.Height;
            }
            return finalSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UIElement internalChild = this.Child;
            Size size = new Size();
            if (internalChild != null)
            {
                Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
                internalChild.Measure(availableSize);
                Size desiredSize = internalChild.DesiredSize;
                Size factor = ComputeScaleFactor(constraint, desiredSize, this.Stretch);
                size.Width = factor.Width * desiredSize.Width;
                size.Height = factor.Height * desiredSize.Height;
            }
            return size;
        }

        /// <summary>
        /// it works like ViewBox (but only with uniform stretch)
        /// </summary>
        /// <param name="arrangeSize"></param>
        /// <param name="finalSize"></param>
        /// <param name="stretch"></param>
        /// <returns></returns>
        Size ComputeScaleFactor(Size arrangeSize, Size finalSize, Stretch stretch)
        {
            double width = 1.0;
            double height = 1.0;
            bool IsInfinityWidth = double.IsPositiveInfinity(arrangeSize.Width);
            bool IsInfinityHeight = double.IsPositiveInfinity(arrangeSize.Height);
            //gestion uniquement du uniform
            if ((IsInfinityWidth && IsInfinityHeight) || stretch != Stretch.Uniform)
            {
                return new Size(width, height);
            }
            width = IsZero(finalSize.Width) ? 0.0 : (arrangeSize.Width / finalSize.Width);
            height = IsZero(finalSize.Height) ? 0.0 : (arrangeSize.Height / finalSize.Height);
            if (IsInfinityWidth)
            {
                width = height;
            }
            else if (IsInfinityHeight)
            {
                height = width;
            }
            else
            {
                switch (stretch)
                {
                    case Stretch.Uniform :
                        double min = (width < height) ? width : height;
                        width = height = min;
                        break;
                }
            }

            return new Size(width, height);
        }

        private bool IsZero(double value)
        {
            return Math.Abs(value) < 0.000000002;
        }
    }
}
