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
    [Authorize(Roles = "Admin")]
    public class NationalitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Nationalities
        
        public ActionResult Index()
        {
            return View(db.Nationalities.ToList());
        }

        // GET: Nationalities/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nationality nationality = db.Nationalities.Find(id);
            if (nationality == null)
            {
                return HttpNotFound();
            }
            return View(nationality);
        }

        // GET: Nationalities/Create
        public ActionResult Create()
        {
            @ViewBag.Status = "Add";
            return View("CreateList",new NationalitiesViewModel { Nationalities = db.Nationalities.ToList()});
        }

        // POST: Nationalities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] Nationality nationality)
        {
            if (ModelState.IsValid)
            {
                db.Nationalities.Add(nationality);
                db.SaveChanges();
                TempData["Success"] = "Nationality has been created Successfully";

                return RedirectToAction("Create");
            }
            TempData["Fail"] = "Enter fields properly";
            @ViewBag.Status = "Add";
            return View("CreateList", new NationalitiesViewModel { Nationalities = db.Nationalities.ToList() });
        }

        // GET: Nationalities/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nationality nationality = db.Nationalities.Find(id);
            if (nationality == null)
            {
                return HttpNotFound();
            }
            @ViewBag.Status = "Update";
            return View("CreateList", new NationalitiesViewModel { Nationality = nationality, Nationalities = db.Nationalities.ToList() });
        }

        // POST: Nationalities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] Nationality nationality)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nationality).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Nationality has been Updated Successfully";

                return RedirectToAction("Create");
            }
            TempData["Fail"] = "Enter fields properly";
            @ViewBag.Status = "Update";
            return View("CreateList", new NationalitiesViewModel { Nationalities = db.Nationalities.ToList() });
        }

        // GET: Nationalities/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nationality nationality = db.Nationalities.Find(id);
            if (nationality == null)
            {
                return HttpNotFound();
            }
            @ViewBag.Status = "Delete";
            return View("CreateList", new NationalitiesViewModel { Nationality = nationality, Nationalities = db.Nationalities.ToList() });
        }

        // POST: Nationalities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Nationality nationality = db.Nationalities.Find(id);
            db.Nationalities.Remove(nationality);
            db.SaveChanges();
            TempData["Success"] = "Nationality is deleted Successfully";

            return RedirectToAction("Create");
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
