# McFileIo - A fully native C# library for Minecraft file I/O

**Notice: Interfaces of this library is not stable yet, and it can change without any notice.**

Currently support is implemented for:

* Region files (writing back to Region file is not supported yet)
* Chunk, including pre-1.13 ID blocks, and post-1.13 namespaced blocks, for
    - Basic information
	- Block getter/setter
	- Heightmap
	- Block entities (a.k.a. TileEntities)

Pre-anvil files are not supported yet.

The library is tweaked for performance. It only takes about 200 seconds to traverse all blocks (not all chunks) of a 2GB-sized map.

See other projects for examples.