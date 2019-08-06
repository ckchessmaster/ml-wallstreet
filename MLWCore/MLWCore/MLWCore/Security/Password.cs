using System;
using System.Collections.Generic;
using System.Text;

namespace MLWCore.Security
{
    public class Password
    {
        public string Hash { get; set; }

        public string Salt { get; set; }
    }
}
