using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Consul
{
    public class ValueEntry
    {
        public static List<ValueEntry> CreateFromJson( string json )
        {
            return JsonConvert.DeserializeObject<List<ValueEntry>>( json );
        }

        public int CreatedIndex { get; set; }
        public int Flags { get; set; }
        public string Key { get; set; }
        public int LockIndex { get; set; }
        public int ModifyIndex { get; set; }
        public string Value { get; set; }

        public string GetDecodedValue()
        {
            return Encoding.Default.GetString( Convert.FromBase64String( Value ) );
        }
    }
}
