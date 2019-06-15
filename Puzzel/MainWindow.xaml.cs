using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Xml;

namespace Puzzel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int size = 100;
        log log = null;

        public MainWindow()
        {
            InitializeComponent();
            log = new log();



        }

        #region #Game functions



        bool pointIsValied(Point point)
        {//check is position for change button location is valied
            return (point.X < puzzelItem.ActualWidth) && (point.Y < puzzelItem.ActualHeight) && (point.X >= 0) && (point.Y >= 0);
        }

        public IEnumerable<Point> CheckPoints(Point point, Button button)
        {//get location around button
            yield return new Point(point.X - button.Width, point.Y);//Left

            yield return new Point(point.X, point.Y + button.Height);//Check Top

            yield return new Point(point.X + button.Width, point.Y);//Check Right

            yield return new Point(point.X, point.Y - button.Height);//Check Bottom
        }

        bool searchButton(Point point)
        {
            //check list of buttons if exist a button with 
            //sended location --> if have a botton we must send true because 
            //we cant change button location to this #point

            bool isValied = false;
            foreach (Button button in buttonsList)
            {
                Point btnLocation = new Point(button.Margin.Left, button.Margin.Top);
                if (btnLocation == point)
                {
                    isValied = true;
                    break;
                }
            }
            return isValied;
        }


        ObservableCollection<Button> buttonsList = new ObservableCollection<Button>();
        private async void GenerateNewButton_Click(object sender, RoutedEventArgs e)
        {
            buttonsList.Clear();
            puzzelItem.ItemsSource = null;
            size = int.Parse(puzzelItem.ActualWidth / double.Parse(txtNumber.Text) + "");

            //buttonsList.CollectionChanged += ButtonsList_CollectionChanged;

            buttonsList.Add(await createButton(null));
            for (int i = 1; i < Math.Pow(int.Parse(txtNumber.Text), 2) - 1; i++)
            {
                if (buttonsList.Last().Margin.Left + buttonsList.Last().Width > puzzelItem.ActualWidth)
                {
                    buttonsList.Last().Margin = new Thickness(0, buttonsList.Last().Margin.Top + size, buttonsList.Last().Margin.Right, buttonsList.Last().Margin.Bottom);
                }

                buttonsList.Add(await createButton(buttonsList.Last()));

                showLog();
            }


            buttonsList = ShuffleList(buttonsList);

            puzzelItem.ItemsSource = buttonsList;
            puzzelItem.Items.Refresh();
            progress.Visibility = Visibility.Collapsed ;

        }

        public async Task<Button> createButton(Button button)
        {
            Func<Button> function = new Func<Button>(() => AddButton(button));
            Button res = await Task.Run<Button>(function);
            return res;
        }

        Button AddButton(Button lastButton)
        {
            Button button = null;

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                progress.Value = buttonsList.Count * 100 / Math.Pow(int.Parse(txtNumber.Text), 2) - 1;

                button = new Button();
                button.Width = size;
                button.Height = size;
                button.HorizontalAlignment = HorizontalAlignment.Left;
                button.VerticalAlignment = VerticalAlignment.Top;
                button.Content = buttonsList.Count + 1;


                if (lastButton == null)
                {
                    button.Margin = new Thickness(0);

                }
                else
                {
                    button.Margin = new Thickness(lastButton.Margin.Left + size, lastButton.Margin.Top, 0, 0);
                }

                button.Click += Button_Click;

            });

            return button;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            Point btnLocation = new Point(button.Margin.Left, button.Margin.Top);

            Button findedButton = buttonsList.Single(x => x.Margin == button.Margin);


            int thisButtonIndex = buttonsList.IndexOf(button);

            int orderedButtonIndex = buttonsList.IndexOf(findedButton);



            foreach (var item in CheckPoints(btnLocation, button))
            {

                if (pointIsValied(item) && !searchButton(new Point(item.X, item.Y)))
                {//if not exist a button to this location we change this button location

                    ThicknessAnimation da = new ThicknessAnimation();
                    da.From = buttonsList.Single(x => x.Margin == button.Margin).Margin;
                    da.To = new Thickness(item.X, item.Y, 0, 0);
                    da.Duration = new Duration(TimeSpan.FromSeconds(0.3));
                    da.DecelerationRatio = 0.9;
                    button.BeginAnimation(MarginProperty, da);
                    button.Margin = new Thickness(item.X, item.Y, 0, 0);

                }
            }
            showLog();

        }

        private ObservableCollection<E> ShuffleList<E>(ObservableCollection<E> inputList)
        {
            ObservableCollection<E> randomList = new ObservableCollection<E>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }



        #endregion


        void showLog()
        {
            log.txtLog.Text = "";
            foreach (var item in buttonsList)
            {
                log.txtLog.Text += "[" + item.Margin + "]" + "\n";
            }
        }

        #region #file functions
        private void BtnLoadfromCache_Click(object sender, RoutedEventArgs e)
        {
            ObjectCache cache = MemoryCache.Default;
            string fileContents = "";
            fileContents = cache["filecontents"] as string;

            if (fileContents == null)
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(10.0);

                List<string> filePaths = new List<string>();
                filePaths.Add("e:\\cache.txt");

                policy.ChangeMonitors.Add(new
                 HostFileChangeMonitor(filePaths));

                fileContents = File.ReadAllText("e:\\cache.txt");

                cache.Set("filecontents", fileContents, policy);
            }


            ///save and Load A list of buttons

            var json = JsonConvert.DeserializeObject<Button>(fileContents);

            XmlReader xr = XmlReader.Create(input: new StringReader(fileContents));
            var control = XamlReader.Load(xr) as Grid;
            this.grid.Children.Add(control);


        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var json = JsonConvert.SerializeObject(buttonsList);
            File.WriteAllText(@"e:\\cache.txt", json);


        }
        #endregion


        private void btnClickMe_Click(object sender, RoutedEventArgs e)
        {
            AddButton(null);
        }

       
    }
}
