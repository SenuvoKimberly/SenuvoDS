using DirectScale.Disco.Extension.Api;

namespace senuvo.Views
{
    public class View2 : ViewEndpoint<View2Model>
    {
        public View2(IHtmlTemplatingService templatingService) : base( Menu.Associates, "View2", "customer/view2" , templatingService)
        {

        }

        public override ViewTemplate<View2Model> GetTemplate(ViewDefinition definition, ApiRequest request)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = "senuvo.Html.htmlpage.html";
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
            {
                string data = reader.ReadToEnd();

                return new ViewTemplate<View2Model>
                {
                    Html = data,
                    Model = new View2Model
                    {
                        PageTitle = "Page Title"
                    }
                };
            }
        }
    }
}
