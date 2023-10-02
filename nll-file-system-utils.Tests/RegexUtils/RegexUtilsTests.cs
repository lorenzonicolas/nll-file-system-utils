using Moq;
using Regex;

namespace Tests.Regex
{
    public class RegexUtilsTests
    {
        private IRegexUtils regexUtils;

        [SetUp]
        public void Setup()
        {
            this.regexUtils = new RegexUtils();
        }

        [Test]
        public void ReplaceAllSpaces_NoSpaces()
        {
            var result = regexUtils.ReplaceAllSpaces("stringWithoutSpaces");

            Assert.That(result, Is.EqualTo("stringWithoutSpaces"));
        }

        [Test]
        public void ReplaceAllSpaces()
        {
            var result = regexUtils.ReplaceAllSpaces("string With Spaces");

            Assert.That(result, Is.EqualTo("string%20With%20Spaces"));
        }

        [Test]
        [TestCase("10 - perfect title.mp3", 10, "perfect title", "mp3")]
        [TestCase("10 - title (With parentesis).mp3", 10, "title (With parentesis)", "mp3")]
        [TestCase("10 - title [With brackets].mp3", 10, "title [With brackets]", "mp3")]
        [TestCase("10 - title (With Parentesis) [With brackets].mp3", 10, "title (With Parentesis) [With brackets]", "mp3")]
        [TestCase("10-title   .mp3", 10, "title   ", "mp3")]
        [TestCase("10-title.mp3", 10, "title", "mp3")]
        [TestCase("10- title.mp3", 10, "title", "mp3")]
        [TestCase("10 -title.mp3", 10, "title", "mp3")]
        [TestCase("12 - Tortured by Disingenuous Light – The Seventh Shrine.mp3\r\n", 12, "Tortured by Disingenuous Light – The Seventh Shrine", "mp3")]
        [TestCase("10 - Pentagram & Wood.mp3", 10, "Pentagram & Wood", "mp3")]
        [Parallelizable(ParallelScope.All)]
        public void GetFileInformation_Success(string fileName, int trackNumber, string title, string extension)
        {
            var result = regexUtils.GetFileInformation(fileName);
            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo(title));
                Assert.That(result.Extension, Is.EqualTo(extension));
                Assert.That(result.TrackNumber, Is.EqualTo(trackNumber.ToString()));
            });
        }

        [Test]
        [TestCase(null)]
        [TestCase("onlyFileName")]
        [TestCase("fileWithExtension.mp3")]
        [TestCase("10-.mp3")]
        [TestCase("10 - TestWithout extension")]
        [Parallelizable(ParallelScope.All)]
        public void GetFileInformation_Error(string fileName)
        {
            Assert.Throws<RegexUtils.FileInfoException>(() => regexUtils.GetFileInformation(fileName));
        }

        [Test]
        [TestCase("1990 - albumName", 1990, "albumName", null)]
        [TestCase("1990 albumName", 1990, "albumName", null)]
        [TestCase("1990- albumName", 1990, "albumName", null)]
        [TestCase("1990 -albumName", 1990, "albumName", null)]
        [TestCase("1990     -      albumName", 1990, "albumName", null)]
        [TestCase("1990           albumName", 1990, "albumName", null)]
        [TestCase("1990 - albumName with spaces", 1990, "albumName with spaces", null)]
        [TestCase("1990 - Album name (Live Tokyo '92)", 1990, "Album name (Live Tokyo '92)", null)]
        [TestCase("1990 - Album name [Live Tokyo '92]", 1990, "Album name [Live Tokyo '92]", null)]
        [TestCase("1990 - Album name [Live Tokyo '92] (With Bonus)", 1990, "Album name [Live Tokyo '92] (With Bonus)", null)]
        [TestCase("2023 - As in Gardens, So in Tombs", 2023, "As in Gardens, So in Tombs", null)]
        [Parallelizable(ParallelScope.All)]
        public void GetFolderInformation_NoBand_Success(string folderName, int year, string album, string band)
        {
            var result = regexUtils.GetFolderInformation(folderName);
            var expectedBand = band == null ? string.Empty : band;

            Assert.Multiple(() =>
            {
                Assert.That(result.Year, Is.EqualTo(year.ToString()));
                Assert.That(result.Album, Is.EqualTo(album));
                Assert.That(result.Band, Is.EqualTo(expectedBand));
            });
        }

        [Test]
        [TestCase("Spirit Adrift - Divided by Darkness (2019) [320]", 2019, "Divided by Darkness", "Spirit Adrift")]
        [TestCase("Satyricon & Darkthrone- Live In Wacken (2004)", 2004, "Live In Wacken", "Satyricon & Darkthrone")]
        [TestCase("Nordjevel - Necrogenesis (Limited Edition) (2019)", 2019, "Necrogenesis (Limited Edition)", "Nordjevel")]
        [TestCase("...and Oceans - As in Gardens, So in Tombs (Deluxe Editon) (2023)", 2023, "As in Gardens, So in Tombs (Deluxe Editon)", "...and Oceans")]
        [TestCase("Suffocation - Pierced From Within", null, "Pierced From Within", "Suffocation")]
        [TestCase("Pierced From Within", null, "Pierced From Within", null)]
        [Parallelizable(ParallelScope.All)]
        public void GetFolderInformation_WithBand_Success(string folderName, int? year, string album, string band)
        {
            var result = regexUtils.GetFolderInformation(folderName);
            var expectedYearStr = year.HasValue ? year.ToString() : string.Empty;
            var expectedBand = band == null ? string.Empty : band;

            Assert.Multiple(() =>
            {
                Assert.That(result.Year, Is.EqualTo(expectedYearStr));
                Assert.That(result.Album, Is.EqualTo(album));
                Assert.That(result.Band, Is.EqualTo(expectedBand));
            });
        }

        [Test]
        [TestCase("2008")]
        public void GetFolderInformation_NullCases(string folderName)
        {
            var result = regexUtils.GetFolderInformation(folderName);
            Assert.Multiple(() =>
            {
                Assert.That(result.Album, Is.Empty);
                Assert.That(result.Year, Is.Empty);
                Assert.That(result.Band, Is.Empty);
            });
        }

        [Test]
        public void GetFolderInformation_Timeout()
        {
            // TODO
        }
    }
}
