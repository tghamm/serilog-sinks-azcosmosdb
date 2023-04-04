using Microsoft.Azure.Cosmos;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Serilog.Sinks.AzCosmosDB.Sinks.AzCosmosDb
{
    public class SystemTextJsonCosmosSerializer : CosmosSerializer
    {
        public delegate void LogJsonErrorDelegate(object sender, JsonException exception, object state);

        public event LogJsonErrorDelegate LogJsonError;

        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        private readonly JsonSerializerOptions _options;

        public SystemTextJsonCosmosSerializer(JsonSerializerOptions options)
        {
            _options = options;
        }

        public override T FromStream<T>(Stream stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            try
            {
                return JsonSerializer.DeserializeAsync<T>(stream, _options).AsTask().GetAwaiter().GetResult();
            }
            catch (JsonException ex)
            {
                LogJsonError?.Invoke(this, ex, typeof(T));
                return default;
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            using (var streamWriter = new StreamWriter(streamPayload, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
            {
                try
                {
                    JsonSerializer.SerializeAsync(streamWriter.BaseStream, input, _options).GetAwaiter().GetResult();
                    streamWriter.Flush();
                }
                catch (JsonException ex)
                {
                    LogJsonError?.Invoke(this, ex, typeof(T));
                }
            }

            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}
