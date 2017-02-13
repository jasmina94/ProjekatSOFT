using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRTest
{
    public class LevenstainStaticDataValidator
    {

        public int ComputeLevensteinDistance(string val1, string val2)
        {
            int result = 0;

            result = LevensteinDistance.Compute(val1, val2);

            return result;
        }
    }
}
