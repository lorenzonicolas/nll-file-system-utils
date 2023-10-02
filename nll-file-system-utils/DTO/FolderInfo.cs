namespace DTO
{
    public class FolderInfo
    {
        public string Band { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        
        public FolderInfo(string band = "", string album = "", string year = "")
        {
            Band = band;
            Album = album;
            Year = year;
        }
    }
}
