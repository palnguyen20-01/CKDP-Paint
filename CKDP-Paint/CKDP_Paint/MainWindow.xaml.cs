using MyContract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Xceed.Wpf.AvalonDock.Controls;
using static System.Windows.Forms.AxHost;

namespace CKDP_Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            actualCanvas.LayoutTransform = canvas_ScaleTranform;
            aboveCanvas.LayoutTransform = canvas_ScaleTranform;
        }

        Dictionary<string, IShape> _abilities = new Dictionary<string, IShape>();
        bool _isDrawing = false;
        bool _isErasing= false;
        static Point dragStart = new Point();
        Prototype _prototype = new Prototype();
        List<IShape> shapeList = new List<IShape>();
        Stack<UIElement> redoBuffer= new Stack<UIElement>();
        Stack<IShape> redoShapeBuffer= new Stack<IShape>();
        ScaleTransform canvas_ScaleTranform = new ScaleTransform();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Tự scan chương trình nạp lên các khả năng của mình
            var domain = AppDomain.CurrentDomain;
            var folder = domain.BaseDirectory;
            var folderInfo = new DirectoryInfo(folder);
            var dllFiles = folderInfo.GetFiles("*.dll");
            
            foreach(var dll in dllFiles)
            {
                Debug.WriteLine(dll.FullName);
                var assembly = Assembly.LoadFrom(dll.FullName);

                var types = assembly.GetTypes();
                
                foreach (var type in types)
                {
                    if (type.IsClass && 
                        typeof(IShape).IsAssignableFrom(type))
                    {
                        var shape = Activator.CreateInstance(type) as IShape;
                        _abilities.Add(shape!.Name, shape);
                    }
                }   
            }

            foreach (var ability in _abilities)
            {
                var button = new Button()
                {
                    Width = 50,
                    Height = 50,
                    Content = ability.Value.Name,
                    Tag = ability.Value.Name,
                };
                button.Content = new Image
                {
                    Source = new BitmapImage(new Uri(ability.Value.Icon, UriKind.Relative)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Stretch = Stretch.UniformToFill,
                };
                button.Click += ability_Click;
                abilitiesStackPanel.Children.Add(button);
            }
        }
        private void ability_Click(object sender, RoutedEventArgs e)
        {
            _isErasing = false;
            aboveCanvas.Cursor = Cursors.Arrow;

            var button = (Button)sender;
            string name = (string)button.Tag;
            _prototype.type = name;
            foreach(Button i_btn in abilitiesStackPanel.Children)
            {
                i_btn.Effect = new System.Windows.Media.Effects.DropShadowEffect()
                {
                    BlurRadius = 0,
                    ShadowDepth = 0
                };
            }
            button.Effect = new System.Windows.Media.Effects.DropShadowEffect()
            {
                BlurRadius = 10,
                ShadowDepth = 5
            };
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isErasing)
            {
                Point _point = e.GetPosition(actualCanvas);
                deletePainting(_point);
            }
            else
            {
                _isDrawing = true;
                Point _start = e.GetPosition(actualCanvas);

                if (!_abilities.ContainsKey(_prototype.type))
                {
                    _isDrawing = false;
                    return;
                }
                _prototype.shape = (IShape)_abilities[_prototype.type].Clone();
                _prototype.applyFormat();
                _prototype.shape.UpdateStart(_start);
                actualCanvas.Children.Add(new UIElement());
                shapeList.Add(_prototype.shape);
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                actualCanvas.Children.RemoveAt(actualCanvas.Children.Count - 1);
                shapeList.RemoveAt(shapeList.Count - 1);

                Point _end = e.GetPosition(actualCanvas);
                _prototype.shape.UpdateEnd(_end);

                UIElement newShape = _prototype.shape.Draw();
                actualCanvas.Children.Add(newShape);
                shapeList.Add(_prototype.shape);
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing)
            {
                _isDrawing = false;
                if (actualCanvas.Children.Count != 0 && actualCanvas.Children[actualCanvas.Children.Count - 1] == new UIElement())
                {
                    actualCanvas.Children.RemoveAt(actualCanvas.Children.Count - 1);
                    shapeList.RemoveAt(shapeList.Count - 1);
                }
                redoBuffer.Clear();
                redoShapeBuffer.Clear();
            }
        }

        private void ClrPcker_Background_SelectedColorChanged_1(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            _prototype.format.stroke.Color = (Color)e.NewValue!;
        }

        private void strokeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _prototype.format.strokeDashArray = ((strokeCombobox.SelectedItem as ComboBoxItem).Content as Line).StrokeDashArray;
            _prototype.format.strokeDashCap = ((strokeCombobox.SelectedItem as ComboBoxItem).Content as Line).StrokeDashCap;
        }

        private void thicknessCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _prototype.format.thickness = ((thicknessCombobox.SelectedItem as ComboBoxItem).Content as Line).StrokeThickness;
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            if(actualCanvas.Children.Count == 0) return;
            var lastObj = actualCanvas.Children[actualCanvas.Children.Count - 1];
            var lastShape = shapeList[shapeList.Count - 1];
            redoBuffer.Push(lastObj);
            redoShapeBuffer.Push(lastShape);
            actualCanvas.Children.Remove(lastObj);
            shapeList.Remove(lastShape);
        }

        private void redoButton_Click(object sender, RoutedEventArgs e)
        {
            if(redoBuffer.Count == 0) return;
            var lastObj = redoBuffer.Pop();
            var lastShape = redoShapeBuffer.Pop();
            actualCanvas.Children.Add(lastObj);
            shapeList.Add(lastShape);
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                canvas_ScaleTranform.ScaleX *= 1.2;
                canvas_ScaleTranform.ScaleY *= 1.2;
            }
            else
            {
                canvas_ScaleTranform.ScaleX /= 1.2;
                canvas_ScaleTranform.ScaleY /= 1.2;
            }
        }

        private void zoomInButton_Click(object sender, RoutedEventArgs e)
        {
            canvas_ScaleTranform.ScaleX *= 1.2;
            canvas_ScaleTranform.ScaleY *= 1.2;
        }

        private void zoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            canvas_ScaleTranform.ScaleX /= 1.2;
            canvas_ScaleTranform.ScaleY /= 1.2;
        }

        private void resetZoomButton_Click(object sender, RoutedEventArgs e)
        {
            canvas_ScaleTranform.ScaleX = 1;
            canvas_ScaleTranform.ScaleY = 1;
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "PNG (*.png)|*.png";

            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;               
                saveCanvasToImage(actualCanvas, path);
            }
        }
        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "PNG (*.png)|*.png";

            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
          
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute));

                Image image = new Image();
                image.Source = brush.ImageSource;
                actualCanvas.Children.Add(image);
                //actualCanvas.Background = brush;
            }
        }

        private void saveCanvasToImage(Canvas canvas, string filename)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight,
             96d, 96d, PixelFormats.Pbgra32);
            canvas.Measure(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight));
            canvas.Arrange(new Rect(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight)));

            renderBitmap.Render(canvas);
        
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filename))
            {
                pngEncoder.Save(file);
            }          
        }

        private void drag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            dragStart = e.GetPosition(element);
            element.CaptureMouse();
        }

        private void drag_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            dragStart = new Point();
            element.ReleaseMouseCapture();
        }

        private void drag_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragStart != new Point() && e.LeftButton == MouseButtonState.Pressed)
            {
                var element = (UIElement)sender;
                var p2 = e.GetPosition(actualCanvas);
                Canvas.SetLeft(element, p2.X - dragStart.X);
                Canvas.SetTop(element, p2.Y - dragStart.Y);
            }
        }


        private void eraserButton_Click(object sender, RoutedEventArgs e)
        {
            _isErasing = true;
            aboveCanvas.Cursor = Cursors.Cross;
        }

        private UIElement detectShapeByPosition(Point point)
        {
            for(int i = actualCanvas.Children.Count - 1; i >= 0 ; i--)
            {
                switch (shapeList[i].GetType().Name)
                {
                    case "MyLine":
                        {
                            double a = -(shapeList[i].End.Y - shapeList[i].Start.Y);
                            double b = (shapeList[i].End.X - shapeList[i].Start.X);
                            double c = a * -(shapeList[i].Start.X) + b * -(shapeList[i].Start.Y);
                            double distance = Math.Abs(a * point.X + b * point.Y + c) / Math.Sqrt(a * a + b * b);
                            if (distance <= shapeList[i].thickness / 2 &&
                                point.X <= Math.Max(shapeList[i].Start.X, shapeList[i].End.X) &&
                                point.X >= Math.Min(shapeList[i].Start.X, shapeList[i].End.X) &&
                                point.Y <= Math.Max(shapeList[i].Start.Y, shapeList[i].End.Y) &&
                                point.Y >= Math.Min(shapeList[i].Start.Y, shapeList[i].End.Y))
                            {
                                return actualCanvas.Children[i];
                            }
                            else continue;
                        }
                    case "MyRectangle": case "MySquare":
                        {
                            double thickness = shapeList[i].thickness;
                            if ((point.X <= Math.Max(shapeList[i].Start.X, shapeList[i].End.X) &&
                                point.X >= Math.Min(shapeList[i].Start.X, shapeList[i].End.X) &&
                                point.Y <= (Math.Min(shapeList[i].Start.Y, shapeList[i].End.Y) + thickness) &&
                                point.Y >= Math.Min(shapeList[i].Start.Y, shapeList[i].End.Y))
                                ||
                                (point.X <= Math.Max(shapeList[i].Start.X, shapeList[i].End.X) &&
                                point.X >= Math.Min(shapeList[i].Start.X, shapeList[i].End.X) &&
                                point.Y <= Math.Max(shapeList[i].Start.Y, shapeList[i].End.Y) &&
                                point.Y >= (Math.Max(shapeList[i].Start.Y, shapeList[i].End.Y) - thickness))
                                ||
                                (point.X <= (Math.Min(shapeList[i].Start.X, shapeList[i].End.X) + thickness) &&
                                point.X >= Math.Min(shapeList[i].Start.X, shapeList[i].End.X) &&
                                point.Y <= Math.Max(shapeList[i].Start.Y, shapeList[i].End.Y) &&
                                point.Y >= Math.Min(shapeList[i].Start.Y, shapeList[i].End.Y))
                                ||
                                (point.X <= Math.Max(shapeList[i].Start.X, shapeList[i].End.X) &&
                                point.X >= (Math.Max(shapeList[i].Start.X, shapeList[i].End.X) - thickness) &&
                                point.Y <= Math.Max(shapeList[i].Start.Y, shapeList[i].End.Y) &&
                                point.Y >= Math.Min(shapeList[i].Start.Y, shapeList[i].End.Y)))
                            {
                                return actualCanvas.Children[i];
                            }
                            else continue;
                        }
                    case "MyCircle":
                        {
                            double width = Math.Abs(shapeList[i].End.X - shapeList[i].Start.X);
                            double height = Math.Abs(shapeList[i].End.Y - shapeList[i].Start.Y);
                            double diameter = Math.Min(width, height);
                            double radius = diameter/2;
                            double thickness = shapeList[i].thickness;
                            int x_sign;
                            int y_sign;
                            if (shapeList[i].End.X > shapeList[i].Start.X)
                            {
                                x_sign = 1;
                            }
                            else
                            {
                                x_sign = -1;
                            }

                            if (shapeList[i].End.Y > shapeList[i].Start.Y)
                            {
                                y_sign = 1;
                            }
                            else
                            {
                                y_sign = -1;
                            }

                            Point center = new Point
                            (
                                Math.Min(shapeList[i].Start.X, shapeList[i].Start.X + x_sign * diameter) + radius,
                                Math.Min(shapeList[i].Start.Y, shapeList[i].Start.Y + y_sign * diameter) + radius
                            );
                            double distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));
                            if (distance <= radius && distance >= radius - thickness)
                            {
                                return actualCanvas.Children[i];
                            }
                            else continue;
                        }
                    case "MyEllipse":
                        {
                            Point left_top = new Point
                            (
                                Math.Min(shapeList[i].Start.X, shapeList[i].End.X),
                                Math.Min(shapeList[i].Start.Y, shapeList[i].End.Y)
                            );
                            double a = Math.Abs(shapeList[i].End.X - shapeList[i].Start.X) / 2;
                            double b = Math.Abs(shapeList[i].End.Y - shapeList[i].Start.Y) / 2;
                            double thickness = shapeList[i].thickness;
                            Point center = new Point(left_top.X + a, left_top.Y + b);
                            double result_1 = 
                                Math.Pow(point.X - center.X, 2) / Math.Pow(a, 2) + 
                                Math.Pow(point.Y - center.Y, 2) / Math.Pow(b, 2);
                            double result_2 = 
                                Math.Pow(point.X - center.X, 2) / Math.Pow(a - thickness, 2) + 
                                Math.Pow(point.Y - center.Y, 2) / Math.Pow(b - thickness, 2);
                            if (result_1 <= 1 && result_2 >= 1)
                            {
                                return actualCanvas.Children[i];
                            }
                            else continue;
                        }
                }
            }
            return new UIElement();
        }

        private void deletePainting(Point point)
        {
            UIElement element = detectShapeByPosition(point);
            if (actualCanvas.Children.IndexOf(element) == -1) return;

            redoBuffer.Push(element);
            redoShapeBuffer.Push(shapeList[actualCanvas.Children.IndexOf(element)]);

            shapeList.RemoveAt(actualCanvas.Children.IndexOf(element));
            actualCanvas.Children.Remove(element);
        }
    }
}
