using CKDP_Paint.MyHistory;
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
using static System.Formats.Asn1.AsnWriter;
using static System.Windows.Forms.AxHost;

namespace CKDP_Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private RenderTargetBitmap _renderTargetBitmap;
        public MainWindow()
        {
            InitializeComponent();
            actualCanvas.LayoutTransform = canvas_ScaleTranform;
            aboveCanvas.LayoutTransform = canvas_ScaleTranform;

            _renderTargetBitmap = new RenderTargetBitmap((int)actualCanvas.Width, (int)actualCanvas.Height, 96, 96, PixelFormats.Default);

        }

        Dictionary<string, IShape> _abilities = new Dictionary<string, IShape>();
        bool _isDrawing = false;
        bool _isErasing= false;
        bool _isMoving= false;
        bool _isFilling=false;

        Prototype _prototype = new Prototype();

        List<IShape> shapeList = new List<IShape>();

        Stack<History> undoHistoryBuffer = new Stack<History>();
        Stack<History> redoHistoryBuffer = new Stack<History>();

        UIElement movingUIElement;
        IShape movingShape;
        Point movingStart;

        ScaleTransform canvas_ScaleTranform = new ScaleTransform();
        private void BFSFillColor(Point p, Color newColor)
        {
            Color oldColor = GetPixelColor(p.X, p.Y);

            if (oldColor == newColor)
            {
                // Already filled with the new color
                return;
            }

            Queue<Point> queue = new Queue<Point>();
            Dictionary<Point, bool> isVisited = new Dictionary<Point, bool>();
            int [] dx = { -1, 1, 0, 0 };
            int [] dy = { 0, 0, -1, 1 };

            queue.Enqueue(p);
            isVisited.Add(p, true);

            var cnt = 0;
            while (queue.Count > 0)
            {
                Point currentPoint = queue.Dequeue();
                //MessageBox.Show(currentPoint.X.ToString() + "??" + currentPoint.Y.ToString());
                cnt++;
                if (cnt > 5000) break;
                Color getCurrentColor = GetPixelColor(currentPoint.X, currentPoint.Y);
                if (getCurrentColor.Equals(oldColor))
                {

                    SetPixelColor(currentPoint.X, currentPoint.Y, newColor);
                    for (int i = 0; i < 4; ++i) { 
                        var ux= currentPoint.X + dx[i];
                        var uy = currentPoint.Y + dy[i];
                        if (ux>0 && ux < actualCanvas.Width - 1 && uy>0 && uy< actualCanvas.Height - 1)
                        {
                            Point NewPoint = new Point(ux, uy);
                            if (!isVisited.ContainsKey(NewPoint))
                            {
                                queue.Enqueue(NewPoint);
                                isVisited.Add(NewPoint, true);

                            }

                        }

                    }
                    
                }
            }
            var a = 5;
            a = 10;

        }
        private void SetPixelColor(double x, double y, Color color)
        {
            int pixelX = (int)(x * actualCanvas.Width / actualCanvas.ActualWidth);
            int pixelY = (int)(y * actualCanvas.Height / actualCanvas.ActualHeight);

            Rectangle rect = new Rectangle
            {
                Fill = new SolidColorBrush(color),
                Width = 1,
                Height = 1
            };

            Canvas.SetLeft(rect, pixelX);
            Canvas.SetTop(rect, pixelY);

            actualCanvas.Children.Add(rect);
        }

        
        private Color GetPixelColor(int x, int y, RenderTargetBitmap bmp)
        {
            byte[] pixel = new byte[4];
            bmp.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);
            return Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
        }
        private Color GetPixelColor(double x, double y)
        {
            int pixelX = (int)(x * actualCanvas.Width / actualCanvas.ActualWidth);
            int pixelY = (int)(y * actualCanvas.Height / actualCanvas.ActualHeight);

            _renderTargetBitmap.Render(actualCanvas);

            byte[] pixels = new byte[4];
            _renderTargetBitmap.CopyPixels(new Int32Rect(pixelX, pixelY, 1, 1), pixels, 4, 0);
            Color res = Color.FromRgb(pixels[2], pixels[1], pixels[0]);
            return res;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Tự scan chương trình nạp lên các khả năng của mình
            loadPlugin();
        }
        private void ability_Click(object sender, RoutedEventArgs e)
        {
            _isErasing = false;
            _isMoving = false;
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

        private void loadPlugin()
        {
            var domain = AppDomain.CurrentDomain;
            var folder = domain.BaseDirectory;
            var folderInfo = new DirectoryInfo(folder);
            var dllFiles = folderInfo.GetFiles("*.dll");

            _abilities.Clear();
            abilitiesStackPanel.Children.Clear();

            foreach (var dll in dllFiles)
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

        private void reloadPluginButton_Click(object sender, RoutedEventArgs e)
        {
            loadPlugin();
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            if (_isFilling)
            {
                Point _point = e.GetPosition(actualCanvas);
                BFSFillColor(_point, _prototype.format.stroke.Color);
                //GetPixelColor(e.GetPosition(actualCanvas).X, e.GetPosition(actualCanvas).Y);
                //MessageBox.Show(GetPixelColor(e.GetPosition(actualCanvas).X, e.GetPosition(actualCanvas).Y).ToString());

            }
            //SetPixelColor(e.GetPosition(actualCanvas).X, e.GetPosition(actualCanvas).Y,_prototype.format.stroke.Color);
            if (_isErasing)
            {
                Point _point = e.GetPosition(actualCanvas);
                deletePainting(_point);
            }
            else if (_isMoving)
            {
                movingStart = e.GetPosition(actualCanvas);
                movingUIElement = detectShapeByPosition(movingStart);
                if (actualCanvas.Children.Contains(movingUIElement))
                {
                    movingShape = shapeList[actualCanvas.Children.IndexOf(movingUIElement)];
                    undoHistoryBuffer.Push(new History_Move(movingUIElement, new Point(Math.Min(movingShape.Start.X, movingShape.End.X), Math.Min(movingShape.Start.Y, movingShape.End.Y))));
                    redoHistoryBuffer.Clear();
                }
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
                undoHistoryBuffer.Push(new History_Add(new UIElement()));
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMoving)
            {
                if (!actualCanvas.Children.Contains(movingUIElement)) return;
                Point _point = e.GetPosition(actualCanvas);
                moveShape(_point);
            }
            else if (_isDrawing)
            {
                actualCanvas.Children.RemoveAt(actualCanvas.Children.Count - 1);
                shapeList.RemoveAt(shapeList.Count - 1);
                undoHistoryBuffer.Pop();

                Point _end = e.GetPosition(actualCanvas);
                _prototype.shape.UpdateEnd(_end);

                UIElement newShape = _prototype.shape.Draw();
                actualCanvas.Children.Add(newShape);
                shapeList.Add(_prototype.shape);
                undoHistoryBuffer.Push(new History_Add(newShape));
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMoving)
            {
                movingStart = new Point(0,0);
                movingUIElement = new UIElement();
            }
            else if (_isDrawing)
            {
                _isDrawing = false;
                if (actualCanvas.Children.Count != 0 && actualCanvas.Children[actualCanvas.Children.Count - 1] == new UIElement())
                {
                    actualCanvas.Children.RemoveAt(actualCanvas.Children.Count - 1);
                    shapeList.RemoveAt(shapeList.Count - 1);
                    undoHistoryBuffer.Pop();
                }
                redoHistoryBuffer.Clear();
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
            if (undoHistoryBuffer.Count == 0) return;
            History _undo = undoHistoryBuffer.Pop();
            if (_undo.Name == "Add")
            {
                redoHistoryBuffer.Push(new History_Delete(_undo.Object, shapeList[actualCanvas.Children.IndexOf(_undo.Object)]));
                shapeList.RemoveAt(actualCanvas.Children.IndexOf(_undo.Object));
                actualCanvas.Children.Remove(_undo.Object);
            }
            else if(_undo.Name == "Delete")
            {
                actualCanvas.Children.Add(_undo.Object);
                shapeList.Add((_undo as History_Delete).ObjectShape);
                redoHistoryBuffer.Push(new History_Add(_undo.Object));
            }
            else if(_undo.Name == "Move")
            {
                movingUIElement = _undo.Object;
                movingShape = shapeList[actualCanvas.Children.IndexOf(_undo.Object)];
                movingStart = new Point(Math.Min(movingShape.Start.X, movingShape.End.X), Math.Min(movingShape.Start.Y, movingShape.End.Y));
                Point newPoint = (_undo as History_Move).Position;
                (_undo as History_Move).Position = movingStart;
                redoHistoryBuffer.Push(_undo);
                moveShape(newPoint);
                movingUIElement = new UIElement();
                movingShape = null;
            }
        }

        private void redoButton_Click(object sender, RoutedEventArgs e)
        {
            if (redoHistoryBuffer.Count == 0) return;
            History _redo = redoHistoryBuffer.Pop();
            if (_redo.Name == "Add")
            {
                undoHistoryBuffer.Push(new History_Delete(_redo.Object, shapeList[actualCanvas.Children.IndexOf(_redo.Object)]));
                shapeList.RemoveAt(actualCanvas.Children.IndexOf(_redo.Object));
                actualCanvas.Children.Remove(_redo.Object);
            }
            else if (_redo.Name == "Delete")
            {
                actualCanvas.Children.Add(_redo.Object);
                shapeList.Add((_redo as History_Delete).ObjectShape);
                undoHistoryBuffer.Push(new History_Add(_redo.Object));
            }
            else if (_redo.Name == "Move")
            {
                movingUIElement = _redo.Object;
                movingShape = shapeList[actualCanvas.Children.IndexOf(_redo.Object)];
                movingStart = new Point(Math.Min(movingShape.Start.X, movingShape.End.X), Math.Min(movingShape.Start.Y, movingShape.End.Y));
                Point newPoint = (_redo as History_Move).Position;
                (_redo as History_Move).Position = movingStart;
                undoHistoryBuffer.Push(_redo);
                moveShape(newPoint);
                movingUIElement = new UIElement();
                movingShape = null;
            }
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
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height,
             96d, 96d, PixelFormats.Pbgra32);
            canvas.Measure(new Size((int)canvas.Width, (int)canvas.Height));
            canvas.Arrange(new Rect(new Size((int)canvas.Width, (int)canvas.Height)));

            renderBitmap.Render(canvas);
        
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filename))
            {
                pngEncoder.Save(file);
            }          
        }

        private void eraserButton_Click(object sender, RoutedEventArgs e)
        {
            _isErasing = true;
            _isMoving = false;
            _isFilling = false;
            _isDrawing = false;
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

            undoHistoryBuffer.Push(new History_Delete(element, shapeList[actualCanvas.Children.IndexOf(element)]));
            redoHistoryBuffer.Clear();

            shapeList.RemoveAt(actualCanvas.Children.IndexOf(element));
            actualCanvas.Children.Remove(element);
        }

        private void moveButton_Click(object sender, RoutedEventArgs e)
        {
            _isMoving = true;
            _isErasing = false;
            _isDrawing = false;
            aboveCanvas.Cursor = Cursors.Hand;
        }

        private void moveShape(Point _point)
        {
            double deltaX = _point.X - movingStart.X;
            double deltaY = _point.Y - movingStart.Y;
            movingStart = _point;
            movingShape.UpdateStart(new Point(movingShape.Start.X + deltaX, movingShape.Start.Y + deltaY));
            movingShape.UpdateEnd(new Point(movingShape.End.X + deltaX, movingShape.End.Y + deltaY));
            switch (movingShape.GetType().Name)
            {
                case "MyLine":
                    {
                        (movingUIElement as Line).X1 = movingShape.Start.X;
                        (movingUIElement as Line).Y1 = movingShape.Start.Y;
                        (movingUIElement as Line).X2 = movingShape.End.X;
                        (movingUIElement as Line).Y2 = movingShape.End.Y;
                        break;
                    }
                case "MyRectangle": case "MySquare":
                    {
                        Canvas.SetLeft(movingUIElement, Math.Min(movingShape.Start.X, movingShape.End.X));
                        Canvas.SetTop(movingUIElement, Math.Min(movingShape.Start.Y, movingShape.End.Y));
                        break;
                    }
                case "MyCircle":
                    {
                        double diameter = Math.Min(Math.Abs(movingShape.End.X - movingShape.Start.X), Math.Abs(movingShape.End.Y - movingShape.Start.Y));

                        int x_sign;
                        int y_sign;
                        if (movingShape.End.X > movingShape.Start.X)
                        {
                            x_sign = 1;
                        }
                        else
                        {
                            x_sign = -1;
                        }

                        if (movingShape.End.Y > movingShape.Start.Y)
                        {
                            y_sign = 1;
                        }
                        else
                        {
                            y_sign = -1;
                        }
                        Canvas.SetLeft(movingUIElement, Math.Min(movingShape.Start.X, movingShape.Start.X + x_sign * diameter));
                        Canvas.SetTop(movingUIElement, Math.Min(movingShape.Start.Y, movingShape.Start.Y + y_sign * diameter));
                        break;
                    }
                case "MyEllipse":
                    {
                        Canvas.SetLeft(movingUIElement, Math.Min(movingShape.Start.X, movingShape.End.X));
                        Canvas.SetTop(movingUIElement, Math.Min(movingShape.Start.Y, movingShape.End.Y));
                        break;
                    }
            }
        }

    }
}
