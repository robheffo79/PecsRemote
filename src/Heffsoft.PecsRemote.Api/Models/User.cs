using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    [Table("Users")]
    public class User
    {
        public Int32 Id { get; set; }
        public String Username { get; set; }
        public String HashedPassword { get; set; }
        public String Salt { get; set; }
    }
}
