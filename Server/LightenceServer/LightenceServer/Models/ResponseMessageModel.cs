using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Models
{
    public class ResponseMessageModel
    {
        public string? Error { get; set; }
        public object? Content { get; set; }

    }
}
