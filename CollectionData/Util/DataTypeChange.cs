using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionData.Util
{
    public class DataTypeChange
    {
        public static float Convert2Float(ushort[] ushorts2, bool order)
        {
            float[] floatData = new float[1];
            //order true 高位在前低位在后,order false高位在后，低位在前
            if (order)
                Buffer.BlockCopy(ushorts2, 0, floatData, 0, 4);
            else
                Buffer.BlockCopy(ushorts2.Reverse().ToArray(), 0, floatData, 0, 4);

            return floatData[0];
        }
    }
}
