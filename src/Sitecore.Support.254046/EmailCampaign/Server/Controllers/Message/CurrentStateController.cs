namespace Sitecore.Support.EmailCampaign.Server.Controllers.Message
{
  using Sitecore.Diagnostics;
  using Sitecore.EmailCampaign.Model.Dispatch;
  using Sitecore.EmailCampaign.Model.Message;
  using Sitecore.EmailCampaign.Model.Web;
  using Sitecore.EmailCampaign.Server.Contexts;
  using Sitecore.EmailCampaign.Server.Controllers;
  using Sitecore.EmailCampaign.Server.Responses;
  using Sitecore.ExM.Framework.Diagnostics;
  using Sitecore.Modules.EmailCampaign.Core;
  using Sitecore.Modules.EmailCampaign.Core.Data;
  using Sitecore.Modules.EmailCampaign.Factories;
  using Sitecore.Modules.EmailCampaign.Messages;
  using Sitecore.Modules.EmailCampaign.Services;
  using Sitecore.Services.Core;
  using System;
  using System.Web.Http;

  [ServicesController("EXM.CurrentState")]
  public class CurrentStateController : ServicesApiControllerBase
  {
    private readonly IExmCampaignService _exmCampaignService;
    private readonly ItemUtilExt itemUtilExt;
    private readonly ILogger logger;
    private readonly IMessageStateInfoFactory _messageStateInfoFactory;
    private readonly IAbnTestService _abnTestService;
    private readonly EcmDataProvider _dataProvider;

    public CurrentStateController(IExmCampaignService exmCampaignService, ItemUtilExt itemUtilExt, EcmDataProvider dataProvider, ILogger logger, IMessageStateInfoFactory messageStateInfoFactory, IAbnTestService abnTestService)
    {
      Assert.ArgumentNotNull(exmCampaignService, "exmCampaignService");
      Assert.ArgumentNotNull(itemUtilExt, "itemUtilExt");
      Assert.ArgumentNotNull(dataProvider, "dataProvider");
      Assert.ArgumentNotNull(logger, "logger");
      Assert.ArgumentNotNull(messageStateInfoFactory, "messageStateInfoFactory");
      Assert.ArgumentNotNull(abnTestService, "abnTestService");
      this._exmCampaignService = exmCampaignService;
      this.itemUtilExt = itemUtilExt;
      this._dataProvider = dataProvider;
      this.logger = logger;
      this._messageStateInfoFactory = messageStateInfoFactory;
      this._abnTestService = abnTestService;
    }

    [ActionName("DefaultAction")]
    public Response CurrentState(CurrentStateContext data)
    {
      Assert.IsNotNull(data, "data");
      base.SetContextLanguageToClientLanguage();
      CurrentStateResponse response = new CurrentStateResponse();
      try
      {
        if (!this._dataProvider.ProviderAvailable && (data.PreviousState != EcmTexts.Localize("Sent", Array.Empty<object>())))
        {
          response.Error = true;
          response.ErrorMessage = EcmTexts.Localize("Your message cannot be sent at this time, please contact your system administrator.", Array.Empty<object>());
          if (!this._dataProvider.ProviderAvailable)
          {
            this.logger.LogError("EXM dispatch database is unavailable, please check the connection");
          }
        }
        MessageItem messageItem = this._exmCampaignService.GetMessageItem(Guid.Parse(data.MessageId));
        MessageState state = messageItem.State;
        response.StateCode = (int)messageItem.State;
        if (state == MessageState.Draft)
        {
          if ((!string.IsNullOrWhiteSpace(data.PreviousState) && (data.PreviousState != EcmTexts.Localize("Draft", Array.Empty<object>()))) && ((data.PreviousState != EcmTexts.Localize("Dispatch Scheduled", Array.Empty<object>())) && (data.PreviousState != EcmTexts.Localize("Activation Scheduled", Array.Empty<object>()))))
          {
            response.StateDescription = EcmTexts.Localize("The message delivery has completed in emulation mode.", Array.Empty<object>());
          }
          response.State = EcmTexts.Localize("Draft", Array.Empty<object>());
        }
        switch (state)
        {
          case MessageState.ActivationScheduled:
            response.State = EcmTexts.Localize("Activation Scheduled", Array.Empty<object>());
            break;

          case MessageState.DispatchScheduled:
            response.State = EcmTexts.Localize("Dispatch Scheduled", Array.Empty<object>());
            break;

          case MessageState.Active:
            response.State = EcmTexts.Localize("Active", Array.Empty<object>());
            response.StateDescription = EcmTexts.Localize("The message has been activated.", Array.Empty<object>());
            break;

          case MessageState.Inactive:
            response.State = EcmTexts.Localize("Inactive", Array.Empty<object>());
            response.StateDescription = EcmTexts.Localize("The message has been deactivated.", Array.Empty<object>());
            break;

          case MessageState.Sent:
            response.State = EcmTexts.Localize("Sent", Array.Empty<object>());
            response.StateDescription = EcmTexts.Localize("The message delivery has completed.", Array.Empty<object>());
            break;
        }
        if ((state == MessageState.Sending) || (state == MessageState.Queuing))
        {
          SendingState sendingState = this._messageStateInfoFactory.CreateInstance(messageItem).SendingState;
          AbnTest abnTest = this._abnTestService.GetAbnTest(messageItem);
          switch (sendingState)
          {
            case SendingState.Initialization:
              {
                response.State = EcmTexts.Localize("Initialization", Array.Empty<object>());
                object[] objArray3 = new object[] { response.State };
                response.StateDescription = EcmTexts.Localize("The message status has been changed to '{0}'.", objArray3);
                return response;
              }
            case SendingState.Queuing:
              {
                response.State = EcmTexts.Localize("Queuing", Array.Empty<object>());
                object[] objArray2 = new object[] { response.State };
                response.StateDescription = EcmTexts.Localize("The message status has been changed to '{0}'.", objArray2);
                return response;
              }
            case SendingState.Paused:
              {
                string str = EcmTexts.Localize("Paused", Array.Empty<object>());
                if (((messageItem.MessageType != MessageType.Automated) && (abnTest != null)) && abnTest.IsWaitingWinner())
                {
                  str = (this.itemUtilExt.GetSelectWinnerScheduleTaskItem(messageItem.ID) != null) ? EcmTexts.Localize("A/B Testing", Array.Empty<object>()) : EcmTexts.Localize("Select Winner", Array.Empty<object>());
                }
                response.State = str;
                object[] objArray4 = new object[] { response.State };
                response.StateDescription = EcmTexts.Localize("The message status has been changed to '{0}'.", objArray4);
                return response;
              }
            case SendingState.Finishing:
              {
                response.State = EcmTexts.Localize("Finishing", Array.Empty<object>());
                object[] objArray1 = new object[] { response.State };
                response.StateDescription = EcmTexts.Localize("The message status has been changed to '{0}'.", objArray1);
                return response;
              }
          }
          response.State = EcmTexts.Localize("Sending", Array.Empty<object>());
          object[] parameters = new object[] { response.State };
          response.StateDescription = EcmTexts.Localize("The message status has been changed to '{0}'.", parameters);
        }
        return response;
      }
      catch (Exception exception)
      {
        this.logger.LogError(exception.Message, exception);
        response.Error = true;
        if (string.IsNullOrEmpty(response.ErrorMessage))
        {
          response.ErrorMessage = EcmTexts.Localize("A serious error occurred please contact the administrator", Array.Empty<object>());
        }
      }
      return response;
    }
  }
}