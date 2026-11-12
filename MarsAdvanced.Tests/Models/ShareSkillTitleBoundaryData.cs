using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Models
{
    public class ShareSkillTitleBoundaryData : ShareSkillData
    {
        public int InputTitleLength { get; set; }

        public int ExpectedTitleLength { get; set; }
    }
}
