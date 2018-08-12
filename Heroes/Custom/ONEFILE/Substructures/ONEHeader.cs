using Reloaded_Mod_Template.Heroes.Custom.RenderWare;

namespace Reloaded_Mod_Template.Heroes.Custom.ONEFILE.Substructures
{
    /// <summary>
    /// Contains the header for the ONE file.
    /// </summary>
    public struct ONEHeader
    {
        /// <summary>
        /// Always 0
        /// </summary>
        public int Null;

        /// <summary>
        /// Contains the total filesize of the .ONE file, minus the 0xC header.
        /// </summary>
        public int FileSize;

        /// <summary>
        /// Stores the RenderWare version assigned to this .ONE file.
        /// </summary>
        public RWVersion RenderWareVersion;
    }
}
