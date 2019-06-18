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
            //this.grid.Children.Add(control);


        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            //var json = JsonConvert.SerializeObject(buttonsList);
            //File.WriteAllText(@"e:\\cache.txt", json);


        }
        #endregion


        private void btnClickMe_Click(object sender, RoutedEventArgs e)
        {
            //AddButton(null);
        }

      
    }
}
