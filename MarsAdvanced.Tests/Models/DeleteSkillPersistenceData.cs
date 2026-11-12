using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Models
{
    public class DeleteSkillPersistenceData
    {
        public SkillData RecordToDelete { get; set; } = new();

        public SkillData OtherRecord { get; set; } = new();
    }
}
