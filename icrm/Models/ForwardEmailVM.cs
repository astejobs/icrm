using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace icrm.Models
{
    public class ForwardEmailVM
    {
      
        public string Id { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}