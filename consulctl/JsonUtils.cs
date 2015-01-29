using Newtonsoft.Json;

namespace Consulctl
{
    public static class JsonUtils
    {
        public static string GetPrettyPrintedJson( string json )
        {
            dynamic parsedJson = JsonConvert.DeserializeObject( json );
            return JsonConvert.SerializeObject( parsedJson, Formatting.Indented );
        }

        public static string GetPrettyPrintedJsonFromObject( object value )
        {
            return JsonConvert.SerializeObject( value, Formatting.Indented );
        }
    }
}
