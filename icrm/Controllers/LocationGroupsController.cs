﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using icrm.Models;

namespace icrm.Controllers
{
    public class LocationGroupsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: LocationGroups
        public ActionResult Index()
        {
            return View(db.LocationGroups.ToList());
        }

        // GET: LocationGroups/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocationGroup locationGroup = db.LocationGroups.Find(id);
            if (locationGroup == null)
            {
                return HttpNotFound();
            }
            return View(locationGroup);
        }

        // GET: LocationGroups/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LocationGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] LocationGroup locationGroup)
        {
            if (ModelState.IsValid)
            {
                db.LocationGroups.Add(locationGroup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(locationGroup);
        }

        // GET: LocationGroups/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocationGroup locationGroup = db.LocationGroups.Find(id);
            if (locationGroup == null)
            {
                return HttpNotFound();
            }
            return View(locationGroup);
        }

        // POST: LocationGroups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] LocationGroup locationGroup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(locationGroup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(locationGroup);
        }

        // GET: LocationGroups/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocationGroup locationGroup = db.LocationGroups.Find(id);
            if (locationGroup == null)
            {
                return HttpNotFound();
            }
            return View(locationGroup);
        }

        // POST: LocationGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LocationGroup locationGroup = db.LocationGroups.Find(id);
            db.LocationGroups.Remove(locationGroup);
            db.SaveChanges();
            return RedirectToAction("Index");
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
