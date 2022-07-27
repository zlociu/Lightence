using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Models
{
    public class KeyModel
    {
        [Required]
        [RegularExpression(@"^(([A-Z]{25})|([A-Z]{5}-[A-Z]{5}-[A-Z]{5}-[A-Z]{5}-[A-Z]{5}))$")]
        public string Appkey { get; set; }
    }
}
