using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Models
{
    public class SearchPaginationData
    {
        public int ListingCount { get; set; }

        public int PageSize { get; set; }

        public ShareSkillData Listing { get; set; } = new();
    }
}
