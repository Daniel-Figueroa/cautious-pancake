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
        public TableData(List<Code> codes, int pages, int page)
        {
            Codes = new List<Code>(codes);
            Pages = pages;
            Page = page;
        }
        public List<Code> Codes {get;set;} 

        public int Pages {get;set;}

        public int Page{get;set;}
    }
}