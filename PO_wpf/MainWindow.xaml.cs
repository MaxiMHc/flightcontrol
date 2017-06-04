﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using projekt_PO;
using System.ComponentModel;

// DODAJ WYSOKOSC DO TABELKI W XAML

namespace PO_wpf
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    /// 
    public static class WindowConstanst
    {
        public const double VehilceImageSize = 5;
    }
    public partial class MainWindow : Window
    {
        public const string IMG = "pack://application:,,,/img/placeholder.bmp";
        //public Random random = new Random();
        public MainWindow()
        {
            Generator generator = new Generator();
            Map map = new Map();
            Obstacle obs = new Obstacle(0,0,10,10,200);

            List<VehicleObject> vehicleobjectlist = new List<VehicleObject>();

            Helicopter a = new Helicopter();
            Plane b = new Plane();

            a.Position = new projekt_PO.Point(0, 0);

            //a.Routes = generator.generateRoutes(3, a); //<------wygenerowana trasa
            map.Vehicles.AddRange(generator.generateVehicles(5)); //<---- tutaj można zmienić ilość pojazdów które są generowane

            a.Position = new projekt_PO.Point(0, 0);
            Segment A1 = new Segment(new projekt_PO.Point(0, 0), new projekt_PO.Point(400, 400), 100, 500);
            Segment A2 = new Segment(new projekt_PO.Point(400, 400), new projekt_PO.Point(500, 400), 50, 500);
            a.Routes = new List<Segment> { A1, A2 };

            map.addVehicle(a);

            b.Position = new projekt_PO.Point(200, 0);
            Segment B1 = new Segment(new projekt_PO.Point(200, 0), new projekt_PO.Point(0, 200), 100, 500);
            Segment B2 = new Segment(new projekt_PO.Point(0, 200), new projekt_PO.Point(200, 400), 50, 500);
            b.Routes = new List<Segment> { B1, B2 };

            Console.WriteLine(b.Routes[0].Begin.X);

            map.addVehicle(b);

            InitializeComponent();

            Width = Constants.mapSizeY+500;             //
            Height = Constants.mapSizeX+100;            //rozmiar okna
            MapBorder.Width = Constants.mapSizeY + 4;       //i mapy
            MapBorder.Height = Constants.mapSizeX + 4;      //
            MapCanvas.Width = Constants.mapSizeY;       
            MapCanvas.Height = Constants.mapSizeX;

            Image obsimg = AddObstacle(obs);

            foreach (Vehicle v in map.Vehicles)
            {
                VehicleObject obj = new VehicleObject(v, AddVehicle(v), AddLines(v), AddBorder(v));
                vehicleobjectlist.Add(obj);
            }

            VehicleList.ItemsSource = vehicleobjectlist;

            map.DetectAllCollisions(map);
            map.DetectAllProximities(map);

            CollisionsList.ItemsSource = map.Collisions;
            ProximitiesList.ItemsSource = map.Proximities;

            StartAsyncProcess(map, vehicleobjectlist);

            
        }

        private async void StartAsyncProcess(Map map, List<VehicleObject> list)           //metoda asynchroniczna
        {
            await Task.Run(() => ProcessNextFrame(map, list));
        }

        private async void ProcessNextFrame(Map map, List<VehicleObject> list)
        {
            while (true)
            {
                if (this.Dispatcher.Invoke(() => (bool)StartControl.IsChecked))
                {
                    Task.Delay(1000).Wait();    //gdy przycisk do normalnej symulacji jest wcisnięty, działaj normalnie
                }
                else
                {
                    await Task.Run(() =>
                    {
                        Task.Delay(250).Wait();
                        while ( (this.Dispatcher.Invoke(() => StepControl.IsPressed)) == false )
                        {
                            //pusty while czeka na przyciśnięcie przycisku
                        }
                        
                    });
                }
                
                map.nextFrame();
                //BindingOperations.GetBindingExpressionBase(VehicleList, ListView.ItemsSourceProperty).UpdateTarget();
                this.Dispatcher.Invoke(() =>        //gdy inny wątek chce zmienić UI wątku głównego używamy tej instrukcji
                {
                    VehicleList.ItemsSource = null;         //Update Binding
                    VehicleList.ItemsSource = list;         //Refreshes Values
                    CollisionsList.ItemsSource = null;
                    CollisionsList.ItemsSource = map.Collisions;
                    ProximitiesList.ItemsSource = null;
                    ProximitiesList.ItemsSource = map.Proximities;

                    foreach (VehicleObject obj in list)
                    {
                        //obj.Img.Margin = new Thickness(obj.Vhc.Position.X, 0, 0, obj.Vhc.Position.Y);
                        Canvas.SetBottom(obj.Img, obj.Vhc.Position.Y - (obj.Img.Height)/2);
                        Canvas.SetLeft(obj.Img, obj.Vhc.Position.X - (obj.Img.Width)/2);
                        Canvas.SetBottom(obj.Brdr, obj.Vhc.Position.Y - (obj.Img.Height) / 2 - 2);
                        Canvas.SetLeft(obj.Brdr, obj.Vhc.Position.X - (obj.Img.Width) / 2 - 2);

                        obj.Lines[0].X1 = obj.Vhc.Position.X;
                        obj.Lines[0].Y1 = Constants.mapSizeY - obj.Vhc.Position.Y;
                        if (obj.Vhc.CurrentSegmentIndex > obj.CurrentLineIndex)
                        {
                            obj.Lines.RemoveAt(0);
                            obj.CurrentLineIndex++;
                        }

                        //if (obj.Vhc.ReachedDestination)
                        //{
                        //    Image img = new Image();
                        //    img.Source = new BitmapImage(new Uri(IMG));
                        //    img.Height = 1;
                        //    img.Width = 1;

                        //    MapCanvas.Children.Add(img);

                        //    Canvas.SetBottom(img, obj.Vhc.Position.Y - (img.Height) / 2);
                        //    Canvas.SetLeft(img, obj.Vhc.Position.X - (img.Width) / 2);
                        //}
                    }
                });
            }
        }

        public Image AddObstacle(Obstacle obs)
        {
            Image img = new Image();

            img.Source = new BitmapImage(new Uri(IMG));     //Uri do zdjęcia; Source ma typ ImageSource

            //img.RenderTransformOrigin = new System.Windows.Point(0, 0);
            //img.Margin = new Thickness(obs.Position.X, obs.Position.Y, 0, 0);
            img.Height = obs.Length;
            img.Width = obs.Width;
            //img.HorizontalAlignment = HorizontalAlignment.Left;
            //img.VerticalAlignment = VerticalAlignment.Bottom;

            MapCanvas.Children.Add(img);

            Canvas.SetBottom(img, obs.Position.Y - (img.Height) / 2);
            Canvas.SetLeft(img, obs.Position.X - (img.Width) / 2);

            return img;
        }

        public Image AddVehicle(Vehicle vhc)
        {
            Image img = new Image();

            img.Source = new BitmapImage(new Uri(IMG));

            //img.RenderTransformOrigin = new System.Windows.Point(0.5,0.5);
            //img.Margin = new Thickness(vhc.Position.X, 0, 0, vhc.Position.Y);
            img.Height = WindowConstanst.VehilceImageSize;
            img.Width = WindowConstanst.VehilceImageSize;
            //img.HorizontalAlignment = HorizontalAlignment.Left;
            //img.VerticalAlignment = VerticalAlignment.Bottom;

            MapCanvas.Children.Add(img);

            Canvas.SetBottom(img, vhc.Position.Y - (img.Height)/2);
            Canvas.SetLeft(img, vhc.Position.X - (img.Width)/2);

            return img;
        }

        public List<Line> AddLines(Vehicle vhc)
        {
            List<Line> lines = new List<Line>();
            foreach (Segment path in vhc.Routes)
            {
                Line line = new Line();

                line.Margin = new Thickness(0, 0, 0, 0);
                //line.HorizontalAlignment = HorizontalAlignment.Left;
                //line.VerticalAlignment = VerticalAlignment.Bottom;
                line.StrokeThickness = 1;
                line.Stroke = System.Windows.Media.Brushes.Gray;
                line.X1 = path.Begin.X;
                line.X2 = path.End.X;
                line.Y1 = Constants.mapSizeY - path.Begin.Y;
                line.Y2 = Constants.mapSizeY - path.End.Y;

                MapCanvas.Children.Add(line);

                lines.Add(line);
            }

            return lines;
        }

        public Border AddBorder(Vehicle vhc)
        {
            Border border = new Border();
            border.BorderBrush = System.Windows.Media.Brushes.Green;
            border.BorderThickness = new Thickness(2, 2, 2, 2);
            border.Height = WindowConstanst.VehilceImageSize + 4;
            border.Width = WindowConstanst.VehilceImageSize + 4;
            border.Visibility = Visibility.Hidden;

            MapCanvas.Children.Add(border);

            Canvas.SetBottom(border, vhc.Position.Y - WindowConstanst.VehilceImageSize / 2 - 2);
            Canvas.SetLeft(border, vhc.Position.X - WindowConstanst.VehilceImageSize / 2 - 2);

            return border;
        }

        private void VehicleList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (VehicleList.SelectedIndex == -1) return;

            VehicleObject vhc = VehicleList.SelectedItems[0] as VehicleObject;
            if (VehicleList.SelectedItems.Count > 1)
            {
                vhc.Brdr.Visibility = Visibility.Hidden;
                VehicleList.SelectedItems.Remove(vhc);
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    vhc.Brdr.Visibility = Visibility.Visible;
                });
            }
            //VehicleList.UnselectAll();
        }
    }

    public class VehicleObject   //każdy pojazd ma swój odpowiednik na mapie
    {
        private List<Line> lines;
        private Image img;
        private Border brdr;
        private Vehicle vhc;
        private string vehicleType;
        private int currentLineIndex = 0;

        public Border Brdr
        {
            get { return brdr; }
            set { brdr = value; }
        }
        public int CurrentLineIndex
        {
            get { return currentLineIndex; }
            set { currentLineIndex = value; }
        }
        public List<Line> Lines
        {
            get { return lines; }
            private set
            {
                lines = value;
            }
        }
        public Image Img
        {
            get { return img; }
            private set
            {
                img = value;
            }
        }
        public Vehicle Vhc
        {
            get { return vhc; }
            private set
            {
                vhc = value;
            }
        }
        public string VehicleType
        {
            get { return vehicleType; }
            private set
            {
                vehicleType = value;
            }
        }

        public VehicleObject(Vehicle _vhc, Image _img, List<Line> _lines, Border _brdr)
        {
            vhc = _vhc;
            img = _img;
            lines = _lines;
            vehicleType = _vhc.GetType().Name;
            brdr = _brdr;
        }
    }
}
