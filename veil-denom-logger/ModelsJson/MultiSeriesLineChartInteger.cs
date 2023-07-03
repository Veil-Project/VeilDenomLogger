using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeilBlockToDB.ModelsJson
{
    public class MultiSeriesLineChartInteger
    {
        public MultiSeriesLineChartInteger() {
            Series1 = new List<LineGraphDataPointInteger>();
            Series2 = new List<LineGraphDataPointInteger>();
        }

        public string LastBlockTime { get; set; }
        public List<LineGraphDataPointInteger> Series1 { get; set; }
        public List<LineGraphDataPointInteger> Series2 { get; set; }
    }
}
