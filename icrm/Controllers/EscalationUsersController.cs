using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using icrm.Models;
using icrm.RepositoryImpl;
using icrm.RepositoryInterface;

namespace icrm.Controllers
{
    public class EscalationUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserInterface userInterface;
        private IFeedback feedInterface;
        public EscalationUsersController()
        {
            userInterface = new UserRepository();
            feedInterface = new FeedbackRepository();
        }


        // GET: EscalationUsers
        public ActionResult Index()
        {
            return View(db.EscalationUsers.ToList());
        }

        // GET: EscalationUsers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EscalationUser escalationUser = db.EscalationUsers.Find(id);
            if (escalationUser == null)
            {
                return HttpNotFound();
            }
            return View(escalationUser);
        }

        // GET: EscalationUsers/Create
        public ActionResult Create()
        {
            ViewBag.CostCenterList = db.CostCenters.OrderBy(m=>m.CostCenterCode).ToList();
            ViewBag.DepartmentList = db.Departments.Where(m=>m.type== Constants.FORWARD).ToList();
            ViewBag.UserList = db.Users.ToList();
            //ViewBag.Categories = db.Categories.Where(c=>c.EscalationUserId== null).ToList();
            ViewBag.Status = "Add";
            return View("CreateList", new EscalationUserViewModel { EscalationUser= new EscalationUser(), EscalationUsers = db.EscalationUsers.ToList()});
        }

        // POST: EscalationUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EscalationUser escalationUser)
        {
            if (!db.Departments.Find(escalationUser.DepartmentId).name.Equals(Constants.OPERATIONS)) {
                if (escalationUser.CategoriesIds == null) {
                    ModelState.AddModelError("add_categories","Please Add Categories for The Departments other than Operations");
                }
            }
            Debug.WriteLine(ModelState.IsValid + "0-----------------------");
            if (ModelState.IsValid)
            {
                if (!db.Departments.Find(escalationUser.DepartmentId).name.Equals(Constants.OPERATIONS))
                {
                    db.EscalationUsers.Add(escalationUser);
                    db.SaveChanges();
                    foreach (int cid in escalationUser.CategoriesIds)
                    {
                        Category c = db.Categories.Find(cid);
                        c.EscalationUserId = escalationUser.Id;
                        db.Entry(c).State = EntityState.Modified;
                        db.SaveChanges();

                        TempData["SuccessMsg"] = "Escalation User Added";
                        return RedirectToAction("Create");
                    }

                }
                else {
                    if (userInterface.getEscalationUserCostCentr(escalationUser) <= 0)
                    {
                        db.EscalationUsers.Add(escalationUser);
                        db.SaveChanges();
                        TempData["SuccessMsg"] = "Escalation User Added";
                        return RedirectToAction("Create");

                    }
                    else
                    {
                        TempData["FailMsg"] = "Cost Center is already assigned to Escalation User";
                    }
                }
               
            }
            ViewBag.CostCenterList = db.CostCenters.OrderBy(m => m.CostCenterCode).ToList();
            ViewBag.DepartmentList = db.Departments.Where(m => m.type == Constants.FORWARD).ToList();
            ViewBag.UserList = db.Users.ToList();
            ViewBag.Categories = db.Categories.Where(m => m.DepartmentId == escalationUser.DepartmentId && m.FeedBackType.name == Constants.Complaints).ToList();
            ViewBag.Status = "Add";
            return View("CreateList", new EscalationUserViewModel { EscalationUsers = db.EscalationUsers.ToList() });
        }

        // GET: EscalationUsers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EscalationUser escalationUser = db.EscalationUsers.Find(id);
            List<Category> category = db.Categories.Where(c => c.EscalationUserId == id).ToList();
            List<int> categoryids = new List<int>();

            foreach (Category i in category) {
                categoryids.Add(i.Id);
            }
            escalationUser.CategoriesIds = categoryids;
         
            if (escalationUser == null)
            {
                return HttpNotFound();
            }

            ViewBag.CostCenterList = db.CostCenters.OrderBy(m => m.CostCenterCode).ToList();
            ViewBag.DepartmentList = db.Departments.Where(m => m.type == Constants.FORWARD).ToList();
            ViewBag.UserList = db.Users.ToList();
            ViewBag.Categories = db.Categories.Where(m=>m.DepartmentId == escalationUser.DepartmentId && m.FeedBackType.name == Constants.Complaints).ToList();
            ViewBag.Status = "Update";
            return View("CreateList", new EscalationUserViewModel {EscalationUser = escalationUser, EscalationUsers = db.EscalationUsers.ToList() });
        }

        // POST: EscalationUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( EscalationUser escalationUser)
        {
            if (!db.Departments.Find(escalationUser.DepartmentId).name.Equals(Constants.OPERATIONS))
            {
                if (escalationUser.CategoriesIds == null)
                {
                    ModelState.AddModelError("add_categories", "Please Add Categories for The Departments other than Operations");
                }
            }
            if (ModelState.IsValid)
            {

                List<Category> categories = db.Categories.Where(c => c.EscalationUserId == escalationUser.Id).ToList();
              
                if (!db.Departments.Find(escalationUser.DepartmentId).name.Equals(Constants.OPERATIONS))
                {
                    foreach (Category i in categories)
                    {
                        i.EscalationUserId = null;
                        db.Entry(i).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    db.Entry(escalationUser).State = EntityState.Modified;
                    db.SaveChanges();
                    foreach (int cid in escalationUser.CategoriesIds)
                    {
                        Category c = db.Categories.Find(cid);
                        c.EscalationUserId = escalationUser.Id;
                        db.Entry(c).State = EntityState.Modified;
                        db.SaveChanges();
                       

                    }
                    TempData["SuccessMsg"] = "Escalation User has been Updated";
                    return RedirectToAction("Create");
                }
                else
                {
                    if (userInterface.getEscalationUserCostCentr(escalationUser) <= 0)
                    {
                        db.Entry(escalationUser).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["SuccessMsg"] = "Escalation User has been Updated";
                        return RedirectToAction("Create");

                    }
                    else
                    {
                        TempData["FailMsg"] = "Cost Center is already assigned to Escalation User";
                    }
                }

            }

            ViewBag.CostCenterList = db.CostCenters.OrderBy(m => m.CostCenterCode).ToList();
            ViewBag.DepartmentList = db.Departments.Where(m => m.type == Constants.FORWARD).ToList();
            ViewBag.UserList = db.Users.ToList();

            //ViewBag.Categories = db.Categories.Where(c => c.EscalationUserId == null).ToList();
            ViewBag.Status = "Update";
            // return RedirectToAction("CreateList", new EscalationUserViewModel { EscalationUsers = db.EscalationUsers.ToList() });
            return RedirectToAction("Edit", new { id = escalationUser.Id });
        }

        // GET: EscalationUsers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EscalationUser escalationUser = db.EscalationUsers.Find(id);
            List<Category> category = db.Categories.Where(c => c.EscalationUserId == id).ToList();
            List<int> categoryids = new List<int>();

            foreach (Category i in category)
            {
                categoryids.Add(i.Id);
            }
            escalationUser.CategoriesIds = categoryids;
            if (escalationUser == null)
            {
                return HttpNotFound();
            }

            ViewBag.CostCenterList = db.CostCenters.OrderBy(m => m.CostCenterCode).ToList();
            ViewBag.DepartmentList = db.Departments.Where(m => m.type == Constants.FORWARD).ToList();
            ViewBag.UserList = db.Users.ToList();
            ViewBag.Categories = db.Categories.Where(m => m.DepartmentId == escalationUser.DepartmentId && m.FeedBackType.name == Constants.Complaints).ToList();
            ViewBag.Status = "Delete";
            return View("CreateList", new EscalationUserViewModel {EscalationUser = escalationUser, EscalationUsers = db.EscalationUsers.ToList() });
        }

        // POST: EscalationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (db.EscalationUsers.Find(id).CostCenterId != null)
            {
                if (feedInterface.findByCostCentr(db.EscalationUsers.Find(id).CostCenterId))
                {

                    TempData["FailMsg"] = "Escalation User cannot be deleted,as it is assigned to a ticket";
                    return RedirectToAction("Delete", new { id = id });
                }
                else
                {
                    deleteEscalationUser(id);
                    return RedirectToAction("Create");
                }
            }
            else 
            {
                if (feedInterface.getTicketsOnCategory(id))
                {
                    TempData["FailMsg"] = "Escalation User cannot be deleted,as it is assigned to a ticket";
                    return RedirectToAction("Delete", new { id = id });
                }
                else {
                    deleteEscalationUser(id);
                    return RedirectToAction("Create");
                }
            }
           
        }

        public void deleteEscalationUser(int id) {
            List<Category> categories = db.Categories.Where(c => c.EscalationUserId == id).ToList();
            foreach (Category i in categories)
            {
                i.EscalationUserId = null;
                db.Entry(i).State = EntityState.Modified;
                db.SaveChanges();
            }
            EscalationUser escalationUser = db.EscalationUsers.Find(id);
            db.EscalationUsers.Remove(escalationUser);
            db.SaveChanges();
            TempData["SuccessMsg"] = "Escalation User is deleted Successfully";
          

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public JsonResult getCategories(int depId)
        {

            List<Category> categories = db.Categories.Where(m=>m.DepartmentId== depId && m.FeedBackType.name==Constants.Complaints && m.EscalationUserId == null).ToList();
            return Json(categories);
        }


    }
}
