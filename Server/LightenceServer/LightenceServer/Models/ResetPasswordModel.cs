using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Models
{
    public class ResetPasswordModel
    {
        public string UserName { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
