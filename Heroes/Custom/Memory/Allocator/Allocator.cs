using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Reloaded_Mod_Template.Heroes.Custom.Memory.Tracer;

namespace Reloaded_Mod_Template.Heroes.Custom.Memory.Allocator
{
    /// <summary>
    /// Provides an implementation of a dynamic .ONE memory allocator buffer for loading in the
    /// internal contents of .ONE files.
    /// </summary>
    public unsafe class Allocator
    {
        // Map individual game allocated addresses to list of buffers.
        // This is in the case the game reuses one address multiple times and the buffer is still not large enough.

        /// <summary>
        /// Maintains an active list of allocation overrides used.
        /// </summary>
        public Dictionary<int, List<Mapping>> AllocationMapping = new Dictionary<int, List<Mapping>>(10000);

        /// <summary>
        /// Gives us access to all of the game's allocations and frees, as well as their functions.
        /// </summary>
        public MemoryTracer MemoryTracer;

        /// <summary>
        /// Provides an implementation of a dynamic .ONE memory allocator buffer for loading in the
        /// internal contents of .ONE files. The allocator's purpose is to allocates a new region of memory whenever
        /// the game
        /// </summary>
        public Allocator()
        {
            // Creates an instance of the memory tracer.
            MemoryTracer = new MemoryTracer();

            // Subscribe to the MemoryTracer's free function, we want to free our resources aswell.
            MemoryTracer.AfterFreeDelegate += AfterFreeDelegate;
        }

        /// <summary>
        /// Intended to be called manually when more memory is needed.
        /// Allocates a new address to a specified original address (if necessary), and adds it to the mapping of addresses.
        /// </summary>
        /// <param name="originalAddress">The original allocation address to map the allocation to.</param>
        /// <param name="size">The length of bytes to allocate.</param>
        /// <returns></returns>
        public int Allocate(int originalAddress, int size)
        {
            if (AllocationMapping.TryGetValue(originalAddress, out List<Mapping> memoryMappings))
            {
                // Mapping already exists, we need to check if an available buffer fits.
                for (int x = 0; x < memoryMappings.Count; x++)
                {
                    if (memoryMappings[x].Size >= size)
                        return (int)memoryMappings[x].Address;    // Return already available buffer.
                }

                // No available buffer already exists, we need to allocate one.
                void* newAddress = MemoryTracer.MallocFunction(size);
                memoryMappings.Add(new Mapping(newAddress, size));
                AllocationMapping[originalAddress] = memoryMappings;

                return (int) newAddress;
            }
            else
            {
                // No mappings exist, we need to create a new one.

                // Allocate
                void* newAddress = MemoryTracer.MallocFunction(size);

                // Create and assign list.
                memoryMappings = new List<Mapping>(5);
                Mapping memoryMapping = new Mapping(newAddress, size);
                memoryMappings.Add(memoryMapping);
                AllocationMapping[originalAddress] = memoryMappings;

                return (int)memoryMapping.Address;
            }
        }

        /// <summary>
        /// Returns the details of a specified allocated memory address from the internal
        /// <see cref="MemoryTracer"/> hook.
        /// </summary>
        /// <param name="address">The memory address to get the details for.</param>
        public MemoryAddressDetails GetAddressDetails(int address)
        {
            MemoryTracer.AllocationList.TryGetValue(address, out MemoryAddressDetails value);
            return value;
        }

        /// <summary>
        /// Our own delegate implementation responsible for freeing our own allocated additional arbitrary memory
        /// whenever the game chooses to free a mapped address.
        /// </summary>
        /// <param name="memoryTracer">The memoryTracer instance that called the Free Delegate.</param>
        /// <param name="address">The address that has been freed.</param>
        /// <param name="memoryAddressDetails">Any extra details about the address that has been freed.</param>
        private void AfterFreeDelegate(ref MemoryTracer memoryTracer, ref int address, ref MemoryAddressDetails memoryAddressDetails)
        {
            // Try to get our own memory mappings for game's memory, then free it.
            if (AllocationMapping.TryGetValue(address, out List<Mapping> ownMemory))
            {
                for (int x = 0; x < ownMemory.Count; x++)
                    MemoryTracer.FreeFunction(ownMemory[x].Address); 

                ownMemory.Clear();
            }
        }
    }
}
