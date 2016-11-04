using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace VideoARDemo.Target
{
    public class RectangleObj : GeometryObj
    {
        CircleObj _selectedIcon;
        Rectangle _rect;
        public RectangleObj(int width, int height, System.Windows.Media.SolidColorBrush stroke = null, System.Windows.Media.SolidColorBrush fill = null, bool needFill = true)
            : base(stroke, fill, needFill)
        {
            _rect = new Rectangle();
            _rect.Fill = fill;
            _rect.Stroke = stroke;
            UpdateSize(width, height);
            this.Children.Add(_rect);
        }
        public RectangleObj(int side, System.Windows.Media.SolidColorBrush stroke = null, System.Windows.Media.SolidColorBrush fill = null, bool needFill = true)
            : this(side, side, stroke, fill, needFill)
        {
        }

        public void UpdateSize(double width, double height)
        {
            _rect.Width = width;
            _rect.Height = height;
            Canvas.SetLeft(_rect, -width / 2);
            Canvas.SetTop(_rect, -height / 2);
        }
        
        bool _isSelected;
        public bool Selected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    if (_selectedIcon == null)
                    {
                        _selectedIcon = new CircleObj(20, 1, System.Windows.Media.Brushes.Blue, null, false);
                        this.Children.Add(_selectedIcon);
                    }
                }
                else
                    removeSelectedIcon();
            }
        }

        private void removeSelectedIcon()
        {
            if (_selectedIcon != null)
                this.Children.Remove(_selectedIcon);
            _selectedIcon = null;
        }
    }
}
