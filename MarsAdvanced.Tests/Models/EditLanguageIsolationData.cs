using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Models
{
    public class EditLanguageIsolationData
    {
        public EditLanguageData RecordToEdit { get; set; } = new();

        public LanguageData OtherRecord { get; set; } = new();
    }
}
