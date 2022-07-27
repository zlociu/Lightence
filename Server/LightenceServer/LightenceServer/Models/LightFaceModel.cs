using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LightenceServer.Models
{
    public class LightFaceModel
    {
        [Key]
        public string UserName { get; set; }

        [Required]
        [Column("Data")]
        public byte[] FaceData { get; set; }
    }
}
