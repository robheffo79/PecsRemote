using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Models
{
    public class Content
    {
        public Guid Id { get; set; }
        public String MimeType { get; set; }
        public String Filename { get; set; }
        public Int64 Size { get; set; }
    }
}
