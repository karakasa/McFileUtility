using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces.McObject
{
    public interface ILootTableCapable
    {
        string LootTable { get; set; }
        long LootTableSeed { get; set; }
    }
}
