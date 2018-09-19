
<div align="center">
	<h1>Project Reloaded (Mod Template)</h1>
	<img src="https://i.imgur.com/BjPn7rU.png" width="150" align="center" />
	<br/> <br/>
	<strong>All your mods are belong to us.</strong>
	<p>C# based universal mod loader framework compatible with arbitrary processes.</p>
</div>


# About This Project

Despite the humorous name, and humorous approach of TONER; it is actually one of the most advanced and useful mods available for Sonic Heroes. It allows us to break the limits of the sizes of files we are allowed to store inside .ONE Archives on an individual file by file basis. 

Basically, this project serves as an implementation of a dynamic memory allocator; used should there be a possibility that the space the game allocates for an individual file to be extracted is insufficient. The project makes use of hooking to know the allocated memory sizes at a specific address, and when to deallocate our own allocated memory to prevent leaks.

Under the hood, it makes use of my own PRS Compression/Decompression library written in D; albeit with its C# frontend.

*(In short, it allows you to place very large files inside .ONE Archives, something normally not possible)*

## How to Use

A. Install TONER like a regular Reloaded mod.
B. Enable TONER.

## Under the Hood: A Quick Summary

This summary is short and non-exhaustive; the only purpose is to cover the basic principles or methods using which TONER functions.

A. TONER hooks the game's own `malloc` and `free` functions used to manage memory where potential files from inside .ONE archives inside the `MemoryTracer` class. The `MemoryTracer` class maintains an active log of all of the memory allocations performed by the game.

B. TONER also implements its own `Allocator` class, which makes use of the `MemoryTracer` class. The purpose of the `Allocator` class is to maintain a mapping (dictionary) of game allocated addresses to our own allocated addresses (See [1]). In addition the `free` hook in `MemoryTracer` also allows us to synchronize memory deallocations.

C. The game contains a set of functions, each used to load a specific file type from a ONE Archive. TONER hooks each of these functions, uses my D based PRS Compression/Decompression library to estimate if a specific file would fit into the game's preallocated buffer. If the file is too big for the game allocated buffer, `Allocator` class is used to acquire a new buffer address.

The actual implementation is a bit more complicated than summarized, but the summary should be sufficient for anyone to get started understanding TONER.

As usual, the most difficult part of a project like this was reverse engineering in order to gain a complete understanding of how the game code used to interact with the .ONE files functions under the hood.

### Notes

[1] The mapping is one game allocated address to many own addresses, as the game can sometimes reuse some memory addresses for loading files. It is possible that, when an address is used a second time, a file may be insufficient for our custom mapped buffer we allocated the first time. (e.g. 4MB file load needing our own custom buffer, then 100MB file load).
