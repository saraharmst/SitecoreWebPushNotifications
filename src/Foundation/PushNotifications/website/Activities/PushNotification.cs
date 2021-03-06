﻿using Sitecore.Xdb.MarketingAutomation.Core.Activity;
using Sitecore.Xdb.MarketingAutomation.Core.Processing.Plan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SF.Foundation.PushNotifications.Facets;
using Sitecore.XConnect.Client;
using SF.Foundation.PushNotifications.Constants;
using Sitecore.XConnect;
using SF.Foundation.PushNotifications.Services;
using SF.Foundation.PushNotifications.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace SF.Foundation.PushNotifications.Activities
{
    public class PushNotification : IActivity
    {
        //Parameters
        public string Body { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Icon { get; set; }
        public string CTA { get; set; }

        protected ILogger<IActivity> Logger { get; }

        public IActivityServices Services { get; set; }

        public IPushNotificationService PushNotificationService { get; set; }

        public PushNotification(ILogger<PushNotification> logger)
        {
            //Was having issues registering the service in the Automation engine, so newing up for now.
            PushNotificationService = new PushNotificationService();

            this.Logger = logger;
        }

        public string MessageId { get; set; }

        public ActivityResult Invoke(IContactProcessingContext context)
        {
            try
            {
                var subscriptions = context.Contact.GetFacet<PushSubscriptions>();
                if (subscriptions == null)
                {
                    return new SuccessMove("default");
                }

                var notification = new Notification()
                {
                    Title = this.Title,
                    Body = this.Body,
                    Image = this.Image,
                    Icon = this.Icon,
                    CTA = this.CTA
                };

                var message = JsonConvert.SerializeObject(notification);

                //Dictionary using device, we just want all, so take the value in the KVP
                foreach (var subscription in subscriptions.Subscriptions)
                {
                    try
                    {
                        //Let's send them.
                        PushNotificationService.SendNotification(subscription.Value, message, subscription.Value.VapidPublicKey, subscription.Value.VapidPrivateKey);
                    }
                    catch(Exception ex)
                    {
                        Logger.LogError(ex, "Failed to send Push Notification");
                    }
                }

                return new SuccessMove("default");

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed sending Message");
                return new Failure("failed");
            }
        }

    }
}