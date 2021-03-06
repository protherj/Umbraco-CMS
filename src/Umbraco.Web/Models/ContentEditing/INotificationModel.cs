﻿using System.Collections.Generic;

namespace Umbraco.Web.Models.ContentEditing
{
    public interface INotificationModel
    {
        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        List<Notification> Notifications { get; }
    }
}