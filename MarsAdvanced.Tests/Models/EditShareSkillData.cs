using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Models
{
    public class EditShareSkillData
    {
        // Complete data used to create the listing before editing.
        public ShareSkillData Original { get; set; } = new();

        // New values applied during the edit.
        public string UpdatedTitle { get; set; } = string.Empty;

        public string UpdatedDescription { get; set; } = string.Empty;
    }
}
