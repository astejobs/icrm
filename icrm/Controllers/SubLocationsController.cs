﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using icrm.Models;

namespace icrm.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubLocationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SubLocations
        public ActionResult Index()
        {
            return View(db.SubLocations.ToList());
        }

        // GET: SubLocations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubLocation subLocation = db.SubLocations.Find(id);
            if (subLocation == null)
            {
                return HttpNotFound();
            }
            return View(subLocation);
        }

        // GET: SubLocations/Create
        public ActionResult Create()
        {
            @ViewBag.Status = "Add";
            ViewBag.Locationlist = db.Locations.ToList();
            return View("CreateList", new SubLocationViewModel { SubLocations = db.SubLocations.ToList()});
        }

        // POST: SubLocations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name,locationId")] SubLocation subLocation)
        {
            if (ModelState.IsValid)
            {
               
                db.SubLocations.Add(subLocation);
                db.SaveChanges();
                TempData["Success"] = "SubLocations has been created Successfully";
                return RedirectToAction("Create");
            }
            TempData["Fail"] = "Enter fields properly";
            @ViewBag.Status = "Add";
            ViewBag.Locationlist = db.Locations.ToList();
           return View("CreateList", new SubLocationViewModel { SubLocations = db.SubLocations.ToList() });
        }

        // GET: SubLocations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubLocation subLocation = db.SubLocations.Find(id);
            if (subLocation == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Update";
            ViewBag.Locationlist = db.Locations.ToList();
            return View("CreateList", new SubLocationViewModel { SubLocation = subLocation, SubLocations = db.SubLocations.ToList() });
        }

        // POST: SubLocations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name,locationId")] SubLocation subLocation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(subLocation).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "SubLocations has been Updated Successfully";
                return RedirectToAction("Create");
            }
            TempData["Fail"] = "Enter fields properly";
            ViewBag.Status = "Update";
            ViewBag.Locationlist = db.Locations.ToList();
           return View("CreateList", new SubLocationViewModel { SubLocations = db.SubLocations.ToList() });
        }

        // GET: SubLocations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubLocation subLocation = db.SubLocations.Find(id);
            if (subLocation == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Delete";
            ViewBag.Locationlist = db.Locations.ToList();
            return View("CreateList", new SubLocationViewModel { SubLocation = subLocation, SubLocations = db.SubLocations.ToList() });
        }

        // POST: SubLocations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SubLocation subLocation = db.SubLocations.Find(id);
            db.SubLocations.Remove(subLocation);
            db.SaveChanges();
            TempData["Success"] = "SubLocations is deleted Successfully";
            return RedirectToAction("Create");
        }

        [HttpPost]
        public JsonResult getSublocations(int? id) {
            Debug.WriteLine(db.SubLocations.Where(s => s.Location.Id == id).Count()+"9090090909090909090909099");
           return Json(db.SubLocations.Where(s => s.Location.Id == id).ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
