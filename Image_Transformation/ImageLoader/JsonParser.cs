using Newtonsoft.Json;
using System.IO;

namespace Image_Transformation
{
    /// <summary>
    /// Reads the content of a json file and creates an object out of it.
    /// </summary>
    public class JsonParser
    {
        public static T Parse<T>(string path)
        {
            string fileContent = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(fileContent);
        }
    }
}