using icrm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace icrm.Controllers
{
    [Authorize(Roles ="Admin")]
    public class ForwardEmailController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Create()
        {
          List<ForwardEmailVM> forwardEmailVMs = db.Users.Where(w=>w.forwarDeptEmailCCStatus==true).Select(x => new ForwardEmailVM
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                Name = x.FirstName + x.LastName,
                Email = x.Email
            }).ToList();
            return View(forwardEmailVMs);
        }
        [HttpPost]
        public ActionResult Create(ForwardEmailVM forwardEmailVM)
        {
            if(ModelState.IsValid)
            {
               var user= db.Users.FirstOrDefault(x => x.EmployeeId == forwardEmailVM.EmployeeId);
                if(user!=null)
                {
                    user.forwarDeptEmailCCStatus = true;
                    db.SaveChanges();

                    TempData["EmailMessage"] = "Email Successfully Added to Forward List";
                }
                else
                {
                    TempData["EmailMessage"] = "Error";
                }
            }
            List<ForwardEmailVM> forwardEmailVMs = db.Users.Where(w => w.forwarDeptEmailCCStatus == true).Select(x => new ForwardEmailVM
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                Name = x.FirstName+" " + x.LastName,
                Email = x.Email
            }).ToList();
            return View(forwardEmailVMs);
        }
        public ActionResult RemoveEmail(string id)
        {
            var user = db.Users.FirstOrDefault(x => x.Id == id);
            user.forwarDeptEmailCCStatus = false;
            db.SaveChanges();
            TempData["EmailMessage"] = "Email Removed from Forward CC List";
            return RedirectToAction("create");
        }
    }
}