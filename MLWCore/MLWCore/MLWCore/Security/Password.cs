using System;
using System.Collections.Generic;
using System.Text;

namespace MLWCore.Security
{
    public class Password
    {
        public string Hash { get; set; }

        public string Salt { get; set; }

        public override bool Equals(object obj)
        {
            var password = obj as Password;

            if (password == null)
            {
                return false;
            }

            if (password.Hash.Equals(Hash))
            {
                return true;
            }

            return false;
        }
    }
}
