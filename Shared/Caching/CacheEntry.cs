using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.Caching
{
    public class CacheEntry
    {
        public string Key { get; set; }
        public byte[] Value { get; set; }
    }
}
