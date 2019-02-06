﻿using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Diagnostics;
using icrm.RepositoryInterface;
using PagedList;
using icrm.Models;
using icrm.RepositoryImpl;
using System.Data.Entity;
using System.Net;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Globalization;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Net.Http;
using System.Web.UI.WebControls;
using System.Web.UI;
using Constants = icrm.Models.Constants;
using Comments = icrm.Models.Comments;
using System.Net.Mail;
using System.Net.Mime;
using icrm.Events;
using System.Drawing.Imaging;

namespace icrm.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        

        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        private IFeedback feedInterface;
        private EventService eventService;
       

        public HRController() {
            ViewBag.Status = Models.Constants.statusList;
            feedInterface = new FeedbackRepository();
            eventService = new EventService();
           
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: HR DAshboard
        public ActionResult DashBoard(int? page)
        {
            ViewBag.linkName = "openticket";
            TicketCounts();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.OpenWithoutDepart(pageIndex,pageSize);          
            return View(feedbackList);
        }


        /*************** Get HR Ticket Raise by HR****************/
        [HttpGet]
        [Route("hr/feedback/")]
        public ActionResult Create()
        {
             var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
              var userRole = roleManager.FindByName("User").Users.FirstOrDefault();
            if (userRole!=null) {
                ViewBag.EmployeeList = db.Users.Where(m => m.Roles.Any(s => s.RoleId == userRole.RoleId)).ToList();
              }         
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            getAttributeList();
            return View();
            }

        /*****************FeedBack View**********************/
        [HttpGet]
        [Route("feedback/{id}")]
        public ActionResult view(string id)
        {           
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }

        }


        /************HR FeedBack Post*******************/
        [HttpPost]
        [Route("hr/feedback/")]
        public ActionResult Create(int? id,string submitButton, Feedback feedback, HttpPostedFileBase file)
        {
            System.Diagnostics.Debug.WriteLine(Request.Form["emailsss"]+"tttttttttttttttttttt");
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            if (roleManager.FindByName("User").Users.FirstOrDefault() != null)
            {
                var userRole = roleManager.FindByName("User").Users.First();
                ViewBag.EmployeeList = db.Users.Where(m => m.Roles.Any(s => s.RoleId == userRole.RoleId)).ToList();
            }
            getAttributeList();
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;

            if (feedback.userId == null) {
                feedback.userId=user.Id;
            }
            if (file != null)
            {
                var fileSize = file.ContentLength;
                if (fileSize > 2100000)
                {
                    TempData["Message"] = "File Size Limit Exceeds";
                    return View("Create", feedback);
                }
                else
                {
                    string filename = null;
                    String ext = Path.GetExtension(file.FileName);
                    filename = feedback.id + ext;
                    feedback.attachment = filename;
                    file.SaveAs(Path.Combine(icrm.Models.Constants.PATH, filename));
                }
            }
            else
            {
                feedback.attachment = null;
            }
                switch (submitButton)
                {
                    case "Forward/Create":
                        if (feedback.departmentID != null && Request.Form["responsee"] == "")
                        {
                            if (ModelState.IsValid)
                            {                                
                                feedback.assignedBy = user.Id;
                                feedback.assignedDate = DateTime.Now;
                                feedback.checkStatus = Constants.ASSIGNED;                               
                                feedInterface.Save(feedback);
                                TempData["MessageSuccess"] = "Ticket created Successfully";
                                eventService.sendEmails(Request.Form["emailsss"], PopulateBody(feedback));
                        }
                            else
                            {
                                ViewData["user"] = user;
                                TempData["Message"] = "Ticket details are not valid,Fill details properly";
                                return View("Create", feedback);
                            }
                        }
                        else
                        {
                            TempData["Message"] = "Select Department & Comment field should be empty";
                            return View("Create", feedback);
                        }
                        return View("Create");


                    case "Resolve":
                        if (feedback.departmentID == null && Request.Form["responsee"] != "")
                        {                   
                            if (ModelState.IsValid)
                          {
                            feedback.submittedById = user.Id;
                            feedback.assignedBy = null;
                            feedback.assignedDate = null;
                            feedback.checkStatus = Constants.RESOLVED;                       
                            feedInterface.Save(feedback);                          
                            Comments c = new Comments();
                            c.text = Request.Form["responsee"];
                            c.commentedById = user.Id;
                            c.feedbackId = feedback.id;
                            db.comments.Add(c);
                            db.SaveChanges();
                            TempData["MessageSuccess"] = "Ticket created Successfully";
                                return RedirectToAction("Create");
                            }
                            else
                            {
                                ViewData["user"] = user;
                                TempData["Message"] = "Ticket details are not valid,Fill details properly";
                                return View("Create", feedback);
                            }

                        }
                        else
                        {
                            TempData["Message"] = "Please fill Comment field & Department should be empty";
                            return View("Create", feedback);
                        }
                      
                    default:
                        return RedirectToAction("Create");                    
                }
        }


        /*****************Change Status By HR in FeedBack*********************/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("update/")]
        public ActionResult update(Feedback feedback)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            getAttributeList();
            string type=Request.Form["typeoflink"];
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            if (roleManager.FindByName("User").Users.FirstOrDefault() != null)
            {
                var userRole = roleManager.FindByName("User").Users.First();
                ViewBag.EmployeeList = db.Users.Where(m => m.Roles.Any(s => s.RoleId == userRole.RoleId)).ToList();

            }                     
            Feedback f = db.Feedbacks.Find(feedback.id);
            f.checkStatus = feedback.status;
            f.status = feedback.status;
            if (feedback.status == Constants.CLOSED) {
                f.closedDate = DateTime.Today;
            }          
            switch(type)
            {
                case "Resolvedtype":
                    if (ModelState.IsValid)
                    {                       
                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                         @TempData["MessageSuccess"] = "Ticket has been Updated Successfully";
                         return View("resolvedview",feedback);
                case "Respondedtype":
                    if (ModelState.IsValid)
                    {                      
                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                          @TempData["MessageSuccess"] = "Ticket has been Updated Successfully";
                          ViewData["commentList"] = db.comments.Where(m => m.feedbackId == feedback.id).ToList();
                         return View("respondedview", feedback);
                case "Assignedtype":                  
                    if (ModelState.IsValid)
                    {                       
                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                         @TempData["MessageSuccess"] = "Ticket has been Updated Successfully";
                          return View("assignedview", feedback);

                case "Rejectedtype":
                    if (ModelState.IsValid)
                    {                     
                        db.Entry(f).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                      @TempData["MessageSuccess"] = "Ticket has been Updated Successfully";
                      return View("rejectedview", feedback);
                default:
                    return View("Assignedtype", feedback);
                  
            }
           
        }

        /**************ASSIGN DEPARTMENT************************/

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("assigndepart/")]
        public ActionResult assign(string submitButton,Feedback feedback)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            
            feedback.user = db.Users.Find(feedback.userId);
            switch (submitButton)
            {
                case "Forward":
                    if (feedback.departmentID != null && Request.Form["responsee"] == "")
                    {
                        feedback.department = db.Departments.Find(feedback.departmentID);
                        feedback.checkStatus = Constants.ASSIGNED;                        
                            if (feedback.assignedDate == null)
                            {
                                feedback.assignedBy = user.Id;
                                feedback.assignedDate = DateTime.Now;                               
                            }
                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["MessageSuccess"] = "Ticket has been Forwarded Successfully";
                             eventService.sendEmails(Request.Form["emailsss"], PopulateBody(feedback));                      
                    }
                    else
                    {
                             TempData["Message"] = "Select Department & Comment field should be empty";
                             return RedirectToAction("view", new { id = feedback.id });
                    }
                    return RedirectToAction("DashBoard");
                case "Resolve":
                    if (feedback.departmentID == null && Request.Form["responsee"] != "")
                    {
                            Comments c = new Comments();
                            c.text = Request.Form["responsee"];
                            c.commentedById = user.Id;
                            c.feedbackId = feedback.id;
                            db.comments.Add(c);
                            db.SaveChanges();
                            feedback.assignedBy = null;
                            feedback.assignedDate = null;
                            feedback.submittedById = user.Id;
                            feedback.checkStatus = Constants.RESOLVED;
                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["MessageSuccess"] = "Ticket has been Resolved Successfully";
                            return RedirectToAction("DashBoard");                     
                    }
                    else
                    {
                        TempData["Message"] = "Please fill Comment field & Department should be empty";
                        return RedirectToAction("view", new { id = feedback.id });
                    }
                  case "Reject":
                    if (feedback.departmentID == null && Request.Form["responsee"] != "") {
                            Comments c = new Comments();
                            c.text = Request.Form["responsee"];
                            c.commentedById = user.Id;
                            c.feedbackId = feedback.id;
                            db.comments.Add(c);
                            db.SaveChanges();
                            feedback.submittedById = user.Id;          
                            feedback.checkStatus = Constants.REJECTED;
                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["MessageSuccess"] = "Ticket has been Rejected";
                            return RedirectToAction("DashBoard");
                    }
                    else
                    {
                        TempData["Message"] = "Comment should not be empty & Department should be empty";
                        return RedirectToAction("view", new { id = feedback.id });
                    }
                default:
                    return RedirectToAction("view", new { id = feedback.id });
            }
        }

        /****************************SEARCH BY HR**************************/
        [HttpGet]
        [Route("hr/search/")]
        public ActionResult search(int? page, string status, string date22, string date1, string export)
        {

           
            string d3 = date22;           
            string dd = date1;
            if (d3.Equals("") || dd.Equals(""))
            {
                TempData["DateMsg"] = "Please Select StartDate And EndDate";
                return RedirectToAction("Dashboard");
            }
            else
            {
                DateTime dt = DateTime.ParseExact(dd, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(d3, "dd-MM-yyyy", CultureInfo.InvariantCulture);              
                if (export == "excel")
                {
                    IEnumerable<Feedback> feedbacks = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), status);                 
                    var grid = new GridView();
                    grid.DataSource = feedbacks;
                    grid.DataBind();
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachement; filename=data.xls");
                    Response.ContentEncoding = System.Text.Encoding.Unicode;
                    Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                    Response.ContentType = "application/excel";
                    StringWriter sw = new StringWriter();
                    HtmlTextWriter htw = new HtmlTextWriter(sw);
                    grid.RenderControl(htw);
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                    return View();
                }
                else
                {
                    ViewBag.Search = export;
                    ViewBag.statuss = status;
                    ViewBag.startDate = date1;
                    ViewBag.endDate = date22;
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    TicketCounts();
                    switch (status)
                    {
                        case "Open":
                            ViewBag.linkName = "openticket";
                            break;
                        case "Closed":
                            ViewBag.linkName = "closedticket";
                            break;
                        case "Resolved":
                            ViewBag.linkName = "resolvedticket";
                            break;
                        case "Rejected":
                            ViewBag.linkName = "rejectedticket";

                            break;
                        default:
                            break;
                    }
                    ViewBag.showDate = Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    int pageSize = 10;
                    int pageIndex = 1;
                    pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                    ViewData["user"] = user;
                    IPagedList<Feedback> feedbacks = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), status, pageIndex, pageSize);
                    ViewBag.Status = Models.Constants.statusList;

                    return View("DashBoard", feedbacks);

                }

            }
            

        }

        /**********GET ALL TICKETS*******************/

        public ActionResult alltickets(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "Alltickets";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAll(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }




        /*****************OPEN TICKETS LIST********************/

        public ActionResult open(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "openticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.OpenWithoutDepart(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }



        public ActionResult openAll(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "openticket";

            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.OpenWithoutDepart(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        public ActionResult openAlltickets(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "openAllticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllOpen(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        public ActionResult assigned(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "assignedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllAssigned(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        /*****************RESOLVED TICKETS LIST********************/

        public ActionResult resolved(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "resolvedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllResolved(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }


        /*****************RESPONDED TICKETS LIST********************/

        public ActionResult responded(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "respondedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllResponded(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        /*****************CLOSED TICKETS LIST********************/

        public ActionResult Closed(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "closedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllClosed(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }

        /*****************REJECTED TICKETS LIST********************/

        public ActionResult Rejected(int? page)
        {
            TicketCounts();
            ViewBag.linkName = "rejectedticket";
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getAllRejected(pageIndex, pageSize);
            return View("Dashboard", feedbackList);
        }


        /*****************ENQUIRIES TICKETS LIST********************/
        public ActionResult tickets(int? page, string typeId)
        {
            TicketCounts();
            ViewBag.linkName = "tickettype";
            ViewBag.type = typeId;
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            IPagedList<Feedback> feedbackList = feedInterface.getListBasedOnType(pageIndex, pageSize, typeId);
            return View("Dashboard", feedbackList);
        }

       
        /*****************VIEW OPEN  TICKET********************/

        public ActionResult openview(string  id)
        {
            var param2 = new SqlParameter();
            param2.ParameterName = "@TotalCount";
            param2.SqlDbType = SqlDbType.Int;
            ViewData["commentList"] = feedInterface.getCOmments(id);
            ViewBag.fff = param2.Value;
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();              
                Feedback f = feedInterface.Find(id);
                return View("view",f);
            }
        }

        /*****************VIEW RESOLVED  TICKET********************/
        public ActionResult resolvedview(string id)
        {           
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }
        }

        /*****************VIEW ASSIGNED  TICKET********************/
        public ActionResult assignedview(string id)
        {          
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);

            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }
        }



        /*****************VIEW   TICKET********************/
        public ActionResult viewticket(string id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View("assignedview",f);
            }
        }



        /*****************VIEW RESPONDED  TICKET********************/
        public ActionResult respondedview(string id)
        {          
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }
        }

        /*****************VIEW CLOSED  TICKET********************/
        public ActionResult closedview(string id)
        {           
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }
        }


        /*****************VIEW REJECTED  TICKET********************/
        public ActionResult rejectedview(string id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            ViewData["commentList"] = feedInterface.getCOmments(id);
            if (id == null)
            {
                ViewBag.ErrorMsg = "This Ticket is not found,Try with proper data";
                return RedirectToAction("list");
            }
            else
            {
                getAttributeList();
                Feedback f = feedInterface.Find(id);
                return View(f);
            }
        }

        /*************DOWNLOAD ATTACHMENT*********/
        public void DownloadFile(string filename)
        {
            string myfile = Models.Constants.PATH + filename;        
            var fi = new FileInfo(myfile);
            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fi.Name);
            Response.WriteFile(myfile);
            Response.End();
        }

        /****** Get Ticket Counts********/
        public void TicketCounts()
        {
            ViewBag.TotalTickets = feedInterface.getAll().Count();
            ViewBag.OpenTickets = feedInterface.getAllOpen().Count();
            ViewBag.ClosedTickets = feedInterface.getAllClosed().Count();
            ViewBag.ResolvedTickets = feedInterface.getAllResolved().Count();
        }

        /*****Get Attribute List***************/
        public void getAttributeList()
        {
            ViewBag.Departmn = feedInterface.getDepartmentsOnType(Constants.FORWARD);
            ViewBag.Priorities = feedInterface.getPriorties();
            ViewBag.Emails = feedInterface.getEmails();
            ViewBag.typeList = feedInterface.getFeedbackTypes();

        }
        [HttpPost]
        public JsonResult getEmpDetails(string id)
        {
            ApplicationUser u = feedInterface.getEmpDetails(id);
            System.Diagnostics.Debug.WriteLine(u.Nationality.name+"lllllllllllllllllllllllll"+u.NationalityId +"djdsj"+u.saudiNationalId);
            return Json(feedInterface.getEmpDetails(id));
        }

        public ViewResult charts()
        {
            string chartsAll = (db.Feedbacks.Count()).ToString();
            string chartsOpen = (db.Feedbacks.Where(f => f.status == "Open").Count()).ToString();
            string chartsClosed = (db.Feedbacks.Where(f => f.status == "Closed").Count()).ToString();
            string chartsResolved = (db.Feedbacks.Where(f => f.status == "Resolved").Count()).ToString();
            ViewBag.All = chartsAll;
            ViewBag.Open = chartsOpen;
            ViewBag.Closed = chartsClosed;
            ViewBag.Resolved = chartsResolved;
            return View("DataCharts");
        }

        [HttpGet]
        [Route("hr/chartssearch/")]
        public ActionResult chartssearch(string date22, string date1)
        {
            string d3 = date22;
            string dd = date1;
            if (d3.Equals("") || dd.Equals(""))
            {
                TempData["DateMsg"] = "Please Select StartDate And EndDate";
                return RedirectToAction("DataCharts");
            }
            else
            {
                DateTime dt = DateTime.ParseExact(dd, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(d3, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                IEnumerable<Feedback> feedbacksopen = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), "Open");
                IEnumerable<Feedback> feedbacksclosed = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), "Closed");
                IEnumerable<Feedback> feedbacksresolved = feedInterface.searchHR(Convert.ToDateTime(dt).ToString("yyyy-MM-dd HH:mm:ss.fff"), Convert.ToDateTime(dt2).ToString("yyyy-MM-dd HH:mm:ss.fff"), "Resolved");        
                ViewBag.Open = feedbacksopen.Count();
                ViewBag.Closed = feedbacksclosed.Count();
                ViewBag.Resolved = feedbacksresolved.Count();
                ViewBag.All = feedbacksopen.Count() + feedbacksclosed.Count() + feedbacksresolved.Count();
                return View("DataCharts");

            }
        }

/*

        public ViewResult charts()
        {

            return View("DataCharts");
        }
*/

        /**************REJECT ACTION************************/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("rejected/action/")]
        public ActionResult feedbackupdate(string submitButton, Feedback feedback)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            feedback.user = db.Users.Find(feedback.userId);
            switch (submitButton)
            {
                case "Forward":
                    if (feedback.departmentID != null && Request.Form["responsee"] == "")
                    {
                        feedback.department = db.Departments.Find(feedback.departmentID);
                        feedback.checkStatus = Constants.ASSIGNED;                     
                            if (feedback.assignedDate == null)
                            {
                                feedback.assignedBy = user.Id;
                                feedback.assignedDate = DateTime.Now;                            
                            }
                                db.Entry(feedback).State = EntityState.Modified;
                                db.SaveChanges();
                        eventService.sendEmails(Request.Form["emailsss"], PopulateBody(feedback));
                        TempData["MessageSuccess"] = "Ticket has been Forwarded Successfully";

                    }
                    else
                    {
                        TempData["Message"] = "Select Department & Comment field should be empty";
                        return RedirectToAction("rejectedview", new { id = feedback.id });
                    }
                    return RedirectToAction("DashBoard");
                case "Resolve":
                    if (feedback.departmentID == null && Request.Form["responsee"] != "")
                    {
                        feedback.checkStatus = Constants.RESOLVED;
                            Comments c = new Comments();
                            c.text = Request.Form["responsee"];
                            c.commentedById = user.Id;
                            c.feedbackId = feedback.id;
                            db.comments.Add(c);
                            db.SaveChanges();
                            feedback.assignedBy = null;
                            feedback.assignedDate = null;
                            feedback.submittedById = user.Id;
                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["MessageSuccess"] = "Ticket has been Resolved Successfully";
                            return RedirectToAction("DashBoard");                     
                    }
                    else
                    {
                        TempData["Message"] = "Please fill Comment field & Department should be empty";
                        return RedirectToAction("rejectedview", new { id = feedback.id });
                    }
                case "Reject":
                    if (feedback.departmentID == null && Request.Form["responsee"] != "")
                    {
                        feedback.checkStatus = Constants.REJECTED;
                            Comments c = new Comments();
                            c.text = Request.Form["responsee"];
                            c.commentedById = user.Id;
                            c.feedbackId = feedback.id;
                            db.comments.Add(c);
                            db.SaveChanges();
                            feedback.submittedById = user.Id;           //----------------later will use this to check rejectedby                           
                            db.Entry(feedback).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["MessageSuccess"] = "Ticket has been Rejected";
                            return RedirectToAction("DashBoard");                    
                    }
                    else
                    {
                        TempData["Message"] = "Please fill Comment field & Department should be empty";
                        return RedirectToAction("rejectedview", new { id = feedback.id });
                    }
                default:
                    return RedirectToAction("rejectedview", new { id = feedback.id });
            }
        }

        /******** HR update status Closed to Open on User's satisfaction status******/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult updatestatus(Feedback feedback)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewData["user"] = user;
            Feedback f = db.Feedbacks.Find(feedback.id);
            f.status = feedback.status;
            f.checkStatus = feedback.status;
            if (ModelState.IsValid)
            {
                db.Entry(f).State = EntityState.Modified;
                db.SaveChanges();
                TempData["displayMsg"] = "Ticket has been Updated Successfully";
                ViewData["decide"] = feedInterface.getCOmments(feedback.id);
                return RedirectToAction("DashBoard");
            }
            else
            {
                TempData["displayMsgErr"] = "Please Enter Valid Information ";
                return RedirectToAction("view", new { name = "closedview", id = feedback.id });
            }
        }
      [HttpPost]
        public JsonResult getCategories(int depId)
        {
            List<Category> categories = feedInterface.getCategories(depId);               
            return Json(categories);
        }
      [HttpPost]
        public JsonResult getSubCategories(int categoryId,int type)
        {
            List<SubCategory> subCategories = feedInterface.getSubCategories(categoryId,type);
            return Json(subCategories);
        }

     public  string PopulateBody(Feedback feedback)
        {
            ApplicationUser user = db.Users.Find(feedback.userId);
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/HR/HRemail.html")))
            {
                body = reader.ReadToEnd();
            }      
            body = body.Replace("{Title}", feedback.title);
            body = body.Replace("{TicketId}", feedback.id);
            
         body = body.Replace("{Location}", user.Location.name);
            
            
            body = body.Replace("{EmployeeId}", user.EmployeeId.ToString());
           body = body.Replace("{Description}",feedback.description);
           body = body.Replace("{email}", user.bussinessEmail);
            body = body.Replace("{issueClass}", "YES");
            
           if (feedback.attachment == null) {
               body = body.Replace("{Attachment}", "No");
           }
           else {
               body = body.Replace("{Attachment}", "Yes");
            }           
            body = body.Replace("{IssueEscalate}", "Yes"); 
            return body;
        }

        [HttpGet]
        public ActionResult HRemail() {

            return View();
        }
    }


    
}