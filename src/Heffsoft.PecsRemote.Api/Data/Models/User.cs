using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heffsoft.PecsRemote.Api.Data.Models
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
