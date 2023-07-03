using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeilBlockToDB.ModelsApi
{
    public class Vin
    {
        public string txid { get; set; }
        public long vout { get; set; }
        public long n { get; set; }
        public string type { get; set; }
        public string serial { get; set; }
        public decimal denomination { get; set; }
        public List<object> addresses { get; set; }
        public double value { get; set; }
    }
}
