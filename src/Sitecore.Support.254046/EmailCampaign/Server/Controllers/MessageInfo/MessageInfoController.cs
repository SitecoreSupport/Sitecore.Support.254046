namespace Sitecore.Support.EmailCampaign.Server.Controllers.MessageInfo
{
  using Sitecore.Configuration.KnownSettings;
  using Sitecore.Diagnostics;
  using Sitecore.EmailCampaign.Model.Web;
  using Sitecore.EmailCampaign.Server.Contexts;
  using Sitecore.EmailCampaign.Server.Controllers;
  using Sitecore.EmailCampaign.Server.Responses;
  using Sitecore.EmailCampaign.Server.Services.Interfaces;
  using Sitecore.ExM.Framework.Diagnostics;
  using Sitecore.Globalization;
  using Sitecore.Modules.EmailCampaign;
  using Sitecore.Services.Core;
  using System;
  using System.Web.Http;

  [ServicesController("EXM.MessageInfo")]
  public class MessageInfoController : ServicesApiControllerBase
  {
    private readonly IMessageInfoService messageInfoService;
    private readonly ILogger logger;
    private readonly CoreSettings coreSettings;

    public MessageInfoController(IMessageInfoService messageInfoService, ILogger logger)
    {
      Assert.ArgumentNotNull(messageInfoService, "messageInfoService");
      Assert.ArgumentNotNull(logger, "logger");
      this.messageInfoService = messageInfoService;
      this.logger = logger;
      coreSettings = new CoreSettings(ServiceProviderServiceExtensions.GetService<BaseSettings>(ServiceLocator.ServiceProvider));
    }

    [ActionName("DefaultAction")]
    public Response MessageInfo(MessageInfoContext data)
    {
      Assert.ArgumentNotNull(data, "data");
      base.SetContextLanguageToClientLanguage();
      MessageInfoResponse response = new MessageInfoResponse();
      try
      {
        response.Info = this.messageInfoService.Get(data.MessageId, data.Language);
      }
      catch (Exception exception)
      {
        this.logger.LogError(exception.Message, exception);
        response.Error = true;
        response.ErrorMessage = EcmTexts.Localize("A serious error occurred please contact the administrator", Array.Empty<object>());
      }
      return response;
    }
  }
}