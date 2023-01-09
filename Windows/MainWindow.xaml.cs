using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using _3dGraphics.Graphics;

namespace _3dGraphics.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Action _leftArrowPressedCmd;
        private readonly Action _leftArrowReleasedCmd;
        private readonly Action _rightArrowPressedCmd;
        private readonly Action _rightArrowReleasedCmd;
        private readonly Action _upArrowPressedCmd;
        private readonly Action _upArrowReleasedCmd;
        private readonly Action _downArrowPressedCmd;
        private readonly Action _downArrowReleasedCmd;
        private readonly Action _pgUpPressedCmd;
        private readonly Action _pgUpReleasedCmd;
        private readonly Action _pgDownPressedCmd;
        private readonly Action _pgDownReleasedCmd;

        public MainWindow(Action leftArrowPressedCmd, Action leftArrowReleasedCmd, Action rightArrowPressedCmd, Action rightArrowReleasedCmd,
                            Action upArrowPressedCmd, Action upArrowReleasedCmd, Action downArrowPressedCmd, Action downArrowReleasedCmd,
                            Action pgUpPressedCmd, Action pgUpReleasedCmd, Action pgDownPressedCmd, Action pgDownReleasedCmd)
        {
            _leftArrowPressedCmd= leftArrowPressedCmd;
            _leftArrowReleasedCmd= leftArrowReleasedCmd;
            _rightArrowPressedCmd = rightArrowPressedCmd;
            _rightArrowReleasedCmd= rightArrowReleasedCmd;
            _upArrowPressedCmd=upArrowPressedCmd;
            _upArrowReleasedCmd=upArrowReleasedCmd;
            _downArrowPressedCmd=downArrowPressedCmd;
            _downArrowReleasedCmd=downArrowReleasedCmd;
            _pgUpPressedCmd = pgUpPressedCmd;
            _pgUpReleasedCmd = pgUpReleasedCmd;
            _pgDownPressedCmd = pgDownPressedCmd;
            _pgDownReleasedCmd= pgDownReleasedCmd;            
            
            InitializeComponent();           
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ///DrawLine(new Point(5,5), new Point(200, 200));
            //DrawTriangle(new Triangle( new Vertex(0.1f,0.1f,0,0) , new Vertex(0.5f,0.5f,0,0), new Vertex(0.2f,0.9f,0,0)));

        }

        public void DrawLine(Point p1, Point p2)
        {
            _canvas.Children.Add(new Line() { X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y, Stroke = Brushes.White });
        }

        public void DrawTriangle(Point p1, Point p2, Point p3)
        {
            Polygon myPolygon = new Polygon() { Points = { p1, p2, p3 }, Stroke = Brushes.White, StrokeThickness = 0.25 };
            _canvas.Children.Add(myPolygon);
        }

        public void ClearCanvas() 
        { 
            _canvas.Children.Clear(); 
        } 

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
           
            Key pressedKey = e.Key;

            switch (pressedKey)
            {
                case Key.Left:
                    _leftArrowPressedCmd.Invoke();
                    break;
                case Key.Right:
                    _rightArrowPressedCmd.Invoke();
                    break;
                case Key.Up:
                    _upArrowPressedCmd.Invoke();
                    break;
                case Key.Down:
                    _downArrowPressedCmd.Invoke();
                    break;
                case Key.PageUp:
                    _pgUpPressedCmd.Invoke();
                    break;
                case Key.PageDown:
                    _pgDownPressedCmd.Invoke();
                    break;
                default:
                    break;
            }
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
           
            Key releasedKey = e.Key;

            switch (releasedKey)
            {
                case Key.Left:
                    _leftArrowReleasedCmd.Invoke();
                    break;
                case Key.Right:
                    _rightArrowReleasedCmd.Invoke();
                    break;
                case Key.Up:
                    _upArrowReleasedCmd.Invoke();
                    break;
                case Key.Down:
                    _downArrowReleasedCmd.Invoke();
                    break;
                case Key.PageUp:
                    _pgUpReleasedCmd.Invoke();
                    break;
                case Key.PageDown:
                    _pgDownReleasedCmd.Invoke();
                    break;
                default:
                    break;
            }
            
            
        }
    }
}
