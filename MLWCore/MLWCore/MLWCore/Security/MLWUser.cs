using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLWCore.Security
{
    public class MLWUser : IdentityUser
    {
        public bool IsActive { get; set; }
    }
}
