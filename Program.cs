using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using csharp_prs;
using Reloaded;
using Reloaded.Assembler;
using Reloaded.Process;
using Reloaded.Process.Functions.X86Hooking;
using Reloaded.Process.Memory;
using Reloaded_Mod_Template.Heroes;
using Reloaded_Mod_Template.Heroes.Classes;
using Reloaded_Mod_Template.Heroes.Custom.Memory.Allocator;
using Reloaded_Mod_Template.Heroes.Custom.Memory.Tracer;
using Reloaded_Mod_Template.Heroes.Custom.ONEFILE;
using Reloaded_Mod_Template.Heroes.Custom.ONEFILE.Substructures;
using static Reloaded_Mod_Template.Heroes.Classes.ONEFILE;

namespace Reloaded_Mod_Template
{
    public static unsafe class Program
    {
        #region Mod Loader Template Description & Explanation | Your first time? Read this.
        /*
         *  Reloaded Mod Loader DLL Modification Template
         *  Sewer56, 2018 ©
         *
         *  -------------------------------------------------------------------------------
         *
         *  Here starts your own mod loader DLL code.
         *
         *  The Init function below is ran at the initialization stage of the game.
         *
         *  The game at this point suspended and frozen in memory. There is no execution
         *  of game code currently ongoing.
         *
         *  This is where you do your hot-patches such as graphics stuff, patching the
         *  window style of the game to borderless, setting up your initial variables, etc.
         *
         *  -------------------------------------------------------------------------------
         *
         *  Important Note:
         *
         *  This function is executed once during startup and SHOULD return as the
         *  mod loader awaits successful completion of the main function.
         *
         *  If you want your mod/code to sit running in the background, please initialize
         *  another thread and run your code in the background on that thread, otherwise
         *  please remember to return from the function.
         *
         *  There is also some extra code, including DLL stubs for Reloaded, classes
         *  to interact with the Mod Loader Server as well as other various loader related
         *  utilities available.
         *
         *  -------------------------------------------------------------------------------
         *  Extra Tip:
         *
         *  For Reloaded mod development, there are also additional libraries and packages
         *  available on NuGet which provide you with extra functionality.
         *
         *  Examples include:
         *  [Input] Reading controller information using Reloaded's input stack.
         *  [IO] Accessing the individual Reloaded config files.
         *  [Overlays] Easy to use D3D and external overlay code.
         *
         *  Simply search libReloaded on NuGet to find those extras and refer to
         *  Reloaded-Mod-Samples subdirectory on Github for examples of using them (and
         *  sample mods showing how Reloaded can be used).
         *
         *  -------------------------------------------------------------------------------
         *
         *  [Template] Brief Walkthrough:
         *
         *  > ReloadedTemplate/Initializer.cs
         *      Stores Reloaded Mod Loader DLL Template/Initialization Code.
         *      You are not required/should not (need) to modify any of the code.
         *
         *  > ReloadedTemplate/Client.cs
         *      Contains various pieces of code to interact with the mod loader server.
         *
         *      For convenience it's recommended you import Client static(ally) into your
         *      classes by doing it as such `Reloaded_Mod_Template.Reloaded_Code.Client`.
         *
         *      This will avoid you typing the full class name and let you simply type
         *      e.g. Print("SomeTextToConsole").
         *
         *  -------------------------------------------------------------------------------
         *
         *  If you like Reloaded, please consider giving a helping hand. This has been
         *  my sole project taking up most of my free time for months. Being the programmer,
         *  artist, tester, quality assurance, alongside various other roles is pretty hard
         *  and time consuming, not to mention that I am doing all of this for free.
         *
         *  Well, alas, see you when Reloaded releases.
         *
         *  Please keep this notice here for future contributors or interested parties.
         *  If it bothers you, consider wrapping it in a #region.
        */
        #endregion Mod Loader Template Description

        #region Mod Template Default Variables

        /// <summary>
        /// Holds the game process for us to manipulate.
        /// Allows you to read/write memory, perform pattern scans, etc.
        /// See libReloaded/GameProcess (folder)
        /// </summary>
        public static ReloadedProcess GameProcess;

        /// <summary>
        /// Stores the absolute executable location of the currently executing game or process.
        /// </summary>
        public static string ExecutingGameLocation;

        /// <summary>
        /// Specifies the full directory location that the current mod 
        /// is contained in.
        /// </summary>
        public static string ModDirectory;

        #endregion Mod Template Default Variables

        /// <summary>
        /// The allocator object allows us to allocate an alternative buffer of memory for loading
        /// of data into.
        /// </summary>
        public static Allocator Allocator;

        public static FunctionHook<OneFileLoadHAnimation>           OneFileLoadHAnimationHook;
        public static FunctionHook<OneFileLoadClump>                OneFileLoadClumpHook;
        public static FunctionHook<OneFileLoadTextureDictionary>    OneFileLoadTextureDictionaryHook;
        public static FunctionHook<OneFileLoadSpline>               OneFileLoadSplineHook;
        public static FunctionHook<OneFileLoadDeltaMorph>           OneFileLoadDeltaMorphHook;
        public static FunctionHook<OneFileLoadWorld>                OneFileLoadWorldHook;
        public static FunctionHook<OneFileLoadUVAnim>               OneFileLoadUVAnimHook;
        public static FunctionHook<OneFileLoadMaestro>              OneFileLoadMaestroHook;
        public static FunctionHook<OneFileLoadCameraTmb>            OneFileLoadCameraTmbHook;

        /// <summary>
        /// Your own user code starts here.
        /// If this is your first time, do consider reading the notice above.
        /// It contains some very useful information.
        /// </summary>
        public static unsafe void Init()
        {
            #if DEBUG
            Debugger.Launch();
            #endif

            // Create new allocator.
            Allocator = new Allocator();

            // :3
            Bindings.PrintWarning("Initializing: The ONE Archive Expandonger");

            OneFileLoadHAnimationHook = FunctionHook<OneFileLoadHAnimation>.Create(0x0042F600, OneFileLoadHAnimationImpl).Activate();
            OneFileLoadClumpHook = FunctionHook<OneFileLoadClump>.Create(0x0042F440, OneFileLoadClumpImpl).Activate();
            OneFileLoadTextureDictionaryHook = FunctionHook<OneFileLoadTextureDictionary>.Create(0x0042F3C0, OneFileLoadTextureDictionaryImpl).Activate();
            OneFileLoadSplineHook = FunctionHook<OneFileLoadSpline>.Create(0x0042F4B0, OneFileLoadSplineImpl).Activate();
            OneFileLoadDeltaMorphHook = FunctionHook<OneFileLoadDeltaMorph>.Create(0x0042F520, OneFileLoadDeltaMorphImpl).Activate();
            OneFileLoadWorldHook = FunctionHook<OneFileLoadWorld>.Create(0x0042F590, OneFileLoadWorldImpl).Activate();
            OneFileLoadUVAnimHook = FunctionHook<OneFileLoadUVAnim>.Create(0x0042F670, OneFileLoadUVAnimImpl).Activate();
            OneFileLoadMaestroHook = FunctionHook<OneFileLoadMaestro>.Create(0x0042F6F0, OneFileLoadMaestroImpl).Activate();
            OneFileLoadCameraTmbHook = FunctionHook<OneFileLoadCameraTmb>.Create(0x0042F770, OneFileLoadCameraTmbImpl).Activate();

            Bindings.PrintText("Your .ONE Archives have now been expandonged!");
            GC.Collect();
        }

        /// <summary>
        /// Checks whether the buffer size for a file about to be loaded is sufficient and
        /// conditonally returns an address to a new buffer with adequate size.
        /// </summary>
        /// <returns>The address of a new buffer for the game to decompress a file to</returns>
        private static void* CheckBufferSize(int fileIndex, void* addressToDecompressTo, ONEFILE* thisPointer)
        {
            if (fileIndex >= 2)
            {
                // Get pointer and length.
                IntPtr onePointer = (IntPtr)thisPointer[0].InternalDataPointer - 0xC; // The start of file pointer is sometimes unused, so we use the InternalDataPointer instead and offset.
                int fileLength = thisPointer[0].FileLength;

                // Now the ONE File
                MemoryONEArchive memoryOneFile = MemoryONEArchive.ParseONEFromMemory(onePointer, fileLength);

                // Now we estimate the size of it.
                int actualFileIndex = fileIndex - 2;
                MemoryONEFile oneFile = memoryOneFile.Files[actualFileIndex];
                byte[] oneFileCopy = GameProcess.ReadMemory((IntPtr)oneFile.CompressedDataPointer, oneFile.DataLength);
                int oneFileLength = Prs.Estimate(ref oneFileCopy);

                // Check if the size of allocation is sufficient.
                MemoryAddressDetails addressDetails = Allocator.GetAddressDetails((int)addressToDecompressTo);

                if (addressDetails.MemorySize < oneFileLength)
                {
                    // Allocate some new data for me please
                    addressToDecompressTo = (void*)Allocator.Allocate((int)addressToDecompressTo, oneFileLength);
                }
            }

            return addressToDecompressTo;
        }


        /*
            --------------------
            Hook Implementations 
            --------------------
        */

        private static int OneFileLoadHAnimationImpl(int fileIndex, void* addressToDecompressTo, ONEFILE* thisPointer)
        {
            addressToDecompressTo = CheckBufferSize(fileIndex, addressToDecompressTo, thisPointer);
            return OneFileLoadHAnimationHook.OriginalFunction(fileIndex, addressToDecompressTo, thisPointer);
        }

        private static int OneFileLoadClumpImpl(int fileIndex, void* addressToDecompressTo, ONEFILE* thisPointer)
        {
            addressToDecompressTo = CheckBufferSize(fileIndex, addressToDecompressTo, thisPointer);
            return OneFileLoadClumpHook.OriginalFunction(fileIndex, addressToDecompressTo, thisPointer);
        }

        private static int OneFileLoadTextureDictionaryImpl(int fileIndex, void* addressToDecompressTo, ONEFILE* thisPointer)
        {
            addressToDecompressTo = CheckBufferSize(fileIndex, addressToDecompressTo, thisPointer);
            return OneFileLoadTextureDictionaryHook.OriginalFunction(fileIndex, addressToDecompressTo, thisPointer);
        }

        private static int OneFileLoadSplineImpl(int fileIndex, void* addressToDecompressTo, ONEFILE* thisPointer)
        {
            addressToDecompressTo = CheckBufferSize(fileIndex, addressToDecompressTo, thisPointer);
            return OneFileLoadSplineHook.OriginalFunction(fileIndex, addressToDecompressTo, thisPointer);
        }

        private static int OneFileLoadDeltaMorphImpl(int fileIndex, void* addressToDecompressTo, ONEFILE* thisPointer)
        {
            addressToDecompressTo = CheckBufferSize(fileIndex, addressToDecompressTo, thisPointer);
            return OneFileLoadDeltaMorphHook.OriginalFunction(fileIndex, addressToDecompressTo, thisPointer);
        }

        private static int OneFileLoadWorldImpl(int fileIndex, void* addressToDecompressTo, ONEFILE* thisPointer)
        {
            addressToDecompressTo = CheckBufferSize(fileIndex, addressToDecompressTo, thisPointer);
            return OneFileLoadWorldHook.OriginalFunction(fileIndex, addressToDecompressTo, thisPointer);
        }

        private static int OneFileLoadUVAnimImpl(int fileIndex, void* addressToDecompressTo, ONEFILE* thisPointer)
        {
            addressToDecompressTo = CheckBufferSize(fileIndex, addressToDecompressTo, thisPointer);
            return OneFileLoadUVAnimHook.OriginalFunction(fileIndex, addressToDecompressTo, thisPointer);
        }

        private static int OneFileLoadMaestroImpl(void* addressToDecompressTo, ONEFILE* thisPointer, int fileIndex)
        {
            addressToDecompressTo = CheckBufferSize(fileIndex, addressToDecompressTo, thisPointer);
            return OneFileLoadMaestroHook.OriginalFunction(addressToDecompressTo, thisPointer, fileIndex);
        }

        private static ONEFILE* OneFileLoadCameraTmbImpl(int fileIndex, void* addressToDecompressTo, ONEFILE* thisPointer)
        {
            addressToDecompressTo = CheckBufferSize(fileIndex, addressToDecompressTo, thisPointer);
            return OneFileLoadCameraTmbHook.OriginalFunction(fileIndex, addressToDecompressTo, thisPointer);
        }
    }
}
