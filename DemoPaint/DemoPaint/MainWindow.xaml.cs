using MyContract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DemoPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Dictionary<string, IShape> _abilities = new Dictionary<string, IShape>();
        bool _isDrawing = false;
        Prototype _prototype = new Prototype();


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

            var button = (Button)sender;
            string name = (string)button.Tag;
            _prototype.type = name;
            button.Effect = new System.Windows.Media.Effects.DropShadowEffect()
            {
                BlurRadius = 10,
                ShadowDepth = 5
            };
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            Point _start = e.GetPosition(actualCanvas);

            _prototype.shape = (IShape)_abilities[_prototype.type].Clone();
            _prototype.shape.stroke.Color = _prototype.color;
            _prototype.shape.UpdateStart(_start);
            actualCanvas.Children.Add(new UIElement());
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                actualCanvas.Children.RemoveAt(actualCanvas.Children.Count - 1);

                Point _end = e.GetPosition(actualCanvas);
                _prototype.shape.UpdateEnd(_end);

                UIElement newShape = _prototype.shape.Draw();
                actualCanvas.Children.Add(newShape);
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            if (actualCanvas.Children[actualCanvas.Children.Count - 1] == new UIElement())
            {
                actualCanvas.Children.RemoveAt(actualCanvas.Children.Count - 1);
            }
        }

        private void ClrPcker_Background_SelectedColorChanged_1(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (_prototype.shape == null) return;
            _prototype.color = (Color)e.NewValue!;
        }
    }
}
