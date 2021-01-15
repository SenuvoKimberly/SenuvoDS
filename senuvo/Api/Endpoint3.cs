using DirectScale.Disco.Extension.Api;

namespace senuvo.Api
{
    public class Endpoint3 : ApiEndpoint<Endpoint3Request>
    {
        public Endpoint3(IRequestParsingService parsingService) : base("senuvo/end3", parsingService)
        {

        }

        public override IApiResponse Post(Endpoint3Request request)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = "senuvo.Html.htmlpage.html";
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
            {
                string data = reader.ReadToEnd();

                return new Ok(new { data });
            }
        }
    }
}
