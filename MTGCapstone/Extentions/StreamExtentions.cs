using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace MTGCapstone.API.Extentions
{
    public static class StreamExtentions
    {
        public static T? ReadAndDeserializeFromJson<T>(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new NotSupportedException("Can't read from this stream.");
            }

            using var streamReader = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(streamReader);
            var jsonSerializer = new JsonSerializer();
            return jsonSerializer.Deserialize<T>(jsonTextReader);


        }
    }
}
