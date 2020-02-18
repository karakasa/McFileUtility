using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    public static class NumericUtility
    {
        public static int GetRequiredBitLength(int num)
        {
            --num;

            var cnt = 0;
            while (num != 0)
            {
                num >>= 1;
                cnt++;
            }

            return cnt;
        }
    }
}
