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
    public class EventReasonsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EventReasons
        public ActionResult Index()
        {
            return View(db.EventReasons.ToList());
        }

        // GET: EventReasons/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventReason eventReason = db.EventReasons.Find(id);
            if (eventReason == null)
            {
                return HttpNotFound();
            }
            return View(eventReason);
        }

        // GET: EventReasons/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EventReasons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] EventReason eventReason)
        {
            if (ModelState.IsValid)
            {
                db.EventReasons.Add(eventReason);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(eventReason);
        }

        // GET: EventReasons/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventReason eventReason = db.EventReasons.Find(id);
            if (eventReason == null)
            {
                return HttpNotFound();
            }
            return View(eventReason);
        }

        // POST: EventReasons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] EventReason eventReason)
        {
            if (ModelState.IsValid)
            {
                db.Entry(eventReason).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(eventReason);
        }

        // GET: EventReasons/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventReason eventReason = db.EventReasons.Find(id);
            if (eventReason == null)
            {
                return HttpNotFound();
            }
            return View(eventReason);
        }

        // POST: EventReasons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EventReason eventReason = db.EventReasons.Find(id);
            db.EventReasons.Remove(eventReason);
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
