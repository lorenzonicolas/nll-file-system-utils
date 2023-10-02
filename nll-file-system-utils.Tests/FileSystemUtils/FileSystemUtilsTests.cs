using Moq;
using DTO;
using System.IO.Abstractions;
using FileSystem;

namespace Tests.FileSystemUtils
{
    [TestFixture]
    [Parallelizable]
    public partial class FileSystemUtilsTests
    {
        public required IFileSystemUtils utils;

        [OneTimeSetUp]
        public void Setup()
        {
            // Mocked data
            MockData();

            this.utils = new SomeUtils(regexUtils.Object, fs.Object);
        }

        [TestCase("FRONT.JPG", true)]
        [TestCase("FOLDER.JPG", true)]
        [TestCase("cover.JPG", false)]
        public void IsAlbumNameCorrect(string albumName, bool isCorrect)
        {
            Assert.That(utils.IsAlbumNameCorrect(albumName), Is.EqualTo(isCorrect));
        }

        [TestCase("Some Album Name (EP)", "EP")]
        [TestCase("Some Album Name [EP]", "EP")]
        [TestCase("Some Album Name (Demo)", "Demo")]
        [TestCase("Some Album Name [Demo]", "Demo")]
        [TestCase("Some Album Name (Single)", "Single")]
        [TestCase("Some Album Name [Single]", "Single")]
        [TestCase("Some Album Name (Split)", "Split")]
        [TestCase("Some Album Name [Split]", "Split")]
        [TestCase("Some Album Name", "FullAlbum")]
        public void GetAlbumType(string algo, string expected)
        {
            var result = utils.GetAlbumType(algo);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(FolderTestType.NormalAlbum, 2)]
        [TestCase(FolderTestType.AlbumWithInnerCds, 0)]
        [TestCase(FolderTestType.BandFolder, 0)]
        [TestCase(FolderTestType.RootBandsFolder, 0)]
        public void GetFolderSongs(FolderTestType folderType, int expected)
        {
            var result = utils.GetFolderSongs(GetObjectToUse(folderType));

            Assert.That(result, Has.Length.EqualTo(expected));
        }

        [TestCase(FolderTestType.NormalAlbum, true)]
        [TestCase(FolderTestType.AlbumWithInnerCds, true)]
        [TestCase(FolderTestType.BandFolder, false)]
        [TestCase(FolderTestType.RootBandsFolder, false)]
        public void IsAlbumFolder(FolderTestType folderType, bool expected)
        {
            var result = utils.IsAlbumFolder(GetObjectToUse(folderType));
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(FolderTestType.NormalAlbum, false)]
        [TestCase(FolderTestType.AlbumWithInnerCds, false)]
        [TestCase(FolderTestType.BandFolder, false)]
        [TestCase(FolderTestType.RootBandsFolder, true)]
        [TestCase(FolderTestType.ExtraLevelFolder, false)]
        public void IsRootArtistsFolder(FolderTestType folderType, bool expected)
        {
            var result = utils.IsRootArtistsFolder(GetObjectToUse(folderType));
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(FolderTestType.NormalAlbum, false)]
        [TestCase(FolderTestType.AlbumWithInnerCds, true)]
        [TestCase(FolderTestType.AlbumWithInnerCds_Disc, true)]
        [TestCase(FolderTestType.BandFolder, false)]
        [TestCase(FolderTestType.RootBandsFolder, false)]
        [TestCase(FolderTestType.NullFolder, false)]
        public void AlbumContainsCDFolders(FolderTestType folderType, bool expected)
        {
            var result = utils.AlbumContainsCDFolders(GetObjectToUse(folderType));
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(FolderTestType.NormalAlbum, false)]
        [TestCase(FolderTestType.AlbumWithInnerCds, false)]
        [TestCase(FolderTestType.AlbumWithInnerCds_Disc, false)]
        [TestCase(FolderTestType.BandFolder, true)]
        [TestCase(FolderTestType.RootBandsFolder, true)]
        public void GetAnyFolderSong(FolderTestType folderType, bool resultIsNull)
        {
            var result = utils.GetAnyFolderSong(GetObjectToUse(folderType));
            Assert.That(result is null, Is.EqualTo(resultIsNull));
        }

        [TestCase(FolderTestType.NormalAlbum, 4)]
        [TestCase(FolderTestType.AlbumWithInnerCds, 0)]
        [TestCase(FolderTestType.BandFolder, 0)]
        [TestCase(FolderTestType.RootBandsFolder, 0)]
        public void GetFolderImages(FolderTestType folderType, int expected)
        {
            var result = utils.GetFolderImages(GetObjectToUse(folderType));

            Assert.That(result, Has.Length.EqualTo(expected));
        }

        [TestCase(FolderTestType.NormalAlbum, false)]
        [TestCase(FolderTestType.AlbumWithInnerCds, false)]
        [TestCase(FolderTestType.BandFolder, true)]
        [TestCase(FolderTestType.RootBandsFolder, false)]
        public void IsArtistFolder (FolderTestType folderType, bool expected)
        {
            var result = utils.IsArtistFolder(GetObjectToUse(folderType));
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(FolderTestType.NormalAlbum, "FRONT.jpg")]
        [TestCase(FolderTestType.AlbumWithWeirdImage, "weirdName.jpg")]
        [TestCase(FolderTestType.AlbumWithInnerCds, null)]
        [TestCase(FolderTestType.BandFolder, null)]
        [TestCase(FolderTestType.RootBandsFolder, null)]
        public void GetAlbumCover(FolderTestType folderType, string expectedFileName)
        {
            var result = utils.GetAlbumCover(GetObjectToUse(folderType));
            Assert.That(result?.Name, Is.EqualTo(expectedFileName));
        }

        [TestCase]
        public void GetAlbumCover_Exception()
        {
            Assert.Throws<Exception>(() => utils.GetAlbumCover(exceptionThrownFolder.Object));
        }

        [TestCase(FolderTestType.NormalAlbum, FolderType.Album)]
        [TestCase(FolderTestType.AlbumWithWeirdImage, FolderType.Album)]
        [TestCase(FolderTestType.AlbumWithInnerCds, FolderType.AlbumWithMultipleCDs)]
        [TestCase(FolderTestType.BandFolder, FolderType.ArtistWithAlbums)]
        [TestCase(FolderTestType.RootBandsFolder, FolderType.Album, true)]
        public void GetFolderType(FolderTestType folderType, FolderType expected, bool throwsEx = false)
        {
            if (throwsEx)
            {
                Assert.Throws<SomeUtils.FolderTypeException>(() => utils.GetFolderType(GetObjectToUse(folderType)));
            }
            else
            {
                Assert.That(utils.GetFolderType(GetObjectToUse(folderType)), Is.EqualTo(expected));
            }                
        }

        [TestCase(FolderTestType.NormalAlbum, 0)]
        [TestCase(FolderTestType.AlbumWithInnerCds, 2)]
        [TestCase(FolderTestType.BandFolder, 1)]
        [TestCase(FolderTestType.RootBandsFolder, 0)]
        public void GetFolderAlbums(FolderTestType folderType, int expected)
        {
            var result = utils.GetFolderAlbums(GetObjectToUse(folderType));
            Assert.That(result?.Length, Is.EqualTo(expected));
        }

        [TestCase(FolderTestType.NormalAlbum, 0)]
        [TestCase(FolderTestType.AlbumWithInnerCds, 2)]
        [TestCase(FolderTestType.BandFolder, 1)]
        [TestCase(FolderTestType.RootBandsFolder, 0)]
        public void GetFolderAlbums_string(FolderTestType folderType, int expected)
        {
            var folderToUse = GetObjectToUse(folderType);
            directoryInfo.Setup(x => x.New("path")).Returns(folderToUse);

            var result = utils.GetFolderAlbums("path");
            Assert.That(result?.Count, Is.EqualTo(expected));
        }

        [TestCase(FolderTestType.NormalAlbum, 0)]
        [TestCase(FolderTestType.AlbumWithInnerCds, 0)]
        [TestCase(FolderTestType.BandFolder, 0)]
        [TestCase(FolderTestType.RootBandsFolder, 1)]
        public void GetFolderArtists(FolderTestType folderType, int expected)
        {
            var result = utils.GetFolderArtists(GetObjectToUse(folderType));
            Assert.That(result?.Length, Is.EqualTo(expected));
        }

        [TestCase(FolderTestType.NormalAlbum, 0)]
        [TestCase(FolderTestType.AlbumWithInnerCds, 0)]
        [TestCase(FolderTestType.BandFolder, 0)]
        [TestCase(FolderTestType.RootBandsFolder, 1)]
        public void GetFolderArtists_string(FolderTestType folderType, int expected)
        {
            var folderToUse = GetObjectToUse(folderType);
            directoryInfo.Setup(x => x.New("path")).Returns(folderToUse);

            var result = utils.GetFolderArtists("path");
            Assert.That(result?.Length, Is.EqualTo(expected));
        }

        [TestCase(FolderTestType.NullFolder, null)]
        [TestCase(FolderTestType.NormalAlbum, "test")]
        // [TestCase(FolderTestType.BandFolder, null)]
        [TestCase(FolderTestType.AlbumWithInnerCds, "test")]
        public void GetAlbumFolderName(FolderTestType folderType, string expected)
        {
            // Arrange
            var folderToUse = GetObjectToUse(folderType);

            regexUtils.Setup(x => x.GetFolderInformation(It.IsAny<string>()))
                .Returns(new FolderInfo() { Album = "test"});

            if(folderType == FolderTestType.BandFolder)
            {
                folderToUse = utils.GetFolderAlbums(folderToUse).First();
            }
            
            var songFile = utils.GetAnyFolderSong(folderToUse);

            // Act
            var result = utils.GetAlbumFolderName(songFile);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ValidateDirectory(bool createIfNotExists)
        {
            this.directory.Setup(d=>d.Exists("testDirectoryPath"))
                .Returns(true);

            if(createIfNotExists)
                this.directory.Setup(d => d.CreateDirectory("testDirectoryPath"))
                    .Returns(new Mock<IDirectoryInfo>().Object);

            var result = utils.ValidateDirectory("testDirectoryPath", createIfNotExists);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateDirectory_Exception()
        {
            this.directory.Setup(d => d.CreateDirectory("testDirectoryPath"))
                .Throws(new Exception("exceptionMsg"));
            Assert.Throws<Exception>(() => utils.ValidateDirectory("testDirectoryPath", true));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void UnlockFile(bool isReadonlyAlready)
        {
            var file1 = new Mock<IFileInfo>();
            file1.Setup(f => f.IsReadOnly).Returns(isReadonlyAlready);
            fileInfoFactory.Setup(f => f.New("fileName")).Returns(file1.Object);

            utils.UnlockFile("fileName");

            if (isReadonlyAlready)
                file1.VerifySet(x => x.IsReadOnly = false, Times.Once);
            else
                file1.VerifySet(x => x.IsReadOnly = false, Times.Never);            
        }

        [TestCase("C:\\Music\\Emperor", "C:\\Destiny", "C:\\Destiny\\Emperor")]
        [TestCase("C:\\Music\\Emperor\\1994 - In", "C:\\Destiny", "C:\\Destiny\\1994 - In")]
        public void MoveFolder(string source, string destiny, string expectedDestiny)
        {
            var mockedDir = new Mock<IDirectoryInfo>();
            directoryInfo.Setup(x => x.New(source)).Returns(mockedDir.Object);

            var result = utils.MoveFolder(source, destiny);

            Assert.That(result, Is.EqualTo(expectedDestiny));
            mockedDir.Verify(x => x.MoveTo(expectedDestiny), Times.Once);
        }

        [Test]
        public void SaveImageFile()
        {
            // arrange
            var mockedImage = new List<byte>().ToArray();
            Mock<Stream> mockedStream = new();
            var outputPath = "somePath";

            path.Setup(o => o.Combine(normalAlbum.Object.FullName, "FRONT.jpg")).Returns(outputPath);
            fs.Setup(o => o.FileStream.Create(outputPath, FileMode.CreateNew)).Returns(mockedStream.Object);
            mockedStream.Setup(o => o.Write(mockedImage, 0, mockedImage.Length));

            // act
            utils.SaveImageFile(normalAlbum.Object, mockedImage);

            // assert
            path.Verify(p => p.Combine(normalAlbum.Object.FullName, "FRONT.jpg"), Times.Once);
            fs.Verify(o => o.FileStream.Create(outputPath, FileMode.CreateNew), Times.Once);
            mockedStream.Verify(o => o.Write(mockedImage, 0, mockedImage.Length), Times.Once);
        }

        [TestCase(LOCKED_FILE_NAME, true)]
        [TestCase(EXCEPTION_LOCKED_FILE_NAME, true)]
        [TestCase(UNLOCKED_FILE_NAME, false)]
        public void IsFileLocked(string fileName, bool expected)
        {
            var result = utils.IsFileLocked(fileName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("C:\\Music\\Emperor\\1994 - In", "C:\\Destiny\\Emperor", "C:\\Destiny\\Emperor\\1994 - In")]
        public void CopyFolder(string source, string destiny, string expectedDestiny)
        {
            var mockedDir = new Mock<IDirectoryInfo>();
            mockedDir.Setup(x => x.FullName).Returns(source);
            directoryInfo.Setup(x => x.New(source)).Returns(mockedDir.Object);
            directory.Setup(x => x.Exists(expectedDestiny)).Returns(true);
            mockedDir.Setup(x => x.GetDirectories()).Returns(new List<IDirectoryInfo>().ToArray());

            var mockedFiles = BuildAlbumFolderFiles();
            foreach (var file in mockedFiles)
            {
                file.Setup(x => x.FullName).Returns($"{source}\\{file.Object.Name}");
            }
            mockedDir.Setup(x => x.GetFiles()).Returns(mockedFiles.Select(x => x.Object).ToArray());

            var result = utils.CopyFolder(source, destiny);

            Assert.That(result, Is.EqualTo(expectedDestiny));
            directory.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never);

            foreach (var item in mockedFiles)
            {
                file.Verify(x => x.Copy(item.Object.FullName, $"{expectedDestiny}\\{item.Object.Name}", true), Times.Once);
            }
        }

        [TearDown]
        public void TearDown()
        {
            directory.Invocations.Clear();
            regexUtils.Invocations.Clear();
            directoryInfo.Invocations.Clear();
            file.Invocations.Clear();
        }
    }
}
