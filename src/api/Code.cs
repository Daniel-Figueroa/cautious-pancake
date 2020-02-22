using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodeFlip.CodeJar.Api
{
    public class Code
    {
        public string StringValue { get; set; }
        public string State { get; set; }
    }
}