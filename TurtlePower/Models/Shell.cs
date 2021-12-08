using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Turtle.Models
{
    public class Shell
    {
        public List<KeyValuePair<string, string>> Headers { get; set; }
        public dynamic Body { get; set; }
    }
}
