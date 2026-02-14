using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telhai.DotNet.PlayerProject
{
    public class MusicTrack 
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public string? ApiSongName { get; set; }
        public string? ApiArtistName { get; set; }
        public string? ApiAlbumName { get; set; }
        public string? ApiArtworkUrl { get; set; }

        // This makes sure the ListBox shows the Name, not "MyMusicPlayer.MusicTrack"
        public override string ToString()
        {
            return Title;
        }
    }
}
