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

    public class Campaign
    {
        public int ID { get; set; }
        public string CampaignName { get; set; }
        public int CodeIDStart { get; set; }
        public int CodeIDEnd { get; set; }
        public int CampaignSize { get; set; }
    }
}