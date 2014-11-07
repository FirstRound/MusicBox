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

        private int IsAuth = 5;
        private String request = "https://oauth.vk.com/authorize?client_id=4609319&scope=audio&redirect_uri=https://oauth.vk.com/blank.html&display=popup&v=5.25&response_type=token";
        public Auth()
        {
            InitializeComponent();
        }

        private void WebBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            webBrowser.Navigate(new Uri(request));
        }

        private void webBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {

        }

        private void webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                while (IsAuth > 0)
                {   
                    String url = webBrowser.Source.ToString();
                    if (url[20] == 'e' || url == request)
                    {
                        WebBrowser_Loaded(null, null);
                        IsAuth--;
                        continue;
                    }
                    String info = url.Split('#')[1];
                    if (info[0] == 'a')
                    {
                        IsAuth = -1;
                        VkSettings.Token = info.Split('&')[0].Split('=')[1];
                        VkSettings.Id = info.Split('=')[3];
                        VkSettings.Auth = true;
                        MainWindow mw = new MainWindow();
                        this.Hide();
                        mw.Show();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (IsAuth == 0)
            {
                MessageBox.Show("Ошибки при подключении к интернету");
            }  
        }
    }
}
