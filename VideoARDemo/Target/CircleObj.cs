using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VideoARDemo.Target
{
    public class CircleObj : GeometryObj
    {
        double _radius;
        int _strokeThickness;
        bool _showTriangle;

        Shape _icon;

        public CircleObj(double radius, int strokeThickness, System.Windows.Media.SolidColorBrush stroke = null, System.Windows.Media.SolidColorBrush fill = null, bool needFill = true)
            : base(stroke, fill, needFill)
        {
            initIcon(radius, strokeThickness);
        }
        
        public override bool IsFill
        {
            get { return base.IsFill; }
            set
            {
                if (base.IsFill != value)
                {
                    base.IsFill = value;
                    updateFill();
                }
            }
        }


        public override double OpacityInfo
        {
            get { return base.OpacityInfo; }
            set
            {
                base.OpacityInfo = value;
                updateFill();
            }
        }

        private void initIcon(double radius, int strokeThickness)
        {
            _radius = radius;
            _strokeThickness = strokeThickness;
            _icon = newEllipse(radius);
            _icon.Fill = FillColor;
            _icon.Stroke = StrokeColor;
            _icon.StrokeThickness = strokeThickness;
            this.Children.Add(_icon);
        }

        Ellipse newEllipse(double radius)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Height = radius * 2 + 1;
            ellipse.Width = radius * 2 + 1;
            Canvas.SetLeft(ellipse, -radius);
            Canvas.SetTop(ellipse, -radius);
            return ellipse;
        }
        
        private void updateFill()
        {
            if (_icon != null)
            {
                if (IsFill)
                    _icon.Fill = FillColor;
                else
                    _icon.Fill = null;
            }
        }
    }
}