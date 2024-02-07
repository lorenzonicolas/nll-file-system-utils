﻿using DTO;
using System.Text.RegularExpressions;

namespace Regex
{
    public class RegexUtils : IRegexUtils
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

        public RegexUtils()
        {
        }

        private const string songsRegex = @"(\d{2})(?>\s?[-.]*\s*)((?:[\wÑñáéíóúÁÉÍÓÚäöüÅÖÜ….,'&()\[\];‘’´!\-–]+\s*)+)(?>\.{1})([\w]+)";
        private const string regex_Band_Album_Year = @"((?:[\wÑñáéíóúÁÉÍÓÚäöüÅÖÜ….,'&]+\s?)+)(?<! )\s*-{1}\s*((?:[\wÑñáéíóúÁÉÍÓÚäöüÅÖÜ…,'&^\-()\.]+\s?)+) \(*([0-9]{4})\)*";
        private const string regex_Year_Album = @"^([\d]{4})(?:\)*\s*-*\s*)((?:[\wÑñáéíóúÁÉÍÓÚäöüÅÖÜ…,'&()\[\-\.\]]+\s?)+)(?<! )";
        private const string regex_Band_Album = @"^((?:[\wÑñáéíóúÁÉÍÓÚäöüÅÖÜ….,'&]+\s?)+)(?<! )\s*-\s*((?:[\wÑñáéíóúÁÉÍÓÚäöüÅÖÜ…,'&()-]+\s?)+)(?<! )";
        private const string regex_band_year_album = @"((?:[\wÑñáéíóúÁÉÍÓÚäöüÅÖÜ….,'&]+\s?)+)(?<! )\s*-{1}\s*\(*([0-9]{4})\)*\s*-{1}\s*((?:[\wÑñáéíóúÁÉÍÓÚäöüÅÖÜ…,'&^\-()\.]+\s?)+)";

        public FolderInfo GetFolderInformation(string folderName)
        {
            var result = new FolderInfo();
            Match matchRegex;

            try
            {
                bool hasYear = DownloadedFolderHasYear(folderName);

                if (hasYear)
                {
                    matchRegex = RunRegex(folderName, regex_Year_Album);
                    if (matchRegex.Success)
                    {
                        result.Year = matchRegex.Groups[1].Value;
                        result.Album = matchRegex.Groups[2].Value;

                        return result;
                    }
                    else
                    {
                        matchRegex = RunRegex(folderName, regex_Band_Album_Year);
                        if (matchRegex.Success)
                        {
                            result.Band = matchRegex.Groups[1].Value;
                            result.Album = matchRegex.Groups[2].Value;
                            result.Year = matchRegex.Groups[3].Value;

                            return result;
                        }
                        else
                        {
                            matchRegex = RunRegex(folderName, regex_band_year_album);
                            if(matchRegex.Success)
                            {
                                result.Band = matchRegex.Groups[1].Value;
                                result.Album = matchRegex.Groups[3].Value;
                                result.Year = matchRegex.Groups[2].Value;
                            }
                        }
                    }
                }
                else
                {
                    matchRegex = RunRegex(folderName, regex_Band_Album);
                    if (matchRegex.Success)
                    {
                        result.Band = matchRegex.Groups[1].Value;
                        result.Album = matchRegex.Groups[2].Value;
                    }
                    else
                        result.Album = folderName;

                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new FolderInfoException("Error on Regex GetFolderInformation: " + ex.Message, ex);
            }
        }

        private bool DownloadedFolderHasYear(string folderName)
        {
            var folderHasYearRegex = @"[(]([0-9]{4})[)]|([0-9]{4})(?:\s*-*\s*)";
            return RunRegex(folderName, folderHasYearRegex).Success;
        }

        public SongInfo GetFileInformation(string fileName)
        {
            try
            {
                var match = RunRegex(fileName, songsRegex);

                if (!match.Success)
                {
                    throw new FileInfoException("Couldn't match any valid file name");
                }

                return new SongInfo(match.Groups[3].Value, match.Groups[2].Value, match.Groups[1].Value);
            }
            catch (Exception ex)
            {
                throw new FileInfoException("Error on Regex GetFileInformation: " + ex.Message, ex);
            }            
        }

        public string ReplaceAllSpaces(string str)
        {
            return System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "%20");
        }

        private Match RunRegex(string input, string regex)
        {
            try
            {
                return System.Text.RegularExpressions.Regex.Match(input, regex, RegexOptions.None, _timeout);
            }
            catch(RegexMatchTimeoutException)
            {
                throw;
            }
            catch(ArgumentOutOfRangeException)
            {
                throw;
            }
        }

        public class FolderInfoException : Exception
        {
            public FolderInfoException(string message) : base(message) { }
            public FolderInfoException(string message, Exception inner) : base(message, inner) { }
        }

        public class FileInfoException : Exception
        {
            public FileInfoException(string message, Exception inner) : base(message, inner) { }
            public FileInfoException(string message) : base(message) { }
        }
    }
}