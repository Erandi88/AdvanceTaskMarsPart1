using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarsAdvanced.Tests.Helpers
{
    public static class JsonReader
    {
        public static T Read<T>(string filePath)
        {
            string json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(json, options)
                ?? throw new InvalidOperationException(
                    $"Could not read test data from '{filePath}'."
                );
        }
    }
}
