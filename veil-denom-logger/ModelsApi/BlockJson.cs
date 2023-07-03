using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeilBlockToDB.ModelsApi
{
    public class BlockJson
    {
        public long BlockID { get; set; }
        public int BlockType { get; set; }
        public long BlockTimestamp { get; set; }
        public DateTime BlockDate { get; set; }
        public string BlockHash { get; set; }
        public decimal PosDiff { get; set; }
        public decimal PowDiff { get; set; }
        public int PowAlgoType { get; set; }
        public int TxCount { get; set; }
    }
}
