using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Models
{
    public class ServerLogModel
    {
        [Key]
        [Column("Id")]
        public int ID { get; set; }

        public string? Description { get; set; }

        [Required]
        public ServerLogType Type { get; set; }

        [Required]
        public ServerLogResult Result { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Column("TimeStamp")]
        public DateTime Time { get; set; }
    }

    public enum ServerLogType
    {
        //user management
        LoginUser,
        RegisterUser,
        LogoutUser,
        DeleteUser,
        UpdateUser,
        ChangePass,

        // hub management
        CreateGroup,
        DeleteGroup,
        //other 
        Other,
        None = -1
    };

    public enum ServerLogResult
    {
        OK = 0,
        Error,
        Warning,
        Info,
        None = -1
    };
}
