using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using BaGet.Protocol.Models;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace JsonPlayground
{
    [MemoryDiagnoser]
    public class Json
    {
        private static readonly Newtonsoft.Json.JsonSerializer Serializer = Newtonsoft.Json.JsonSerializer.Create(JsonSettings);
        private static readonly Newtonsoft.Json.JsonSerializerSettings JsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
            DateParseHandling = Newtonsoft.Json.DateParseHandling.DateTimeOffset,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
        };

        private readonly MemoryStream _jsonStream;
        private readonly byte[] _jsonBytes;

        public Json()
        {
            _jsonStream = new MemoryStream();

            using (var writer = new StreamWriter(_jsonStream, leaveOpen: true))
            {
                writer.Write(TestData.CatalogPage);
            }

            _jsonBytes = Encoding.UTF8.GetBytes(TestData.CatalogPage);
        }

        [GlobalSetup]
        public void Setup() => _jsonStream.Position = 0;

        [Benchmark]
        public CatalogPage NewtonsoftJson()
        {
            using (var textReader = new StreamReader(_jsonStream, leaveOpen: true))
            using (var jsonReader = new Newtonsoft.Json.JsonTextReader(textReader))
            {
                return Serializer.Deserialize<CatalogPage>(jsonReader);
            }
        }

        [Benchmark]
        public CatalogPage SystemTextJson()
        {
            var reader = new Utf8JsonReader(_jsonBytes);

            return reader.GetCatalogPage();
        }
    }

    public static class Utf8JsonReaderExtensions
    {
        private static readonly JsonEncodedText AtIdName = JsonEncodedText.Encode("@id");
        private static readonly JsonEncodedText AtTypeName = JsonEncodedText.Encode("@type");
        private static readonly JsonEncodedText CommitTimestampName = JsonEncodedText.Encode("commitTimeStamp");
        private static readonly JsonEncodedText CountName = JsonEncodedText.Encode("count");
        private static readonly JsonEncodedText ItemsName = JsonEncodedText.Encode("items");
        private static readonly JsonEncodedText NuGetIdName = JsonEncodedText.Encode("nuget:id");
        private static readonly JsonEncodedText NuGetPackageDeleteName = JsonEncodedText.Encode("nuget:PackageDelete");
        private static readonly JsonEncodedText NuGetPackageDetailsName = JsonEncodedText.Encode("nuget:PackageDetails");
        private static readonly JsonEncodedText NuGetVersionName = JsonEncodedText.Encode("nuget:version");

        public static CatalogPage GetCatalogPage(ref this Utf8JsonReader reader)
        {
            var result = new CatalogPage();
            
            Assert(reader.Read());
            //Assert(reader.TokenType == JsonTokenType.StartObject);
            Assert(reader.Read());

            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals(AtIdName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    //Assert(reader.TokenType == JsonTokenType.String);
                    result.CatalogIndexUrl = reader.GetString();
                }
                else if (reader.ValueTextEquals(CountName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    //Assert(reader.TokenType == JsonTokenType.Number);
                    result.Count = reader.GetInt32();
                }
                else if (reader.ValueTextEquals(CommitTimestampName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    //Assert(reader.TokenType == JsonTokenType.String);
                    Assert(reader.TryGetDateTimeOffset(out var commitTimestamp));
                    result.CommitTimestamp = commitTimestamp;
                }
                else if (reader.ValueTextEquals(ItemsName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    //Assert(reader.TokenType == JsonTokenType.StartArray);
                    Assert(reader.Read());
                    result.Items = new List<CatalogLeafItem>();

                    while (reader.TokenType == JsonTokenType.StartObject)
                    {
                        result.Items.Add(reader.GetCatalogLeafItem());

                        //Assert(reader.TokenType == JsonTokenType.EndObject);
                        Assert(reader.Read());
                    }

                    //Assert(reader.TokenType == JsonTokenType.EndArray);
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                }

                Assert(reader.Read());
            }

            //Assert(reader.TokenType == JsonTokenType.EndObject);

            return result;
        }

        private static CatalogLeafItem GetCatalogLeafItem(ref this Utf8JsonReader reader)
        {
            var leaf = new CatalogLeafItem();

            //Assert(reader.TokenType == JsonTokenType.StartObject);
            Assert(reader.Read());

            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals(AtIdName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    //Assert(reader.TokenType == JsonTokenType.String);
                    leaf.CatalogLeafUrl = reader.GetString();
                }
                else if (reader.ValueTextEquals(AtTypeName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    //Assert(reader.TokenType == JsonTokenType.String);
                    if (reader.ValueTextEquals(NuGetPackageDetailsName.EncodedUtf8Bytes))
                    {
                        leaf.Type = CatalogLeafType.PackageDetails;
                    }
                    else if (reader.ValueTextEquals(NuGetPackageDeleteName.EncodedUtf8Bytes))
                    {
                        leaf.Type = CatalogLeafType.PackageDelete;
                    }
                }
                else if (reader.ValueTextEquals(CommitTimestampName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    //Assert(reader.TokenType == JsonTokenType.String);
                    Assert(reader.TryGetDateTimeOffset(out var commitTimestamp));
                    leaf.CommitTimestamp = commitTimestamp;
                }
                else if (reader.ValueTextEquals(NuGetIdName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    //Assert(reader.TokenType == JsonTokenType.String);
                    leaf.PackageId = reader.GetString();
                }
                else if (reader.ValueTextEquals(NuGetVersionName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    //Assert(reader.TokenType == JsonTokenType.String);
                    leaf.PackageVersion = reader.GetString();
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                }

                Assert(reader.Read());
            }

            //Assert(reader.TokenType == JsonTokenType.EndObject);

            return leaf;
        }

        private static void Assert(bool condition, string message = "")
        {
            //if (!condition) throw new Exception(message);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Json>();
        }
    }
}
