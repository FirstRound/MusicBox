using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicBox
{
    /// <summary>
    /// Логика взаимодействия для Auth.xaml
    /// </summary>
    public partial class Auth : Window
    {
        public Auth()
        {
            InitializeComponent();
        }

        private void WebBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            webBrowser.Navigate(new Uri("https://oauth.vk.com/authorize?client_id=4609319&scope=audio&redirect_uri=https://oauth.vk.com/blank.html&display=popup&v=5.25&response_type=token"));
        }

        private void webBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {

        }

        private void webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                String url = webBrowser.Source.ToString();
                String info = url.Split('#')[1];
                if (info[0] == 'a')
                {
                    VkSettings.Token = info.Split('&')[0].Split('=')[1];
                    VkSettings.Id = info.Split('=')[3];
                    VkSettings.Auth = true;
                    MainWindow mw = new MainWindow();
                    this.Hide();
                    mw.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
