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

    public static class State
    {
        public static byte Active = 0;
        public static byte Redeemed = 1;
        public static byte Inactive = 2;

        public static string ConvertToString(byte state)
        {
            var stateString = "";

            switch (state)
            {
                case 0:
                    stateString = "Active";
                break;
                case 1:
                    stateString = "Redeemed";
                    break;
                case 2:
                    stateString = "Inactive";
                    break;
                default:
                    stateString = "";
                    break;
            }
            return stateString;
        }
    }
}