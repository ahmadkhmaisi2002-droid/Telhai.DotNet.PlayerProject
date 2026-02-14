using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Telhai.DotNet.PlayerProject;

namespace Telhai.DotNet.PlayerProject.ViewModels
{
    public class SongEditorViewModel : ViewModelBase
    {
        private readonly ObservableCollection<MusicTrack> _library;
        private readonly Action _saveLibraryAction;
        private readonly Action _closeAction;

        private MusicTrack? _track;
        public MusicTrack? Track
        {
            get => _track;
            set { _track = value; OnPropertyChanged(); OnPropertyChanged(nameof(Title)); OnPropertyChanged(nameof(Artist)); OnPropertyChanged(nameof(Album)); OnPropertyChanged(nameof(ArtworkUrl)); }
        }

        // עריכה "Per-Song" (מתחבר לשדות שכבר יש לך ב-MusicTrack)
        public string Title
        {
            get => Track?.Title ?? "";
            set { if (Track != null) { Track.Title = value; OnPropertyChanged(); } }
        }

        public string Artist
        {
            get => Track?.ApiArtistName ?? "";
            set { if (Track != null) { Track.ApiArtistName = value; OnPropertyChanged(); } }
        }

        public string Album
        {
            get => Track?.ApiAlbumName ?? "";
            set { if (Track != null) { Track.ApiAlbumName = value; OnPropertyChanged(); } }
        }

        public string ArtworkUrl
        {
            get => Track?.ApiArtworkUrl ?? "";
            set { if (Track != null) { Track.ApiArtworkUrl = value; OnPropertyChanged(); } }
        }

        // "תמונות משויכות" — נשמור מקומית ב-Images ונשים את הנתיב בתוך ApiArtworkUrl
        public ICommand ChooseImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RemoveSongCommand { get; }
        public ICommand CloseCommand { get; }

        public SongEditorViewModel(ObservableCollection<MusicTrack> library, MusicTrack track, Action saveLibraryAction, Action closeAction)
        {
            _library = library;
            _saveLibraryAction = saveLibraryAction;
            _closeAction = closeAction;
            Track = track;

            ChooseImageCommand = new RelayCommand(_ => ChooseImage());
            SaveCommand = new RelayCommand(_ => Save());
            RemoveSongCommand = new RelayCommand(_ => RemoveSong());
            CloseCommand = new RelayCommand(_ => _closeAction());
        }

        private void ChooseImage()
        {
            if (Track == null) return;

            var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp",
                Multiselect = false
            };

            if (ofd.ShowDialog() == true)
            {
                // מעתיקים לתיקיית Images בתוך הפרויקט בזמן ריצה
                string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                Directory.CreateDirectory(imagesDir);

                string fileName = Path.GetFileName(ofd.FileName);
                string dest = Path.Combine(imagesDir, fileName);

                File.Copy(ofd.FileName, dest, overwrite: true);

                // נשמור נתיב מקומי (ולא API)
                Track.ApiArtworkUrl = dest;
                OnPropertyChanged(nameof(ArtworkUrl));
            }
        }

        private void Save()
        {
            _saveLibraryAction();
        }

        private void RemoveSong()
        {
            if (Track == null) return;
            _library.Remove(Track);
            _saveLibraryAction();
            _closeAction();
        }
    }
}
