using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket_Programming_Server_Side
{
    public class Message
    {
        public byte[] MessageContent { get; set; }
        public string ToAddress { get; set; }
    }
}
