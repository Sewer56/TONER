namespace Reloaded_Mod_Template.Heroes.Custom.ONEFILE.Substructures
{
    public struct ONEFile
    {
        /// <summary>
        /// Stores the header for this individual ONE file.
        /// </summary>
        public ONEFileHeader ONEFileHeader;

        /// <summary>
        /// Stores the PRS compressed data for the individual file.
        /// </summary>
        public byte[] CompressedData;
    }
}
