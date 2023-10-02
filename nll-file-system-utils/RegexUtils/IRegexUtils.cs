using DTO;

namespace Regex
{
    public interface IRegexUtils
    {
        string ReplaceAllSpaces(string str);
        SongInfo GetFileInformation(string fileName);
        FolderInfo GetFolderInformation(string folderName);
    }
}
