using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reloaded_Mod_Template.Heroes.Custom.Memory.Allocator
{
    /// <summary>
    /// Defines an individual mapping that is used to map a game allocated address to our own.
    /// </summary>
    public unsafe struct Mapping
    {
        /// <summary>
        /// The address containing the allocation.
        /// </summary>
        public void* Address;

        /// <summary>
        /// The size of the allocated bytes at this address.
        /// </summary>
        public int Size;
        
        public Mapping(void* address, int size)
        {
            Address = address;
            Size = size;
        }
    }
}
