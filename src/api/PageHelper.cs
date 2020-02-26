using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;

namespace CodeFlip.CodeJar.Api
{
    public static class Pagination
    {
        public static int PaginationPageNumber(int pageNumber, int pageSize)
        {
            var p = pageNumber;

             p--;

            if(p > 0)
            {
                p *= pageSize;
            }
            return p;
        }
    }
}