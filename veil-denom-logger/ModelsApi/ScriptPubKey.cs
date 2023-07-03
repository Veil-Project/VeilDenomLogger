using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeilBlockToDB.ModelsApi
{
    public class ScriptPubKey
    {
        public string asm { get; set; }
        public string hex { get; set; }
        public string type { get; set; }
    }
}
