using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks.Definitions
{
    public class Grass : Block
    {
        public static readonly Guid BuiltInUniqueId = new Guid("{094DEBF1-8E98-4D1B-9600-DEDC48CC9D6A}");
        internal Grass() : base(BuiltInUniqueId)
        {
        }

        // TODO: Snowy feature
    }
}
