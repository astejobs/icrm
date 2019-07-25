using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class FeedbackAssignedVM
    {
        [Required]
        public string Status { get; set; }
        public string Satisfaction { get; set; }
        [Required]
        public string Response { get; set; }
    }
}