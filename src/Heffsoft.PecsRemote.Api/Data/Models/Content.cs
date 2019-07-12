using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heffsoft.PecsRemote.Api.Data.Models
{
    [Table("Content")]
    public class Content
    {
        public Guid Id { get; set; }
        public String MimeType { get; set; }
        public String Filename { get; set; }
        public Int64 Size { get; set; }
    }
}
