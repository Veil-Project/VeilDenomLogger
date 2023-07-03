using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeilBlockToDB.ModelsApi
{
    public class ZerocoinSupply
    {
        public string denom { get; set; }
        public Int64 amount { get; set; }
        public decimal amount_formatted { get; set; }
        public decimal percent { get; set; }
    }

}
