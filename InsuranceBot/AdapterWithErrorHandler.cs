﻿using System;
using System.Linq;
using InsuranceBot.Middlewares;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace InsuranceBot
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(ICredentialProvider credentialProvider, ILogger<BotFrameworkHttpAdapter> logger, HeroCardsTranslationMiddleware heroCardsTranslationMiddleware, TranslationMiddleware translationMiddleware, ConversationState conversationState = null)

            : base(credentialProvider)
        {
            // Use translation middleware
            if (translationMiddleware == null)
            {
                throw new NullReferenceException(nameof(translationMiddleware));
            }

            Use(translationMiddleware);

            // Use hero cards translation middleware
            if (heroCardsTranslationMiddleware == null)
            {
                throw new NullReferenceException(nameof(heroCardsTranslationMiddleware));
            }

            Use(heroCardsTranslationMiddleware);


            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }
    }
}
