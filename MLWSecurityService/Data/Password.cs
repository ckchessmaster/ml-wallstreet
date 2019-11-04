using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MLWSecurityService.Data
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

        public override int GetHashCode()
        {
            return HashCode.Combine(Hash, Salt);
        }
    }
}
