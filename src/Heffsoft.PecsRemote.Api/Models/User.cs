using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    public class User
    {
        public String Username { get; set; }
        public String HashedPassword { get; set; }
        public String Salt { get; set; }
        public String Roles { get; set; }
    }
}
