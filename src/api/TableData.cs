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
    public class TableData
    {
        public TableData(List<Code> codes, int pages)
        {
            Codes = codes;
            Pages = pages;
        }
        public List<Code> Codes {get;set;} = new List<Code>();

        public int Pages {get;set;}
    }
}