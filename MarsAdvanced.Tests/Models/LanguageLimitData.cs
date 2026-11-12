using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Models
{
    public class LanguageLimitData
    {
        public List<LanguageData> Languages { get; set; } = new();

        public string LanguageToDelete { get; set; } = string.Empty;
    }
}
