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
            var page = pageNumber;

            if(page > 0)
            {
                page *= pageSize;
            }
            return page;
        }
    }
}