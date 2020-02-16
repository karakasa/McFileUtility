using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    /// <summary>
    /// This interface is to provide interopability between general DynBitArray and specialized versions
    /// </summary>
    public interface IDynBitArray
    {
        int CellSize { get; }
        int Length { get; }
        int this[int index] { get; set; }
    }
}
