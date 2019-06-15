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
    public class JobTitlesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: JobTitles
        public ActionResult Index()
        {
            return View(db.JobTitles.ToList());
        }

        // GET: JobTitles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobTitle jobTitle = db.JobTitles.Find(id);
            if (jobTitle == null)
            {
                return HttpNotFound();
            }
            return View(jobTitle);
        }

        // GET: JobTitles/Create
        public ActionResult Create()
        {

            ViewBag.Status = "Add";
            return View("CreateList",new JobTitlesViewModel { jobTitles = db.JobTitles.ToList()});
        }

        // POST: JobTitles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] JobTitle jobTitle)
        {
            if (ModelState.IsValid)
            {
                db.JobTitles.Add(jobTitle);
                db.SaveChanges();
                TempData["Success"] = "Job Title has been created Successfully";
                return RedirectToAction("Create");
            }
            TempData["Fail"] = "Enter fields properly";
            ViewBag.Status = "Add";
            return View("CreateList", new JobTitlesViewModel { jobTitles = db.JobTitles.ToList() });
        }

        // GET: JobTitles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobTitle jobTitle = db.JobTitles.Find(id);
            if (jobTitle == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Update";
            return View("CreateList", new JobTitlesViewModel {jobTitle = jobTitle,  jobTitles = db.JobTitles.ToList() });
        }

        // POST: JobTitles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] JobTitle jobTitle)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jobTitle).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Job Title has been Updated Successfully";
                return RedirectToAction("Create");
            }
            TempData["Fail"] = "Enter fields properly";
            ViewBag.Status = "Update";
            return View("CreateList", new JobTitlesViewModel { jobTitles = db.JobTitles.ToList() });
        }

        // GET: JobTitles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobTitle jobTitle = db.JobTitles.Find(id);
            if (jobTitle == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "Delete";
            return View("CreateList", new JobTitlesViewModel {jobTitle = jobTitle, jobTitles = db.JobTitles.ToList() });
        }

        // POST: JobTitles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JobTitle jobTitle = db.JobTitles.Find(id);
            db.JobTitles.Remove(jobTitle);
            db.SaveChanges();
            TempData["Success"] = "Job Title is deleted Successfully";
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
