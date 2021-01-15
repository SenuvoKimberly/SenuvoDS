using System.Web;
using DirectScale.Disco.Extension.Api;

namespace senuvo.Views
{
    public class View1 : IViewEndpoint
    {
        public ViewDefinition GetDefinition()
        {
            return new ViewDefinition
            {
                Menu = Menu.Associates,
                SecurityRight = "",
                Title = "Custom V1"
            };
        }

        public View GetView(ApiRequest request)
        {
            return new View
            {
                Html = "<head></head><body><h1>View1 Page Title</h1></body>"
            };
        }
    }
}
