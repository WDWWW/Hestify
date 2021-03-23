using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace Hestify.Benchmark
{

    public class XmlConvertScenario
    {
        [Benchmark]
        public string ConvertXml()
        {
            var xmlSerializer = new XmlSerializer(typeof(NestedClass));
            using var memoryStream = new MemoryStream();
            using var streamWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlSerializer.Serialize(streamWriter, NestedClass.Instance);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        [Benchmark]
        public string ConvertXmlViaJson()
        {
            var jsonText = JsonConvert.SerializeObject(NestedClass.Instance);
            return JsonConvert.DeserializeXmlNode(jsonText, "root")!.OuterXml;
        }

        [MinColumn]
        [MaxColumn]
        [MedianColumn]
        [IterationsColumn]
        public class NestedClass
        {
            public static NestedClass Instance = new NestedClass()
            {
                A = "A",
                B = "B",
                Inner = new ()
                {
                    C = true,
                    D = new []
                    {
                        "A", "B"
                        
                    }
                }
            };
            
            public string A { get; set; }

            public string B { get; set; }

            public InnerClass Inner { get; set; }

            public class InnerClass
            {
                public bool C { get; set; }

                public string[] D { get; set; }
            }
        }
    }
}