using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Models
{
    public class ShareSkillDescriptionBoundaryData : ShareSkillData
    {
        public int InputDescriptionLength { get; set; }

        public int ExpectedDescriptionLength { get; set; }
    }
}
