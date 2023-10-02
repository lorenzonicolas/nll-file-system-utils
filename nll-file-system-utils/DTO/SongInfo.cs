namespace DTO
{
    public class SongInfo
    {
        public SongInfo(string extension, string title, string trackNumber)
        {
            Extension = extension;
            Title = title;
            TrackNumber = trackNumber;
        }

        public string TrackNumber { get; set; }
        public string Title { get; set; }
        public string Extension { get; set; }
    }
}
