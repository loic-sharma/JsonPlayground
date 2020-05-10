using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            return reader.ReadCatalogPage();
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

        public static CatalogPage ReadCatalogPage(ref this Utf8JsonReader reader)
        {
            var result = new CatalogPage();
            
            reader.ReadStartObject();
            Debug.Assert(reader.Read());

            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals(AtIdName.EncodedUtf8Bytes))
                {
                    result.CatalogIndexUrl = reader.ReadString();
                }
                else if (reader.ValueTextEquals(CountName.EncodedUtf8Bytes))
                {
                    result.Count = reader.ReadInt32();
                }
                else if (reader.ValueTextEquals(CommitTimestampName.EncodedUtf8Bytes))
                {
                    result.CommitTimestamp = reader.ReadDateTimeOffset();
                }
                else if (reader.ValueTextEquals(ItemsName.EncodedUtf8Bytes))
                {
                    result.Items = new List<CatalogLeafItem>();

                    reader.ReadStartArray();

                    while (reader.TryReadCatalogLeafItem(out var leaf))
                    {
                        result.Items.Add(leaf);
                    }

                    Debug.Assert(reader.TokenType == JsonTokenType.EndArray);
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                }

                Debug.Assert(reader.Read());
            }

            Debug.Assert(reader.TokenType == JsonTokenType.EndObject);

            return result;
        }

        private static bool TryReadCatalogLeafItem(ref this Utf8JsonReader reader, out CatalogLeafItem leaf)
        {
            Debug.Assert(reader.Read());

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                leaf = null;
                return false;
            }

            leaf = new CatalogLeafItem();

            Debug.Assert(reader.Read());

            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals(AtIdName.EncodedUtf8Bytes))
                {
                    leaf.CatalogLeafUrl = reader.ReadString();
                }
                else if (reader.ValueTextEquals(AtTypeName.EncodedUtf8Bytes))
                {
                    reader.Read();
                    Debug.Assert(reader.TokenType == JsonTokenType.String);
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
                    leaf.CommitTimestamp = reader.ReadDateTimeOffset();
                }
                else if (reader.ValueTextEquals(NuGetIdName.EncodedUtf8Bytes))
                {
                    leaf.PackageId = reader.ReadString();
                }
                else if (reader.ValueTextEquals(NuGetVersionName.EncodedUtf8Bytes))
                {
                    leaf.PackageVersion = reader.ReadString();
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                }

                reader.Read();
            }

            Debug.Assert(reader.TokenType == JsonTokenType.EndObject);

            return true;
        }

        private static void ReadStartObject(ref this Utf8JsonReader reader)
        {
            Debug.Assert(reader.Read());
            Debug.Assert(reader.TokenType == JsonTokenType.StartObject);
        }

        private static void ReadStartArray(ref this Utf8JsonReader reader)
        {
            Debug.Assert(reader.Read());
            Debug.Assert(reader.TokenType == JsonTokenType.StartArray);
        }

        private static string ReadString(ref this Utf8JsonReader reader)
        {
            Debug.Assert(reader.Read());
            Debug.Assert(reader.TokenType == JsonTokenType.String);

            return reader.GetString();
        }

        private static int ReadInt32(ref this Utf8JsonReader reader)
        {
            Debug.Assert(reader.Read());
            Debug.Assert(reader.TokenType == JsonTokenType.Number);

            return reader.GetInt32();
        }

        private static DateTimeOffset ReadDateTimeOffset(ref this Utf8JsonReader reader)
        {
            reader.Read();
            Debug.Assert(reader.TokenType == JsonTokenType.String);
            if (!reader.TryGetDateTimeOffset(out var commitTimestamp))
            {
                Debug.Fail(message: null);
            }

            return commitTimestamp;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            //var bytes = Encoding.UTF8.GetBytes(TestData.CatalogPage);
            //var reader = new Utf8JsonReader(bytes);
            //var result = reader.ReadCatalogPage();

            var summary = BenchmarkRunner.Run<Json>();
        }
    }
}
