using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Reloaded.Process.Memory;
using Reloaded_Mod_Template.Heroes.Custom.ONEFILE.Substructures;
using Reloaded_Mod_Template.Utilities;

namespace Reloaded_Mod_Template.Heroes.Custom.ONEFILE
{
    /// <summary>
    /// Contains the structure representing a minimal .ONE file already stored in memory,
    /// that is parsed from game memory on file load requests.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public unsafe class MemoryONEArchive
    {
        /// <summary>
        /// The header for the ONE file.
        /// </summary>
        public ONEHeader* FileHeader;

        /// <summary>
        /// Contains the header which describes the upcoming file name section.
        /// </summary>
        public ONEFileNameSectionHeader* FileNameSectionHeader;

        /// <summary>
        /// Stores an array of usernames containing all of the individual names of 
        /// files inside ONE archives.
        /// </summary>
        public ONEFileName* FileNames;

        /// <summary>
        /// Contains the amount of files stored in the File Name section.
        /// </summary>
        public int FileNameCount;

        /// <summary>
        /// Array of the actual file structures used within the game.
        /// Equivalent to the file count.
        /// </summary>
        public List<MemoryONEFile> Files;

        /// <summary>
        /// Private constructor, please use <see cref="ParseONEFromMemory"/> factory method instead.
        /// </summary>
        private MemoryONEArchive() { }

        /// <summary>
        /// Parses the details of a minimal Heroes .ONE file from a supplied memory
        /// address and length. This is simply a modified version of HeroesONE-R's version.
        /// </summary>
        /// <returns>The structure of a ONE Archive.</returns>
        public static MemoryONEArchive ParseONEFromMemory(IntPtr filePointer, int fileLength)
        {
            // Creates a .ONE Archive instance ready to feed.
            MemoryONEArchive oneArchive = new MemoryONEArchive();

            // Stores a pointer, increasing as we read the individual file structures.
            int pointer = 0;

            // Get the header and individual sections.
            oneArchive.FileHeader = (ONEHeader*)StructUtilities.MemoryOffsetToPointer<ONEHeader>(filePointer, pointer, ref pointer);
            oneArchive.FileNameSectionHeader = (ONEFileNameSectionHeader*)StructUtilities.MemoryOffsetToPointer<ONEFileNameSectionHeader>(filePointer, pointer, ref pointer);

            // Get the filenames.
            oneArchive.FileNameCount = (*oneArchive.FileNameSectionHeader).GetNameCount();
            oneArchive.FileNames = (ONEFileName*)StructUtilities.MemoryOffsetToPointer<ONEFileName>(filePointer, pointer);
            pointer += (*oneArchive.FileNameSectionHeader).FileNameSectionLength;

            // Parse all of the files.
            oneArchive.Files = new List<MemoryONEFile>(oneArchive.FileNameCount);
            int fileCount = 0;
            for (int x = 0; x < oneArchive.FileNameCount; x++)
            {
                if (oneArchive.FileNames[x].ToString() != "")
                    fileCount += 1;
            }

            // Some ONE files have been padded at the end of file, thus reading until the end of file may fail - only take as many files as we have filenames.
            while ((pointer < fileLength) && (oneArchive.Files.Count < fileCount))
            {
                // Create ONE ArchiveFile
                MemoryONEFile oneFile = new MemoryONEFile();

                // Parse the contents.
                oneFile.ONEFileHeader = (ONEFileHeader*)StructUtilities.MemoryOffsetToPointer<ONEFileHeader>(filePointer, pointer, ref pointer);
                oneFile.CompressedDataPointer = (byte*)StructUtilities.MemoryOffsetToPointer<ONEFileHeader>(filePointer, pointer);
                oneFile.DataLength = oneFile.ONEFileHeader[0].FileSize;
                pointer += oneFile.DataLength;

                // We're done here :3
                oneArchive.Files.Add(oneFile);
            }

            return oneArchive;
        }
    }
}
