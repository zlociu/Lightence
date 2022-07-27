using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Models
{
    public class ProductKeyModel
    {
        [Key]
        [Required]
        [RegularExpression(@"^[A-Z]{25}$")]
        public string Key { get; set; }

        [Required]
        [Column("UseCount")]
        public int Uses { get; set; }

        [DataType(DataType.Date)]
        [Required]
        [Column("CreationDate")]
        public DateTime CreateDate { get; set; }
    }
}
