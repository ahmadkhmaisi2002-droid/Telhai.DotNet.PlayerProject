using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Telhai.DotNet.PlayerProject.Services;



namespace Telhai.DotNet.PlayerProject
{
    /// <summary>
    /// Interaction logic for MusicPlayer.xaml
    /// </summary>
    public partial class MusicPlayer : Window
    {

        private MediaPlayer mediaPlayer = new MediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
        private List<MusicTrack> library = new List<MusicTrack>();
        private bool isDragging = false;
        private const string FILE_NAME = "library.json";
        private readonly ItunesService _itunes = new ItunesService();
        private CancellationTokenSource? _cts;
        private readonly Dictionary<string, ItunesSong?> _metaCache = new Dictionary<string, ItunesSong?>();



        public MusicPlayer()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(Timer_Tick);

            this.Loaded += MusicPlayer_Loaded;

            // this.MouseDoubleClick += MusicPlayer_MouseDoubleClick;
            // this.MouseDoubleClick += new MouseButtonEventHandler(MusicPlayer_MouseDoubleClick);
            
        }


        private void MusicPlayer_Loaded(object? sender, EventArgs e)
        {
            this.LoadLibrary();
        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Update slider ONLY if music is loaded AND user is NOT holding the handle
            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan && !isDragging)
            {
                sliderProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliderProgress.Value = mediaPlayer.Position.TotalSeconds;
            }
        }


        //private void Timer_Tick(object? sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}


        //private void MusicPlayer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    MainWindow p = new MainWindow();
        //    p.Title = "yyyy";
        //    p.Show();
        // }

        // --- EMPTY PLACEHOLDERS TO MAKE IT BUILD ---
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            timer.Start();
            txtStatus.Text = "Playing";
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e) 
        {
            mediaPlayer.Pause();
            txtStatus.Text = "Paused";
        }
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            sliderProgress.Value = 0;
            txtStatus.Text = "Stopped";
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = sliderVolume.Value;
        }

        private void Slider_DragStarted(object sender, MouseButtonEventArgs e)
        {
            isDragging = true; // Stop timer updates
        }

        private void Slider_DragCompleted(object sender, MouseButtonEventArgs e)
        {
            isDragging = false; // Resume timer updates
            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
        }


        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "MP3 Files|*.mp3";

            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    MusicTrack track = new MusicTrack
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(file),
                        FilePath = file
                    };
                    library.Add(track);
                }
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void UpdateLibraryUI()
        {
            lstLibrary.ItemsSource = null;
            lstLibrary.ItemsSource = library;
        }

        private void SaveLibrary()
        {
            string json = JsonSerializer.Serialize(library);
            File.WriteAllText(FILE_NAME, json);
        }

        private void LoadLibrary()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);
                library = JsonSerializer.Deserialize<List<MusicTrack>>(json) ?? new List<MusicTrack>();
                UpdateLibraryUI();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                library.Remove(track);
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void LstLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                mediaPlayer.Open(new Uri(track.FilePath));
                mediaPlayer.Play();
                timer.Start();
                txtCurrentSong.Text = track.Title;
                txtStatus.Text = "Playing";

                _ = LoadMetadataAsync(track);
            }
        }


        private async Task LoadMetadataAsync(MusicTrack track)
        {
            txtFilePath.Text = track.FilePath;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            string key = track.FilePath;

            try
            {
                txtStatus.Text = "Fetching metadata...";
                txtSongName.Text = "";
                txtArtistName.Text = "";
                txtAlbumName.Text = "";
                imgAlbum.Source = null;

                if (_metaCache.TryGetValue(key, out var cached))
                {
                    ApplyMetadataToUI(track, cached);
                    txtStatus.Text = "Ready";
                    return;
                }

                
                string cleanTitle = track.Title.Replace("_", " ").Replace("-", " ");
                string query = Uri.EscapeDataString(cleanTitle);

                var resp = await _itunes.SearchSongAsync(query, token);
                MessageBox.Show(resp?.ResultCount.ToString() ?? "NULL");

                ItunesSong? song = null;
                if (resp != null && resp.ResultCount > 0 && resp.Results.Length > 0)
                    song = resp.Results[0];

                _metaCache[key] = song;

                ApplyMetadataToUI(track, song);
                txtStatus.Text = "Ready";
            }
            catch (OperationCanceledException) { }
            catch
            {
                txtStatus.Text = "Metadata error";
            }
        }

        private void ApplyMetadataToUI(MusicTrack track, ItunesSong? song)
        {
            if (song == null)
            {
                txtSongName.Text = track.Title;
                txtArtistName.Text = "Unknown";
                txtAlbumName.Text = "Unknown";
                imgAlbum.Source = null;
                return;
            }

            txtSongName.Text = song.TrackName ?? track.Title;
            txtArtistName.Text = song.ArtistName ?? "Unknown";
            txtAlbumName.Text = song.CollectionName ?? "Unknown";

            if (!string.IsNullOrWhiteSpace(song.ArtworkUrl100))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(song.ArtworkUrl100);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    imgAlbum.Source = bmp;
                }
                catch { imgAlbum.Source = null; }
            }
            else imgAlbum.Source = null;
        }


    }
}


