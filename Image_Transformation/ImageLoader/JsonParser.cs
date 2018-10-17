using Newtonsoft.Json;
using System.IO;

namespace Image_Transformation
{
    public class JsonParser
    {
        public static T Parse<T>(string path)
        {
            string fileContent = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(fileContent);
        }
    }
}