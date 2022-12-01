using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DumpToAzureWebJob
{
    public class DumpLog
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; } = null;
    }
}
