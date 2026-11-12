using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Models
{
    public class DuplicateSkillEditData : EditSkillData
    {
        public string ExistingSkill { get; set; } = string.Empty;

        public string ExistingLevel { get; set; } = string.Empty;
    }
   
}
