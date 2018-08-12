namespace Reloaded_Mod_Template.Heroes.Custom.Memory.Tracer
{
    /// <summary>
    /// Contains details about a memory address that has been allocated with
    /// the game's own malloc.
    /// </summary>
    public unsafe struct MemoryAddressDetails
    {
        /// <summary>
        /// The size of the memory allocated.
        /// </summary>
        public int MemorySize;

        public MemoryAddressDetails(int memorySize)
        {
            MemorySize = memorySize;
        }
    }
}
