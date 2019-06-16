using Puzzel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Puzzel.ViewModel
{
    class ViewModel
    {
        //size of a puzzel button
        int size = 0;

        private string rowCount;

        public string RowCount
        {
            get
            {
                int value;
                if (!(rowCount.Trim().Length > 0 && int.TryParse(rowCount, out value)))
                {
                    rowCount = "3";
                }
                return rowCount;
            }
            set
            {
                if (value != rowCount)
                {
                    rowCount = value;
                    OnPropertyChanged(nameof(RowCount));
                }

            }
        }

        //list of  puzzel button that fill for show in Grid
        private ObservableCollection<Button> _buttonList = new ObservableCollection<Button>();

        public ObservableCollection<Button> ButtonList
        {
            get
            {
                return _buttonList;
            }
            set
            {

                _buttonList = value;
                OnPropertyChanged(nameof(ButtonList));

            }
        }

        // نمایش یا عدم نمایش مودال
        public ICommand GeneratePuzzels { get { return new RelayCommand(generatePuzzels); } }
        public async void generatePuzzels(object obj)
        {
            if (ButtonList != null && ButtonList.Count > 0)
            {
                ButtonList.Clear();
                randomed.Clear();
            }
            size = int.Parse(600 / double.Parse(RowCount) + "");

            //ButtonList.CollectionChanged += ButtonsList_CollectionChanged;

            ButtonList.Add(await createButton(null));
            for (int i = 1; i < Math.Pow(int.Parse(RowCount), 2) - 1; i++)
            {
                if (ButtonList.Last().Margin.Left + ButtonList.Last().Width > 600)
                {


                    ButtonList.Last().Margin = new Thickness(0, ButtonList.Last().Margin.Top + size, ButtonList.Last().Margin.Right, ButtonList.Last().Margin.Bottom);
                }

                ButtonList.Add(await createButton(ButtonList.Last()));
            }



        }


        List<int> randomed = new List<int>();
        Random randomGen = new Random();
        int random(int x)
        {

            int generatedRandom = randomGen.Next(0, x);
            if (!randomed.Contains(generatedRandom))
            {
                randomed.Add(generatedRandom);
            }
            else
            {
                generatedRandom = random(x);
            }
            return generatedRandom;

        }








        #region #Game functions

        bool pointIsValied(Point point)
        {//check is position for change button location is valied
            return (point.X < 600) && (point.Y < 600) && (point.X >= 0) && (point.Y >= 0);
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
            foreach (Button button in ButtonList)
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

                button = new Button();
                button.Width = size;
                button.Height = size;
                button.HorizontalAlignment = HorizontalAlignment.Left;
                button.VerticalAlignment = VerticalAlignment.Top;
                //button.Content = random(int.Parse(RowCount) * int.Parse(RowCount));
                button.Content = ButtonList.Count + 1;
                if (lastButton == null)
                {
                    button.Margin = new Thickness(0);

                  

                }
                else
                {
                    changePosition(
                        new Thickness(0, 0, 0, 0),
                        new Thickness(lastButton.Margin.Left + size, lastButton.Margin.Top, 0, 0),
                        button
                        );

                    button.Margin = new Thickness(lastButton.Margin.Left + size, lastButton.Margin.Top, 0, 0);



                }

                button.Click += Button_Click;

            });

            return button;
        }



        void changePosition(Thickness thickness1, Thickness thickness2, Button button)
        {
            ThicknessAnimation da = new ThicknessAnimation();
            da.From = thickness1;
            da.To = thickness2;
            da.Duration = new Duration(TimeSpan.FromSeconds(2));
            da.DecelerationRatio = 0.9;
            button.BeginAnimation(FrameworkElement.MarginProperty, da);

        }





        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            Point btnLocation = new Point(button.Margin.Left, button.Margin.Top);

            Button findedButton = ButtonList.Single(x => x.Margin == button.Margin);


            int thisButtonIndex = ButtonList.IndexOf(button);

            int orderedButtonIndex = ButtonList.IndexOf(findedButton);



            foreach (var item in CheckPoints(btnLocation, button))
            {

                if (pointIsValied(item) && !searchButton(new Point(item.X, item.Y)))
                {//if not exist a button to this location we change this button location

                    ThicknessAnimation da = new ThicknessAnimation();
                    da.From = ButtonList.Single(x => x.Margin == button.Margin).Margin;
                    da.To = new Thickness(item.X, item.Y, 0, 0);
                    da.Duration = new Duration(TimeSpan.FromSeconds(0.3));
                    da.DecelerationRatio = 0.9;
                    button.BeginAnimation(FrameworkElement.MarginProperty, da);
                    button.Margin = new Thickness(item.X, item.Y, 0, 0);

                }
            }

        }




        #endregion


        #region INotifyPropertyChanged Methods

        public void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, args);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


    }
}
