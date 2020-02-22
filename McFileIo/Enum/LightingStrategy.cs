using System;
using System.Collections.Generic;
using System.Text;
using McFileIo.World;

namespace McFileIo.Enum
{
    /// <summary>
    /// Determine how block/skylight is recalculated if the chunk is altered
    /// </summary>
    public enum LightingStrategy
    {
        /// <summary>
        /// Choose the default value depending on the chunk type/version.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Default mode for <see cref="NamespacedChunk"/>.
        /// Light information will be removed.
        /// For post-1.13 versions, lighting will be calculated by Minecraft.
        /// For pre-1.13 versions, lighting may not render correctly and need manual intervention.
        /// Bukkit (Spigot, Catserver, etc) server may regenerate chunks incorrectly with this mode.
        /// Refer to bugtrack: <a href="https://bugs.mojang.com/browse/MC-133855">https://bugs.mojang.com/browse/MC-133855</a>
        /// </summary>
        RemoveExisting,

        /// <summary>
        /// Default mode for <see cref="ClassicChunk"/>.
        /// Copy the lighting data from the original chunk. Best for block-replacing without emissive/transparent blocks.
        /// You need to raise a light update in that chunk manually, in other conditions.
        /// </summary>
        CopyFromOldData,

        /// <summary>
        /// NOT IMPLEMENTED YET. DO NOT USE.
        /// Recalculate the light by McFileIo. Probably not as accurate as Minecraft.
        /// </summary>
        Recalculate
    }
}
