using System.Collections.Generic;
using System.Runtime.InteropServices;
using Reloaded.Process.Functions.X86Functions;
using Reloaded.Process.Functions.X86Hooking;

namespace Reloaded_Mod_Template.Heroes.Custom.Memory.Tracer
{
    /// <summary>
    /// This class performs automatic hooking of Sonic Heroes' copy of
    /// free and malloc and maintains an active list of all currently allocated
    /// memory addresses. This class exposes events to which you may subscribe that
    /// are called after running the original malloc/free functions.
    /// </summary>
    public unsafe class MemoryTracer
    {
        /// <summary>
        /// Maintains an active list of allocations used.
        /// </summary>
        public Dictionary<int, MemoryAddressDetails> AllocationList = new Dictionary<int, MemoryAddressDetails>(25000);

        /*
            Hooks and functions. 
        */

        /// <summary>
        /// Exposes the game's original Malloc function.
        /// This function is not piped through the hook.
        /// </summary>
        public Malloc MallocFunction;

        /// <summary>
        /// Exposes the game's original Free function.
        /// This function is not piped through the hook.
        /// </summary>
        public Free FreeFunction;

        private FunctionHook<Malloc> _mallocHook;
        private FunctionHook<Free> _freeHook;

        /*
            Delegates 
        */

        /// <summary>
        /// This delegate is executed after the hook calls Malloc and the <see cref="AllocationList"/>
        /// has been updated. 
        /// </summary>
        public AfterMalloc AfterMallocDelegate;

        /// <summary>
        /// This delegate is executed before the hook calls Malloc and allows you to set 
        /// </summary>
        public Malloc BeforeMallocDelegate;

        /// <summary>
        /// This delegate is executed after the hook calls Free but and <see cref="AllocationList"/>
        /// updates.
        /// </summary>
        public AfterFree AfterFreeDelegate;

        [ReloadedFunction(CallingConventions.Cdecl)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void* Malloc(int bytes);

        [ReloadedFunction(CallingConventions.Cdecl)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Free(void* memoryAddress);

        public delegate void AfterMalloc(ref MemoryTracer memoryTracer, ref int memoryAddress, ref MemoryAddressDetails memoryAddressDetails);
        public delegate void AfterFree(ref MemoryTracer memoryTracer, ref int address, ref MemoryAddressDetails memoryAddressDetails);

        /// <summary>
        /// For class definition, see Class Summary <see cref="MemoryTracer"/>.
        /// Hooks Sonic Heroes' free & malloc functions and exposes delegates which
        /// you may use to either alter the parameters of the function or act.
        /// </summary>
        public MemoryTracer()
        {
            // Hook the game functions.
            _mallocHook = FunctionHook<Malloc>.Create(0x0067B475, MallocHookImpl).Activate();
            _freeHook = FunctionHook<Free>.Create(0x0067B35D, FreeHookImpl).Activate();
            MallocFunction = _mallocHook.OriginalFunction;
            FreeFunction = _freeHook.OriginalFunction;
        }

        /// <summary>
        /// Implements the hook for Sonic Heroes' free function, removing the address allocated from the set and
        /// calling any other necessary delegates.
        /// </summary>
        /// <param name="memoryAddress">The memory address to be freed from memory.</param>
        private void FreeHookImpl(void* memoryAddress)
        {
            // Call original deallocator.
            _freeHook.OriginalFunction(memoryAddress);

            // They can sometimes pass 0 in.
            if (AllocationList.ContainsKey((int) memoryAddress))
            {
                // Call subscribed delegates.
                MemoryTracer thisTracer = this;
                int memoryAddressCopy = (int)memoryAddress;
                MemoryAddressDetails addressDetails = AllocationList[memoryAddressCopy];

                // Remove from dictionary and call delegate.
                AllocationList.Remove((int)memoryAddress);
                AfterFreeDelegate?.Invoke(ref thisTracer, ref memoryAddressCopy, ref addressDetails);
            }
        }

        /// <summary>
        /// Implements the hook for Sonic Heroes' malloc function, adding the address allocated to the set and
        /// calling any other necessary delegates.
        /// </summary>
        /// <param name="bytes">The amount of bytes to be allocated.</param>
        /// <returns>The memory address of allocation</returns>
        private void* MallocHookImpl(int bytes)
        {
            // Call our own delegate and get potential new return value.
            if (BeforeMallocDelegate != null)
            {
                void* potentialResult = BeforeMallocDelegate(bytes);
                if ((int)potentialResult > 0)
                    return potentialResult;
            }


            // Call original allocator.
            void* newMemoryLocation =_mallocHook.OriginalFunction(bytes);

            // Some extra prep.
            int newMemoryLocationCopy = (int)newMemoryLocation;
            var memoryAddressDetails = new MemoryAddressDetails(bytes);

            // Add to dictionary.
            AllocationList[newMemoryLocationCopy] = memoryAddressDetails;

            // Call subscribed delegates.
            MemoryTracer thisTracer = this;
            AfterMallocDelegate?.Invoke(ref thisTracer, ref newMemoryLocationCopy, ref memoryAddressDetails);

            // Return
            return newMemoryLocation;
        }
    }
}
