using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Models
{
    public class VisionLoginModel
    {
        [Required]
        public byte[] data { get; set; }

        [Required]
        [EmailAddress]
        public string userName { get; set; }    
    }
}
