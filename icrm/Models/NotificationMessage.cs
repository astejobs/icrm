using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class NotificationMessage
    {
        public string DeviceId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string For { get; set; }
        public string Status { get; set; }
        public string FeedbackId { get; set; }
    }
}