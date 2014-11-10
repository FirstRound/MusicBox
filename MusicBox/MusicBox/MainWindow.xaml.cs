using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
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
using System.Net;

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
        private List<int> order = new List<int>();
        private int current_song = 0;
        private bool userIsDraggingSlider = false;
        private ContextMenu TrayMenu = null;
        private System.Windows.Forms.NotifyIcon TrayIcon = null;

        private bool fCanClose = false;

        public MainWindow()
        {
            InitializeComponent();

            labelSong.MouseLeftButtonDown += new MouseButtonEventHandler(Label_MouseLeftButtonDown);
            backgroundThread.DoWork += backgroundThread_GetAudio;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += timer_Tick;
            bw.RunWorkerAsync();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            createTrayIcon();
        }

        private bool createTrayIcon()
        {
            bool result = false;
            if (TrayIcon == null)
            {
                TrayIcon = new System.Windows.Forms.NotifyIcon();
                TrayIcon.Icon = MusicBox.Properties.Resources.favicon;
                TrayIcon.Text = "MusicBox";
                TrayMenu = Resources["TrayMenu"] as ContextMenu;

                TrayIcon.Click += delegate(object sender, EventArgs e)
                {
                    if ((e as System.Windows.Forms.MouseEventArgs).Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        ShowHideMainWindow(sender, null);
                    }
                    else
                    {
                        TrayMenu.IsOpen = true;
                        Activate();
                    }
                };
                result = true;
            }
            else
            {
                result = true;
            }
            TrayIcon.Visible = true;
            return result;
        }

        private void ShowHideMainWindow(object sender, RoutedEventArgs e)
        {
            TrayMenu.IsOpen = false;
            if (IsVisible)
            {
                Hide();
                (TrayMenu.Items[0] as MenuItem).Header = "Развернуть";
            }
            else
            {
                Show();
                (TrayMenu.Items[0] as MenuItem).Header = "Свернуть";
                WindowState = CurrentWindowState;
                Activate();
            }
        }

        private WindowState fCurrentWindowState = WindowState.Normal;
        public WindowState CurrentWindowState
        {
            get { return fCurrentWindowState; }
            set { fCurrentWindowState = value; }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (this.WindowState == System.Windows.WindowState.Minimized)
            {
                Hide();
                (TrayMenu.Items[0] as MenuItem).Header = "Развернуть";
            }
            else
            {
                CurrentWindowState = WindowState;
            }
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            CanClose = true;
            Button_Click(null, null);
            Close();
        }
        public bool CanClose
        {
            get { return fCanClose; }
            set { fCanClose = value; }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!CanClose)
            {    
                e.Cancel = true;
                CurrentWindowState = this.WindowState;
                (TrayMenu.Items[0] as MenuItem).Header = "Развернуть";
                Hide();
            }
            else
            {
                TrayIcon.Visible = false;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mediaElement.Source != null) && (mediaElement.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mediaElement.Position.TotalSeconds;
                if ((Math.Round(mediaElement.Position.TotalSeconds, 0) % 60) < 10)
                    labelTime.Content = (Math.Round(mediaElement.Position.TotalMinutes) / 1).ToString() + ":0" + (Math.Round(mediaElement.Position.TotalSeconds, 0) % 60).ToString();
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

            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.VolumeActive = false;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mediaElement.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!AppSettings.IsDownloading)
                Application.Current.Shutdown();
            else
            {
                String caption = "Идет загрузка...";
                String message = "Файл еще не загружен. Вы уверенны, что хотите выйти?";
                var result = MessageBox.Show(message, caption,
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }

            }
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
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.VolumeActive = false;

            try
            {
                this.DragMove();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.VolumeActive = false;

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
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.VolumeActive = false;

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
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.VolumeActive = false;

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

            playList.ScrollIntoView(playList.SelectedItem);
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
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.VolumeActive = false;

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
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.VolumeActive = false;

            if (AppSettings.RandomOrder)
            {
                AppSettings.RandomOrder = false;
                btnRandom.Opacity = 0.3;
                audio_list = reorderAudioList(audio_list, order);
                fillListBox(audio_list);
            }
            else
            {
                AppSettings.RandomOrder = true;
                btnRandom.Opacity = 1.0;
                shuffleAudioList(audio_list);
                fillListBox(audio_list);
            }
        }

        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.VolumeActive)
            {
                AppSettings.VolumeActive = false;
                btnMinimize.Visibility = System.Windows.Visibility.Visible;
                sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                AppSettings.VolumeActive = true;
                btnMinimize.Visibility = System.Windows.Visibility.Collapsed;
                sliderVolume.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.VolumeActive = false;
        }

        private void btnAdvise_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.AdviseMode)
            {
                AppSettings.AdviseMode = false;
                btnAdvise.Opacity = 0.3;
                backgroundThread.RunWorkerAsync();
            }
            else
            {
                AppSettings.PopularMode = false;
                btnPopular.Opacity = 0.3;

                AppSettings.AdviseMode = true;
                btnAdvise.Opacity = 1.0;
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += backgroundThread_GetAdviseAudio;
                bw.RunWorkerAsync();
            }
        }


        private void btnPopular_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.PopularMode)
            {
                AppSettings.PopularMode = false;
                btnPopular.Opacity = 0.3;
                backgroundThread.RunWorkerAsync();
            }
            else
            {
                AppSettings.AdviseMode = false;
                btnAdvise.Opacity = 0.3;

                AppSettings.PopularMode = true;
                btnPopular.Opacity = 1.0;
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += backgroundThread_GetPopularAudio;
                bw.RunWorkerAsync();
            }
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += backgroundThread_DownloadAudio;
            bw.RunWorkerAsync();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.WindowState = WindowState.Minimized;
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
                btnMinimize.Visibility = System.Windows.Visibility.Visible;

                btnPopular.Visibility = System.Windows.Visibility.Collapsed;
                btnAdvise.Visibility = System.Windows.Visibility.Collapsed;
                btnSearch.Visibility = System.Windows.Visibility.Collapsed;
                btnAdd.Visibility = System.Windows.Visibility.Collapsed;
                sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
                btnDownload.Visibility = System.Windows.Visibility.Collapsed;
                btnExit.Visibility = System.Windows.Visibility.Collapsed;

                sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                AppSettings.MenuOn = true;
                menuStripe.Visibility = System.Windows.Visibility.Visible;

                btnRepeat.Visibility = System.Windows.Visibility.Collapsed;
                btnVolume.Visibility = System.Windows.Visibility.Collapsed;
                btnRandom.Visibility = System.Windows.Visibility.Collapsed;
                btnMinimize.Visibility = System.Windows.Visibility.Collapsed;

                btnPopular.Visibility = System.Windows.Visibility.Visible;
                btnAdvise.Visibility = System.Windows.Visibility.Visible;
                btnSearch.Visibility = System.Windows.Visibility.Visible;
                btnAdd.Visibility = System.Windows.Visibility.Visible;
                btnDownload.Visibility = System.Windows.Visibility.Visible;
                btnExit.Visibility = System.Windows.Visibility.Visible;

                sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            }
        }


        //BEGIN PRIVATE METHODS

        private void backgroundThread_GetAudio(object sender, DoWorkEventArgs e)
        {
            clearPlayList();

            audio_list = request.getMyAudioList();
            my_audio = audio_list;
            setOrder(audio_list, order);

            fillPlayList(audio_list);
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

            fillPlayList(audio_list);
        }

        private void backgroundThread_GetPopularAudio(object sender, DoWorkEventArgs e)
        {
            clearPlayList();

            audio_list = request.getPopularAudioList();

            fillPlayList(audio_list);
        }

        private void backgroundThread_GetAdviseAudio(object sender, DoWorkEventArgs e)
        {
            clearPlayList();

            audio_list = request.getAdviseAudioList();

            fillPlayList(audio_list);
        }


        private void clearPlayList()
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
        }

        private void fillPlayList(List<Audio> list)
        {
            if (playList.Dispatcher.CheckAccess())
            {
                loadingText.Visibility = System.Windows.Visibility.Collapsed;
                fillListBox(list);
            }
            else
            {
                playList.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    loadingText.Visibility = System.Windows.Visibility.Collapsed;
                    fillListBox(list);
                }));
            }
        }



        private void backgroundThread_DownloadAudio(object sender, DoWorkEventArgs e)
        {
            try
            {
                String name = "MusicBoxVKSong";
                Uri source = new Uri(audio_list[0].url);
                if (mediaElement.Dispatcher.CheckAccess())
                {
                    source = mediaElement.Source;
                }
                else
                {
                    mediaElement.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        source = mediaElement.Source;
                    }));
                }

                if (playList.Dispatcher.CheckAccess())
                {
                    name = audio_list[current_song].artist + ": " + audio_list[current_song].title;
                }
                else
                {
                    playList.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        name = audio_list[current_song].artist + ": " + audio_list[current_song].title;
                    }));
                }

                String path = "";

                SaveFileDialog saveFileDialog = new SaveFileDialog();

                saveFileDialog.Filter = "Музыка (*.mp3)|*.mp3";
                saveFileDialog.DefaultExt = ".mp3";
                saveFileDialog.FileName = name;

                if (saveFileDialog.ShowDialog() == true)
                {
                    WebClient Client = new WebClient();
                    path = saveFileDialog.FileName;
                    AppSettings.IsDownloading = true;
                    Client.DownloadFile(source, path);
                    AppSettings.IsDownloading = false;
                }
            }
            catch (Exception ex)
            {
                //do smth
            }
        }

        private void fillListBox(List<Audio> list)
        {
            playList.Items.Clear();
            for (int i = 0; i < list.Count; i++)
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

        private void shuffleAudioList(List<Audio> audio_list)
        {
            order.Clear();
            for (int i = 0; i < audio_list.Count; i++)
            {
                order.Add(i);
            }

            Random rng = new Random();
            int n = audio_list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Audio value = audio_list[k];
                audio_list[k] = audio_list[n];
                audio_list[n] = value;

                int s = order[k];
                order[k] = order[n];
                order[n] = s;

            }
        }

        private void setOrder(List<Audio> audio_list, List<int> ord)
        {
            //
        }

        private List<Audio> reorderAudioList(List<Audio> audio_list, List<int> order)
        {
            List<Audio> right_ordered = new List<Audio>();

            for (int i = 0; i < audio_list.Count; i++)
            {
                right_ordered.Add(new Audio());
            }

            for (int i = 0; i < order.Count; i++)
            {
                right_ordered[order[i]] = audio_list[i];
            }
            return right_ordered;
        }

        //END PRIVATE METHODS

    }
}
