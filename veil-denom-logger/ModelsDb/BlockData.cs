using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeilBlockToDB.ModelsDb
{
    [Table("BlockData", Schema = "bak")]
    public class BlockData
    {
        private int _iSupplyDivisor = 100000000;
        private decimal _dMoneySupply = 0;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long XID { get; set; }
        public long BlockID { get; set; }
        public int BlockType { get; set; }
        public long BlockTimestamp { get; set; }
        public DateTime BlockDate { get; set; }
        public string BlockHash { get; set; }
        public double PoWDiff { get; set; }
        public double PoSDiff { get; set; }
        public int TxCount { get; set; }
        public long MoneySupply { get; set; }       
    }
}