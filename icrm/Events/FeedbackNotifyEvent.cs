using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using icrm.Models;

namespace icrm.Events
{
    public class FeedbackNotifyEventArgs : EventArgs
    {
        public NotificationMessage NotificationMessage { get; set; }
    }
    public class FeedbackNotifyEvent
    {
       
        public event EventHandler<FeedbackNotifyEventArgs> FeedbackNotified;

        public void notify(NotificationMessage message)
        {
            Debug.Print("----firing event--------");
            OnFeedbackNotify(message);
        }

        protected virtual void OnFeedbackNotify(NotificationMessage message)
        {
            Debug.Print("----firing event-----again---" + message);
            if (message != null)
                FeedbackNotified(this, new FeedbackNotifyEventArgs() { NotificationMessage = message });
        }
    }
}