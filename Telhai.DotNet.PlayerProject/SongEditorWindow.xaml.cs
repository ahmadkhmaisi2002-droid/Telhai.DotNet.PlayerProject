using System;
using System.Collections.ObjectModel;
using System.Windows;
using Telhai.DotNet.PlayerProject.ViewModels;

namespace Telhai.DotNet.PlayerProject
{
    public partial class SongEditorWindow : Window
    {
        public SongEditorWindow(ObservableCollection<MusicTrack> library, MusicTrack track, Action saveLibraryAction)
        {
            InitializeComponent();

            DataContext = new SongEditorViewModel(
                library,
                track,
                saveLibraryAction,
                closeAction: this.Close
            );
        }
    }
}
