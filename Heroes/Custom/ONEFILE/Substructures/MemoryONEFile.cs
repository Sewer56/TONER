namespace Reloaded_Mod_Template.Heroes.Custom.ONEFILE.Substructures
{
    public unsafe struct MemoryONEFile
    {
        /// <summary>
        /// Stores the header for this individual ONE file.
        /// </summary>
        public ONEFileHeader* ONEFileHeader;

        /// <summary>
        /// Stores the PRS compressed data for the individual file.
        /// </summary>
        public byte* CompressedDataPointer;

        /// <summary>
        /// Contains the length of the PRS compressed data.
        /// </summary>
        public int DataLength;
    }
}
