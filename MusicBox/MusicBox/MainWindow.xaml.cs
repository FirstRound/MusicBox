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
using System.Net;

namespace MusicBox
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker _backgroundThread = new BackgroundWorker();
        private Request _request;
        //private List<Audio> _audio_list = new List<Audio>();
        //private List<Audio> _my_audio = new List<Audio>();
        private List<int> _order = new List<int>();
       // private int _current_song = 0;
        private bool _user_is_dragging_slider = false;
        private ContextMenu _tray_menu = null;
        private System.Windows.Forms.NotifyIcon _tray_icon = null;

        //
        private AudioController _audio_controller;
        private WindowState fCurrentWindowState = WindowState.Normal;
        //

        private bool fCanClose = false;

        public MainWindow(VkSettings vk)
        {
            InitializeComponent();

            _request = new Request(vk);
            _audio_controller = new AudioController(vk);

            labelSong.MouseLeftButtonDown += new MouseButtonEventHandler(Label_MouseLeftButtonDown);
            _backgroundThread.DoWork += backgroundThread_GetAudio;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            createTrayIcon();
        }

        private bool createTrayIcon()
        {
            bool result = false;
            if (_tray_icon == null)
            {
                _tray_icon = new System.Windows.Forms.NotifyIcon();
                _tray_icon.Icon = MusicBox.Properties.Resources.favicon;
                _tray_icon.Text = "MusicBox";
                _tray_menu = Resources["TrayMenu"] as ContextMenu;

                _tray_icon.Click += delegate(object sender, EventArgs e)
                {
                    if ((e as System.Windows.Forms.MouseEventArgs).Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        ShowHideMainWindow(sender, null);
                    }
                    else
                    {
                        _tray_menu.IsOpen = true;
                        Activate();
                    }
                };
                result = true;
            }
            else
            {
                result = true;
            }
            _tray_icon.Visible = true;
            return result;
        }

        private void ShowHideMainWindow(object sender, RoutedEventArgs e)
        {
            _tray_menu.IsOpen = false;
            if (IsVisible)
            {
                Hide();
                (_tray_menu.Items[0] as MenuItem).Header = "Развернуть";
            }
            else
            {
                Show();
                (_tray_menu.Items[0] as MenuItem).Header = "Свернуть";
                WindowState = CurrentWindowState;
                Activate();
            }
        }

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
                (_tray_menu.Items[0] as MenuItem).Header = "Развернуть";
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
                (_tray_menu.Items[0] as MenuItem).Header = "Развернуть";
                Hide();
            }
            else
            {
                _tray_icon.Visible = false;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mediaElement.Source != null) && (mediaElement.NaturalDuration.HasTimeSpan) && (!_user_is_dragging_slider))
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
                    if (AppSettings.sRepeatSong)
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
            _user_is_dragging_slider = true;

            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.sVolumeActive = false;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _user_is_dragging_slider = false;
            mediaElement.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!AppSettings.sIsDownloading)
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
                AppSettings.sIsFind = false;

            }
            else
            {
                sliProgress.Visibility = System.Windows.Visibility.Visible;
                textboxSearch.IsEnabled = false;
                textboxSearch.Visibility = System.Windows.Visibility.Collapsed;
                if (AppSettings.sIsFind)
                {
                    _backgroundThread.RunWorkerAsync();
                }
            }
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.sVolumeActive = false;

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
            AppSettings.sVolumeActive = false;
            sliProgress.IsEnabled = true;
            labelSong.Content = _audio_controller.Play(ref mediaElement);
            playList.SelectedIndex = _audio_controller.CurrentSong; // второй раз запускается плеер - проблема
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            _backgroundThread.RunWorkerAsync();
        }

        public void btnNext_Click(object sender, RoutedEventArgs e)
        {
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.sVolumeActive = false;
            Audio next = _audio_controller.GetNextSong();
            labelSong.Content = next.Description;
            playList.SelectedIndex = _audio_controller.CurrentSong;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.sVolumeActive = false;
            Audio prev = _audio_controller.GetPrevSong();
            labelSong.Content = prev.Description;
            playList.SelectedIndex = _audio_controller.CurrentSong;
        }

        private void Element_AudioOpened(object sender, EventArgs e)
        {
            sliProgress.Value = 0;
            sliProgress.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;

        }

        private void playList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sliProgress.IsEnabled = true;
            _audio_controller.CurrentSong = playList.SelectedIndex;
            if (sender != null && playList.SelectedIndex <= _audio_controller.CurrentAudioList.Count - 1 && playList.SelectedIndex != -1)
            {
                labelSong.Content = _audio_controller.SelectionChanged(ref mediaElement);
                AppSettings.sIsPlaying = true;
                mediaElement.Play();
                checkAdd(_audio_controller.CurrentAudio);
            }

            playList.ScrollIntoView(playList.SelectedItem);
        }


        private void textboxSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AppSettings.sIsFind = true;
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

            _audio_controller.Add(i);

            if (!_audio_controller.Settings.AddMode)
            {
                fillListBox(_audio_controller.MyAudioList);
                _audio_controller.ChangeToMyAudioList();
                mediaElement.Pause();
            }

            checkAdd(_audio_controller.CurrentAudioList[i]);
        }

        private void btnRepeat_Click(object sender, RoutedEventArgs e)
        {
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.sVolumeActive = false;

            if (AppSettings.sRepeatSong)
            {
                AppSettings.sRepeatSong = false;
                btnRepeat.Opacity = 0.3;
            }
            else
            {
                AppSettings.sRepeatSong = true;
                btnRepeat.Opacity = 1.0;
            }
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.sVolumeActive = false;

            bool isRandom = _audio_controller.Shuffle();

            if (isRandom)
            {
                AppSettings.sRandomOrder = false;
                btnRandom.Opacity = 0.3;
                fillListBox(_audio_controller.CurrentAudioList);
            }
            else
            {
                AppSettings.sRandomOrder = true;
                btnRandom.Opacity = 1.0;
                fillListBox(_audio_controller.CurrentAudioList);
            }
        }

        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            _audio_controller.Settings.VolumeActive = !(_audio_controller.Settings.VolumeActive);
            ChangeVisibility(btnMinimize);
            ChangeVisibility(sliderVolume);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            sliderVolume.Visibility = System.Windows.Visibility.Collapsed;
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            AppSettings.sVolumeActive = false;
        }

        private void btnAdvise_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.sAdviseMode)
            {
                AppSettings.sAdviseMode = false;
                btnAdvise.Opacity = 0.3;
                _backgroundThread.RunWorkerAsync();
            }
            else
            {
                AppSettings.sPopularMode = false;
                btnPopular.Opacity = 0.3;

                AppSettings.sAdviseMode = true;
                btnAdvise.Opacity = 1.0;
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += backgroundThread_GetAdviseAudio;
                bw.RunWorkerAsync();
            }
        }


        private void btnPopular_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.sPopularMode)
            {
                AppSettings.sPopularMode = false;
                btnPopular.Opacity = 0.3;
                _backgroundThread.RunWorkerAsync();
            }
            else
            {
                AppSettings.sAdviseMode = false;
                btnAdvise.Opacity = 0.3;

                AppSettings.sPopularMode = true;
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
            _audio_controller.Settings.MenuOn = !(_audio_controller.Settings.MenuOn);
            ChangeVisibility(menuStripe);

            ChangeVisibility(btnRepeat);
            ChangeVisibility(btnVolume);
            ChangeVisibility(btnRandom);
            ChangeVisibility(btnMinimize);

            ChangeVisibility(btnPopular);
            ChangeVisibility(btnAdvise);
            ChangeVisibility(btnSearch);
            ChangeVisibility(btnAdd);
            ChangeVisibility(btnDownload);
            ChangeVisibility(btnExit);
        }


        //BEGIN PRIVATE METHODS

        private void backgroundThread_GetAudio(object sender, DoWorkEventArgs e)
        {
            clearPlayList();
            _audio_controller.GetMyAudio();
            fillPlayList(_audio_controller.CurrentAudioList);
        }

        private void backgroundThread_SearchAudio(object sender, DoWorkEventArgs e)
        {
            String search = "";
            var del = new Action(delegate()
                {
                    search = textboxSearch.Text;
                    playList.Items.Clear();
                    loadingText.Visibility = System.Windows.Visibility.Visible;
                });

            if (playList.Dispatcher.CheckAccess())
            {
                del.Invoke();
            }
            else
            {
                playList.Dispatcher.Invoke(DispatcherPriority.Normal, del);
            }

            _audio_controller.SearchAudio(search);
            fillPlayList(_audio_controller.CurrentAudioList);
        }

        private void backgroundThread_GetPopularAudio(object sender, DoWorkEventArgs e)
        {
            clearPlayList();
            _audio_controller.GetPopularAudio();
            fillPlayList(_audio_controller.CurrentAudioList);
        }

        private void backgroundThread_GetAdviseAudio(object sender, DoWorkEventArgs e)
        {
            clearPlayList();
            _audio_controller.GetAdviceAudio();
            fillPlayList(_audio_controller.CurrentAudioList);
        }


        private void clearPlayList()
        {
            var del = new Action(delegate()
                {
                    playList.Items.Clear();
                    loadingText.Visibility = System.Windows.Visibility.Visible;
                });

            if (playList.Dispatcher.CheckAccess())
            {
                del.Invoke();
            }
            else
            {
                playList.Dispatcher.Invoke(DispatcherPriority.Normal, del);
            }
        }

        private void fillPlayList(List<Audio> list)
        {
            var del = new Action(delegate()
                {
                    loadingText.Visibility = System.Windows.Visibility.Collapsed;
                    fillListBox(list);
                });

            if (playList.Dispatcher.CheckAccess())
            {
                del.Invoke();
            }
            else
            {
                playList.Dispatcher.Invoke(DispatcherPriority.Normal, del);
            }
        }



        private void backgroundThread_DownloadAudio(object sender, DoWorkEventArgs e)
        {
            /*
            try
            {
                String name = "MusicBoxVKSong";
                Uri source = new Uri(_audio_list[0].Url);
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
                    name = _audio_list[_current_song].Artist + ": " + _audio_list[_current_song].Title;
                }
                else
                {
                    playList.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        name = _audio_list[_current_song].Artist + ": " + _audio_list[_current_song].Title;
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
                    AppSettings.sIsDownloading = true;
                    Client.DownloadFile(source, path);
                    AppSettings.sIsDownloading = false;
                }
            }
            catch (Exception ex)
            {
                //do smth
            }
             */
        }

        private void fillListBox(List<Audio> list)
        {
            playList.Items.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                playList.Items.Add(list[i].Title);
            }
        }

        private void checkAdd(Audio song)
        {
            if (isInMyAudio(song))
            {
                if (_audio_controller.Settings.AddMode)
                {
                    addBtnAnimation(0, 45);
                    _audio_controller.Settings.AddMode = false;

                }

            }
            else
            {
                if (!_audio_controller.Settings.AddMode)
                {
                    addBtnAnimation(45, 0);
                    _audio_controller.Settings.AddMode = true;
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
            foreach (Audio a in _audio_controller.MyAudioList)
            {
                if (a.AudioID == song.AudioID && a.OwnerID == song.OwnerID)
                    return true;
            }
            return false;
        }

        private void shuffleAudioList(List<Audio> audio_list)
        {
            _order.Clear();
            for (int i = 0; i < audio_list.Count; i++)
            {
                _order.Add(i);
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

                int s = _order[k];
                _order[k] = _order[n];
                _order[n] = s;

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

        private void ChangeVisibility(Control elem)
        {
            if (elem.Visibility == System.Windows.Visibility.Collapsed)
            {
                elem.Visibility = System.Windows.Visibility.Visible;
            }
            else 
            {
                elem.Visibility = System.Windows.Visibility.Collapsed;
            }
             
        }

        //END PRIVATE METHODS

    }
}
