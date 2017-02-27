using System;

namespace versionOneProxy
{
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Web;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using ShiftIt.Http;

    class Program
    {
        public static string bearerToken;
        public static string targetSite;
        public static IHttpClient client;


        static void Main()
        {
            var conf = XDocument.Load(@".\config.xml");
            client = new HttpClient();
            bearerToken = conf.XPathSelectElement("//auth-token").Value;
            targetSite = conf.XPathSelectElement("//target-site").Value;

            for (; ; )
            {
                try
                {
                    var listen = new[] { "http://127.0.0.1:8020/", "http://localhost:8020/" };
                    using (new WebServer(SendResponse, listen))
                    {
                        Console.WriteLine("listening on " + string.Join(", ", listen));
                        for (; ; )
                        {
                            Thread.Sleep(250);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Web host failed: " + ex.Message);
                    Thread.Sleep(1000);
                    Console.WriteLine("Restarting");
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public static string SendResponse(HttpListenerRequest request, HttpListenerResponse rawResponse)
        {
            var rq = new HttpRequestBuilder()
                .Get(new Uri(targetSite + request.Url.PathAndQuery))
                .AddHeader("Authorization", bearerToken)
                .Accept("application/xml")
                .Build();

            using (var result = client.Request(rq))
            {
                switch (result.StatusClass)
                {
                    case StatusClass.Success:
                        rawResponse.AddHeader("Content-Type", "text/xml");
                        var xsltFileName = HttpUtility.ParseQueryString(request.Url.Query).Get("xsl");
                        var raw = TransformXml(result.BodyReader.ReadStringToLength(), xsltFileName);
                        return raw;

                    // All error cases for us:
                    case StatusClass.Invalid:
                    case StatusClass.ClientError:
                    case StatusClass.ServerError:
                        rawResponse.StatusCode = result.StatusCode;
                        return "Target server gave an error: "+result.StatusMessage;
                    case StatusClass.Information:
                        rawResponse.StatusCode = 500;
                        return "Target server is trying to do a multi-part response. I'm too simple to handle that";
                    case StatusClass.Redirection:
                        rawResponse.StatusCode = 500;
                        return "Target server requested a redirect. I'm too simple to handle that";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        }

        public static string TransformXml(String xmlFile, string xslFileName)
        {
            var doc = new XPathDocument(new StringReader(xmlFile));
            var xslt = new System.Xml.Xsl.XslCompiledTransform();
            xslt.Load(xslFileName);
            var stm = new MemoryStream();
            xslt.Transform(doc, null, stm);
            stm.Position = 0;
            var sr = new StreamReader(stm);
            return sr.ReadToEnd();
        } 
    }
}
