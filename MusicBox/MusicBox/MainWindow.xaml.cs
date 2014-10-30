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
        private BackgroundWorker backgroundThread = new BackgroundWorker();
        private Request request = new Request();
        private List<Audio> audio_list = new List<Audio>();
        private List<Audio> my_audio = new List<Audio>();
        private int current_song = 0;
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
                if ((Math.Round(mediaElement.Position.TotalSeconds, 0) % 60) < 10)
                    labelTime.Content = (Math.Round(mediaElement.Position.TotalMinutes)/1).ToString() + ":0" + (Math.Round(mediaElement.Position.TotalSeconds, 0) % 60).ToString();
                else
                    labelTime.Content = ((int)(mediaElement.Position.TotalMinutes)).ToString() + ":" + (Math.Round(mediaElement.Position.TotalSeconds, 0) % 60).ToString();
                if (mediaElement.NaturalDuration == mediaElement.Position)
                {
                    if (AppSettings.RepeatSong)
                    {
                        this.playList_SelectionChanged(null, null);
                    }
                    else
                    {
                        this.btnNext_Click(null, null);
                    }
                }
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
                textboxSearch.IsEnabled = true;
                textboxSearch.Visibility = System.Windows.Visibility.Visible;
                AppSettings.IsFind = false;
            }
            else
            {
                sliProgress.Visibility = System.Windows.Visibility.Visible;
                textboxSearch.IsEnabled = false;
                textboxSearch.Visibility = System.Windows.Visibility.Collapsed;
                if (AppSettings.IsFind)
                {
                    backgroundThread.RunWorkerAsync();
                }
            }
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch(Exception ex)
            {

            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            sliProgress.IsEnabled = true;
            if (!AppSettings.IsPlaying)
            {
                if (audio_list.Count > current_song)
                {
                    if (mediaElement.Source != new Uri(audio_list[current_song].url))
                        mediaElement.Source = new Uri(audio_list[current_song].url);
                    AppSettings.IsPlaying = true;
                    playList.SelectedIndex = current_song;
                    labelSong.Content = audio_list[current_song].artist + ": " + audio_list[current_song].title;
                    mediaElement.Play();
                }
            }
            else
            {
                AppSettings.IsPlaying = false;
                mediaElement.Pause();
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            backgroundThread.RunWorkerAsync();
        }

        public void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (current_song + 1 <= audio_list.Count - 1)
            {
                current_song++;
                playList.SelectedIndex++;
            }
            if (AppSettings.IsPlaying)
            {
                mediaElement.Source = new Uri(audio_list[current_song].url);
                labelSong.Content = audio_list[current_song].artist + ": " + audio_list[current_song].title;
                mediaElement.Play();
            }
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (current_song - 1 >= 0)
            {
                current_song--;
                playList.SelectedIndex--;
            }
            if (AppSettings.IsPlaying)
            {
                mediaElement.Source = new Uri(audio_list[current_song].url);
                labelSong.Content = audio_list[current_song].artist + ": " + audio_list[current_song].title;
                mediaElement.Play();
            }
        }

        private void Element_AudioOpened(object sender, EventArgs e)
        {
            sliProgress.Value = 0;
            sliProgress.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            
        }

        private void playList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sliProgress.IsEnabled = true;
            if (playList.SelectedIndex <= audio_list.Count - 1 && playList.SelectedIndex != -1)
            { 
                current_song = playList.SelectedIndex;
                mediaElement.Source = new Uri(audio_list[current_song].url);
                labelSong.Content = audio_list[current_song].artist + ": " + audio_list[current_song].title;
                AppSettings.IsPlaying = true;
                mediaElement.Play();
                checkAdd(audio_list[current_song]);
            }
        }


        private void textboxSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AppSettings.IsFind = true;
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += backgroundThread_SearchAudio;
                bw.RunWorkerAsync();
            }
        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (playList.SelectedIndex == -1)
                return;
            int i = playList.SelectedIndex;
            if (AppSettings.AddMode)
            {
                request.addToMyAudio(audio_list[i]);
                my_audio.Add(audio_list[i]);
            }
            else
            {
                request.deleteFromMyAudio(audio_list[i]);
                my_audio.Remove(audio_list[i]);
                fillListBox(my_audio);
                audio_list = my_audio;
                mediaElement.Pause();
            }
            checkAdd(audio_list[i]);
        }

        private void btnRepeat_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.RepeatSong)
            {
                AppSettings.RepeatSong = false;
                btnRepeat.Opacity = 0.3;
            }
            else
            {
                AppSettings.RepeatSong = true;
                btnRepeat.Opacity = 1.0;
            }
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.RandomOrder)
            {
                AppSettings.RandomOrder = false;
                btnRandom.Opacity = 1.0;
            }
            else
            {
                AppSettings.RandomOrder = true;
                btnRandom.Opacity = 0.3;
            }
        }

        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.VolumeActive)
            {
                AppSettings.VolumeActive = false;
                btnExit.Visibility = System.Windows.Visibility.Visible;
                sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                AppSettings.VolumeActive = true;
                btnExit.Visibility = System.Windows.Visibility.Collapsed;
                sliderVolume.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.MenuOn)
            {
                AppSettings.MenuOn = false;

                menuStripe.Visibility = System.Windows.Visibility.Collapsed;

                btnRepeat.Visibility = System.Windows.Visibility.Visible;
                btnVolume.Visibility = System.Windows.Visibility.Visible;
                btnRandom.Visibility = System.Windows.Visibility.Visible;
                btnExit.Visibility = System.Windows.Visibility.Visible;

                btnPopular.Visibility = System.Windows.Visibility.Collapsed;
                btnAdvise.Visibility = System.Windows.Visibility.Collapsed;
                btnSearch.Visibility = System.Windows.Visibility.Collapsed;
                btnAdd.Visibility = System.Windows.Visibility.Collapsed;
                sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                AppSettings.MenuOn = true;
                menuStripe.Visibility = System.Windows.Visibility.Visible;

                btnRepeat.Visibility = System.Windows.Visibility.Collapsed;
                btnVolume.Visibility = System.Windows.Visibility.Collapsed;
                btnRandom.Visibility = System.Windows.Visibility.Collapsed;
                btnExit.Visibility = System.Windows.Visibility.Visible;

                btnPopular.Visibility = System.Windows.Visibility.Visible;
                btnAdvise.Visibility = System.Windows.Visibility.Visible;
                btnSearch.Visibility = System.Windows.Visibility.Visible;
                btnAdd.Visibility = System.Windows.Visibility.Visible;
                sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            }
        }


        //BEGIN PRIVATE METHODS

        private void backgroundThread_GetAudio(object sender, DoWorkEventArgs e)
        {
            if (playList.Dispatcher.CheckAccess())
            {
                playList.Items.Clear();
                loadingText.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                playList.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    playList.Items.Clear();
                    loadingText.Visibility = System.Windows.Visibility.Visible;
                }));
            }

            audio_list = request.getMyAudioList();
            my_audio = audio_list;

            if(playList.Dispatcher.CheckAccess())
            {
                loadingText.Visibility = System.Windows.Visibility.Collapsed;
                fillListBox(audio_list);
            }
            else
            {
                playList.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        loadingText.Visibility = System.Windows.Visibility.Collapsed;
                        fillListBox(audio_list);
                    }));
                }
        }

        private void backgroundThread_SearchAudio(object sender, DoWorkEventArgs e)
        {
            String search = "";
            if (playList.Dispatcher.CheckAccess())
            {
                search = textboxSearch.Text;
                playList.Items.Clear();
                loadingText.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                playList.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    search = textboxSearch.Text;
                    playList.Items.Clear();
                    loadingText.Visibility = System.Windows.Visibility.Visible;
                }));
            }

            audio_list = request.searchAudio(search);

            if (playList.Dispatcher.CheckAccess())
            {
                loadingText.Visibility = System.Windows.Visibility.Collapsed;
                fillListBox(audio_list);
            }
            else
            {
                playList.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    loadingText.Visibility = System.Windows.Visibility.Collapsed;
                    fillListBox(audio_list);
                }));
            }
        }



        private void fillListBox(List<Audio> list) 
        {
            playList.Items.Clear();
            for (int i = 0; i < audio_list.Count; i++)
            {
                playList.Items.Add(list[i].title);
            }
        }

        private void checkAdd(Audio song)
        {
            if (isInMyAudio(song))
            {
                if (AppSettings.AddMode)
                {
                    addBtnAnimation(0, 45);
                    AppSettings.AddMode = false;
                }

            }
            else
            {
                if (!AppSettings.AddMode)
                {
                    addBtnAnimation(45, 0);
                    AppSettings.AddMode = true;
                }
            }
        }

        private void addBtnAnimation(int from, int to)
        {
            DoubleAnimation da = new DoubleAnimation
                (from, to, new Duration(TimeSpan.FromSeconds(1)));
            RotateTransform rt = new RotateTransform();
            btnAdd.RenderTransform = rt;
            btnAdd.RenderTransformOrigin = new Point(0.5, 0.5);
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
        }

        private bool isInMyAudio(Audio song)
        {
            foreach (Audio a in my_audio)
            {
                if (a.aid == song.aid && a.owner_id == song.owner_id)
                    return true;
            }
            return false;
        }


        //END PRIVATE METHODS

    }
}
