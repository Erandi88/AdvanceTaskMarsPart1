using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Models
{
    public class EditLanguageLimitData
    {
        public List<LanguageData> Languages { get; set; } = new();

        public EditLanguageData RecordToEdit { get; set; } = new();
    }
}
