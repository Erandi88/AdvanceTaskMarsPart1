using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Models
{
    public class EditSkillIsolationData
    {
        public EditSkillData RecordToEdit { get; set; } = new();

        public SkillData OtherRecord { get; set; } = new();
    }
}
