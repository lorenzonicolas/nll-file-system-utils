using Moq;
using Regex;
using System.IO.Abstractions;

namespace Tests.FileSystemUtils
{
    public partial class FileSystemUtilsTests
    {
        private const string EMPEROR_ALBUM_NAME = "1994 - In the Nightside Eclipse";
        private const string LOCKED_FILE_NAME = "lockedFile.mp3";
        private const string UNLOCKED_FILE_NAME = "unlockedFile.mp3";
        private const string EXCEPTION_LOCKED_FILE_NAME = "exceptionLockedFile.mp3";
        private readonly Mock<IRegexUtils> regexUtils = new();
        private readonly Mock<IFileSystem> fs = new();
        private readonly Mock<IDirectory> directory = new();
        private readonly Mock<IPath> path = new();
        private readonly Mock<IFileStreamFactory> fileStream = new();
        private readonly Mock<IDirectoryInfoFactory> directoryInfo = new();
        private readonly Mock<IFileInfoFactory> fileInfoFactory = new();
        private Mock<IFileInfo> lockedFile = new();
        private Mock<IFileInfo> unlockedFile = new();
        private readonly Mock<IFile> file = new();

        /// <summary>
        /// Song[]
        /// </summary>
        private Mock<IDirectoryInfo> normalAlbum = new ();

        /// <summary>
        /// album -> song[]
        /// </summary>
        private readonly Mock<IDirectoryInfo> albumWithWeirdImageFileName = new();

        /// <summary>
        /// [CD1,CD2] -> song[]
        /// </summary>
        private readonly Mock<IDirectoryInfo> albumWithInnerFolders = new();

        /// <summary>
        /// [Disc1,Disc2] -> song[]
        /// </summary>
        private readonly Mock<IDirectoryInfo> albumWithInnerFolders_Disc = new();

        /// <summary>
        /// album -> song[]
        /// </summary>
        private readonly Mock<IDirectoryInfo> bandFolder = new();

        /// <summary>
        /// bands[] -> album -> song[]
        /// </summary>
        private readonly Mock<IDirectoryInfo> rootBandsFolder = new();
        
        /// <summary>
        /// extra Level -> bands[] -> album -> song[]
        /// </summary>
        private readonly Mock<IDirectoryInfo> extraLevelFolder = new();

        /// <summary>
        /// folder that throws exception on basic operations
        /// </summary>
        private readonly Mock<IDirectoryInfo> exceptionThrownFolder = new();

        /// <summary>
        /// Generates folder mocks to use in tests
        /// </summary>
        private void MockData()
        {
            // Mocked interfaces
            fs.Setup(f => f.Directory).Returns(directory.Object);
            fs.Setup(f => f.DirectoryInfo).Returns(directoryInfo.Object);
            fs.Setup(f => f.FileInfo).Returns(fileInfoFactory.Object);
            fs.Setup(f => f.File).Returns(file.Object);
            fs.Setup(f => f.Path).Returns(path.Object);
            fs.Setup(f => f.FileStream).Returns(fileStream.Object);

            // Setup a normal album folder with songs, images and a txt
            normalAlbum = CreateNormalAlbum();

            // Setup an album folder with a weird image name instead of FRONT.jpg
            var files = BuildAlbumFiles_WeirdImage();
            albumWithWeirdImageFileName.Setup(mock => mock.GetFiles()).Returns(files.ToArray());
            albumWithWeirdImageFileName.Setup(mock => mock.Name).Returns(EMPEROR_ALBUM_NAME);

            // Setup an album folder with inner cd folders: albumname/cd1/songs etc.
            List<IDirectoryInfo> innerCDs = GetInnerCDs("CD");
            albumWithInnerFolders.Setup(mock => mock.EnumerateDirectories()).Returns(innerCDs);
            albumWithInnerFolders.Setup(mock => mock.GetDirectories()).Returns(innerCDs.ToArray());
            albumWithInnerFolders.Setup(mock => mock.Name).Returns(EMPEROR_ALBUM_NAME);

            // Setup an album folder with inner cd folders: albumname/disc1/songs etc.
            List<IDirectoryInfo> innerDiscs = GetInnerCDs("Disc");
            albumWithInnerFolders_Disc.Setup(mock => mock.EnumerateDirectories()).Returns(innerDiscs);
            albumWithInnerFolders_Disc.Setup(mock => mock.GetDirectories()).Returns(innerDiscs.ToArray());
            albumWithInnerFolders_Disc.Setup(mock => mock.Name).Returns(EMPEROR_ALBUM_NAME);

            // Setup a normal band folder with 1 normal album
            bandFolder.Setup(mock => mock.EnumerateDirectories())
                .Returns(new List<IDirectoryInfo> { normalAlbum.Object });
            bandFolder.Setup(mock => mock.GetDirectories())
                .Returns(new List<IDirectoryInfo> { normalAlbum.Object }.ToArray());
            bandFolder.Setup(mock => mock.Name).Returns("Emperor");

            // Setup a root artists folder - with only Emperor as band
            rootBandsFolder.Setup(mock => mock.EnumerateDirectories())
                .Returns(new List<IDirectoryInfo> { bandFolder.Object });
            rootBandsFolder.Setup(mock => mock.GetDirectories())
                .Returns(new List<IDirectoryInfo> { bandFolder.Object }.ToArray());

            // Setup an extra level folder with root artists folder insise
            extraLevelFolder.Setup(mock => mock.EnumerateDirectories())
                .Returns(new List<IDirectoryInfo> { rootBandsFolder.Object });
            extraLevelFolder.Setup(mock => mock.GetDirectories())
                .Returns(new List<IDirectoryInfo> { rootBandsFolder.Object }.ToArray());

            // Setup a folder that throws exception on actions
            exceptionThrownFolder.Setup(mock => mock.GetFiles()).Throws<Exception>();
            exceptionThrownFolder.Setup(mock => mock.GetDirectories()).Throws<Exception>();

            // Create locked/unlocked files
            lockedFile = CreateFile(LOCKED_FILE_NAME, true);
            unlockedFile = CreateFile(UNLOCKED_FILE_NAME, false);

            fileInfoFactory.Setup(mock => mock.New(LOCKED_FILE_NAME)).Returns(lockedFile.Object);
            fileInfoFactory.Setup(mock => mock.New(UNLOCKED_FILE_NAME)).Returns(unlockedFile.Object);
            fileInfoFactory.Setup(mock => mock.New(EXCEPTION_LOCKED_FILE_NAME)).Returns(unlockedFile.Object);

            fileStream.Setup(mock => mock.New(EXCEPTION_LOCKED_FILE_NAME, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                .Throws<Exception>();
        }

        private Mock<IFileInfo> CreateFile(string fileName, bool isLocked = false)
        {
            var mock = new Mock<IFileInfo>();
            mock.Setup(f => f.Name).Returns(fileName);
            mock.Setup(f => f.Extension).Returns(fileName.Split('.').Last());
            mock.Setup(f => f.IsReadOnly).Returns(isLocked);

            return mock;
        }

        List<IDirectoryInfo> GetInnerCDs(string albumName)
        {
            var cd1 = CreateNormalAlbum();
            cd1.Setup(mock => mock.Name).Returns(albumName+"1");
            cd1.Setup(mock => mock.Parent).Returns(albumWithInnerFolders.Object);
            var cd2 = CreateNormalAlbum();
            cd2.Setup(mock => mock.Name).Returns(albumName+"2");
            cd2.Setup(mock => mock.Parent).Returns(albumWithInnerFolders.Object);
            var innerCDs = new List<IDirectoryInfo> { cd1.Object, cd2.Object };
            return innerCDs;
        }

        private static Mock<IDirectoryInfo> CreateNormalAlbum()
        {
            var mockedFiles = BuildAlbumFolderFiles();
            var albumMock = new Mock<IDirectoryInfo>();
            var parentFolderMock = new Mock<IDirectoryInfo>();
            albumMock.Setup(mock => mock.GetFiles()).Returns(mockedFiles.Select(x=>x.Object).ToArray());
            albumMock.Setup(mock => mock.Name).Returns(EMPEROR_ALBUM_NAME);

            foreach (var file in mockedFiles)
            {
                file.Setup(x => x.Directory).Returns(albumMock.Object);
                file.Setup(x => x.Directory.Parent).Returns(parentFolderMock.Object);
            }
            return albumMock;
        }

        private IDirectoryInfo GetObjectToUse(FolderTestType value)
        {
            switch (value)
            {
                case FolderTestType.NormalAlbum:
                    return normalAlbum.Object;
                case FolderTestType.AlbumWithInnerCds:
                    return albumWithInnerFolders.Object;
                case FolderTestType.AlbumWithInnerCds_Disc:
                    return albumWithInnerFolders_Disc.Object;
                case FolderTestType.BandFolder:
                    return bandFolder.Object;
                case FolderTestType.RootBandsFolder:
                    return rootBandsFolder.Object;
                case FolderTestType.AlbumWithWeirdImage:
                    return albumWithWeirdImageFileName.Object;
                case FolderTestType.ExtraLevelFolder:
                    return extraLevelFolder.Object;
                case FolderTestType.NullFolder:
                    return null;
                default:
                    break;
            }

            return normalAlbum.Object;
        }

        public enum FolderTestType
        {
            NormalAlbum,
            AlbumWithInnerCds,
            AlbumWithInnerCds_Disc,
            BandFolder,
            RootBandsFolder,
            AlbumWithWeirdImage,
            ExtraLevelFolder,
            NullFolder
        }

        private Mock<IDirectoryInfo> BuildMockedDirectoryFromJSON(string json)
        {
            var mockExplanation = Newtonsoft.Json.JsonConvert.DeserializeObject<MockFolder>(json);

            return BuildDirectory(mockExplanation);
        }

        private Mock<IDirectoryInfo> BuildDirectory(MockFolder? info, IDirectoryInfo? parentFolder = null)
        {
            var directory = new Mock<IDirectoryInfo>();

            if(info == null)
            {
                return directory;
            }

            directory.Setup(x => x.Exists).Returns(true);
            directory.Setup(x => x.Name).Returns(info.FolderName);

            var mockedFiles = info.Files.Select(x => BuildFile(x).Object).ToArray();
            directory.Setup(x => x.GetFiles()).Returns(mockedFiles);

            // Parent folder setup
            var parent = parentFolder ?? BuildDirectory(info.FolderParent).Object;
            directory.Setup(x => x.Parent).Returns(parent);

            // Child folders setup
            var childFolders = info.ChildFolders.Select(x => BuildDirectory(x, directory.Object).Object).ToArray();
            directory.Setup(x => x.GetDirectories()).Returns(childFolders);

            return directory;
        }

        private static Mock<IFileInfo> BuildFile(MockFile info)
        {
            var file = new Mock<IFileInfo>();
            file.Setup(x => x.Exists).Returns(true);
            file.Setup(x => x.Extension).Returns(info.FileName.Split('.').Last());
            file.Setup(x => x.Name).Returns(info.FileName);
            file.Setup(x => x.FullName).Returns(info.FileFullName);
            file.Setup(x => x.DirectoryName).Returns(info.FileFullName);

            return file;
        }

        private class MockFolder
        {
            public required string FolderName { get; set ; }
            public required MockFolder FolderParent { get; set; }
            public required IList<MockFolder> ChildFolders { get; set; }
            public required IList<MockFile> Files { get; set; }
        }

        private class MockFile
        {
            public required string FileName { get; set; }
            public required string FileFullName { get; set; }
        }

        private static List<IFileInfo> BuildAlbumFiles_WeirdImage()
        {
            var file1 = new Mock<IFileInfo>();
            file1.Setup(f => f.Name).Returns("file1.mp3");
            file1.Setup(f => f.Extension).Returns(".mp3");

            var file31 = new Mock<IFileInfo>();
            file31.Setup(f => f.Name).Returns("weirdName.jpg");
            file31.Setup(f => f.Extension).Returns(".jpg");

            var fileList = new List<IFileInfo>
            {
                file1.Object, file31.Object
            };
            return fileList;
        }

        private static List<Mock<IFileInfo>> BuildAlbumFolderFiles()
        {
            var file1 = new Mock<IFileInfo>();
            file1.Setup(f => f.Name).Returns("file1.mp3");
            file1.Setup(f => f.Extension).Returns(".mp3");

            var file2 = new Mock<IFileInfo>();
            file2.Setup(f => f.Name).Returns("file2.wav");
            file2.Setup(f => f.Extension).Returns(".wav");

            var file31 = new Mock<IFileInfo>();
            file31.Setup(f => f.Name).Returns("FRONT.jpg");
            file31.Setup(f => f.Extension).Returns(".jpg");

            var file32 = new Mock<IFileInfo>();
            file32.Setup(f => f.Name).Returns("file32.jpe");
            file32.Setup(f => f.Extension).Returns(".jpe");

            var file33 = new Mock<IFileInfo>();
            file33.Setup(f => f.Name).Returns("file33.bmp");
            file33.Setup(f => f.Extension).Returns(".bmp");

            var file34 = new Mock<IFileInfo>();
            file34.Setup(f => f.Name).Returns("file34.png");
            file34.Setup(f => f.Extension).Returns(".png");

            var filex = new Mock<IFileInfo>();
            filex.Setup(f => f.Name).Returns("file3.txt");
            filex.Setup(f => f.Extension).Returns(".txt");

            var fileList = new List<Mock<IFileInfo>>
            {
                file1, file2, file31, file32, file33, file34, filex
            };
            return fileList;
        }
    }
}