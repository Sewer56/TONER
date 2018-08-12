using Reloaded_Mod_Template.Heroes.Custom.RenderWare;

namespace Reloaded_Mod_Template.Heroes.Custom.ONEFILE.Substructures
{
    public struct ONEFileNameSectionHeader
    {
        /// <summary>
        /// Always 1
        /// Maybe compression related?
        /// </summary>
        public int Unknown;

        /// <summary>
        /// Stores the total length of the file name section.
        /// Each file has maximum name length of 64 bytes/characters.
        /// Thus to get the entry count, divide this number by 64.
        /// </summary>
        public int FileNameSectionLength;

        /// <summary>
        /// Another instance of the RenderWare Version
        /// </summary>
        public RWVersion RenderWareVersion;

        /// <summary>
        /// Retrieves the amount of ArchiveFile Name entries following the header.
        /// </summary>
        /// <returns></returns>
        public int GetNameCount()
        {
            return FileNameSectionLength / ONEFileName.FileNameLength;
        }
    }
}
