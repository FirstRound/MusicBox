using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;

namespace MusicBox
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker backgroundThread = new BackgroundWorker();
        Request request = new Request();
        List<Audio> audio_list = new List<Audio>();
        int current_song = 0;
        bool is_play = false;
        private bool userIsDraggingSlider = false;

        public MainWindow()
        {
            InitializeComponent();
            labelSong.MouseLeftButtonDown += new MouseButtonEventHandler(Label_MouseLeftButtonDown);
            backgroundThread.DoWork += backgroundThread_GetAudio;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mediaElement.Source != null) && (mediaElement.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mediaElement.Position.TotalSeconds;
            }
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mediaElement.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (textboxSearch.Visibility == System.Windows.Visibility.Collapsed)
            {
                sliProgress.Visibility = System.Windows.Visibility.Collapsed;
                textboxSearch.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                sliProgress.Visibility = System.Windows.Visibility.Visible;
                textboxSearch.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!is_play)
            {
                if (audio_list.Count > current_song)
                {
                    if (mediaElement.Source != new Uri(audio_list[current_song].url))
                        mediaElement.Source = new Uri(audio_list[current_song].url);
                    is_play = true;
                    mediaElement.Play();
                }
            }
            else
            {
                is_play = false;
                mediaElement.Pause();
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            backgroundThread.RunWorkerAsync();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (current_song + 1 <= audio_list.Count - 1)
            {
                current_song++;
            }
            if (is_play)
            {
                mediaElement.Source = new Uri(audio_list[current_song].url);
                mediaElement.Play();
            }
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (current_song - 1 >= 0)
            {
                current_song--;
            }
            if (is_play)
            {
                mediaElement.Source = new Uri(audio_list[current_song].url);
                mediaElement.Play();
            }
        }

        private void Element_AudioOpened(object sender, EventArgs e)
        {
            sliProgress.Value = 0;
            sliProgress.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            
        }


        //BEGIN PRIVATE METHODS

        private void backgroundThread_GetAudio(object sender, DoWorkEventArgs e)
        {
            // Проверка, авторизовался ли пользователь
            while (!Settings.Auth)
            {
                Thread.Sleep(500);
            }
            audio_list = request.getMyAudioList();

            if(playList.Dispatcher.CheckAccess())
            {
                for (int i = 0; i < audio_list.Count; i++)
                {
                    playList.Items.Add(audio_list[i].title);
                }
            }
            else
            {
                playList.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate(){
                    for (int i = 0; i < audio_list.Count; i++)
                    {
                        playList.Items.Add(audio_list[i].title);
                    }
                }));
                }
        }


        //END PRIVATE METHODS

    }
}
