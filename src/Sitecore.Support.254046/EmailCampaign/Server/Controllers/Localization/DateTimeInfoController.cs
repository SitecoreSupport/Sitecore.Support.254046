namespace Sitecore.Support.EmailCampaign.Server.Controllers.Localization
{
  using Newtonsoft.Json;
  using Sitecore.EmailCampaign.Server.Controllers;
  using Sitecore.ExM.Framework.Formatters;
  using Sitecore.Services.Core;
  using System;
  using System.Net;
  using System.Net.Http;
  using System.Text;
  using System.Web.Http;

  [ServicesController("EXM.DateTimeInfo")]
  public class DateTimeInfoController : ServicesApiControllerBase
  {
    [ActionName("DefaultAction")]
    public HttpResponseMessage Get()
    {
      base.SetContextLanguageToClientLanguage();
      string str = JsonConvert.SerializeObject(new DateTimeInfo());
      string content = $"define([], function() {{ return {str}; }});";
      return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content, Encoding.UTF8, "text/javascript") };
    }
  }
}