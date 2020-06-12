﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using icrm.Models;
using icrm.RepositoryInterface;
using PagedList.Mvc;
using PagedList;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Data.SqlClient;
using System.Data;
using Constants = icrm.Models.Constants;
using System.Data.Entity;

namespace icrm.RepositoryImpl
{
    public class FeedbackRepository : IFeedback
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public void Save(Feedback feedback)
        {

            db.Feedbacks.Add(feedback);

            db.SaveChanges();

        }

        public Feedback Find(string id)
        {
            return db.Feedbacks.Find(id);
        }


        public IPagedList<Feedback> Pagination(int pageIndex, int pageSize, string userId)
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).Where(m => m.user.Id == userId).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllAssigned(int pageIndex, int pageSize)
        {
            var param = new SqlParameter();
            param.ParameterName = "@status";
            param.SqlDbType = SqlDbType.VarChar;
            param.SqlValue = Constants.ASSIGNED;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllAssigned @status",param).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.OrderByDescending(m=>m.createDate).ToPagedList(pageIndex, pageSize);

        }

        public IEnumerable<Feedback> searchlist(DateTime d1, DateTime d2) {

            
            IEnumerable<Feedback> feedbacks = db.Feedbacks.ToList();
            var query = from f in feedbacks
                        where (f.createDate >= d1 && f.createDate <= d2)
                        select f;
            return query.ToList();
                        

        }
        public IPagedList<Feedback> search(string d1, string d2, string status, string id,int pageIndex, int pageSize)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";
         
            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Status";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = status;


            var param4 = new SqlParameter();
            param4.ParameterName = "@id";
            param4.SqlDbType = SqlDbType.VarChar;
            param4.SqlValue = id;

           

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("search @D1,@D2,@Status,@id", param1,param2,param3,param4).ToList();
            foreach (var r in result) {
                feedlist.Add(r);
            }
            
            return feedlist.OrderByDescending(m=>m.createDate).ToPagedList(pageIndex,pageSize);
        }
      
        public IEnumerable<Feedback> getAll()
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).ToList();

        }

        public IPagedList<Feedback> getAll(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).ToPagedList(pageIndex,pageSize);

        }

        public IEnumerable<Feedback> getAllOpen()
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).Where(m=>m.checkStatus== Models.Constants.OPEN && m.type.name==Constants.Complaints).ToList();

        }


        public IEnumerable<Feedback> getRespondedDepartmenet()
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).ToList();

        }


        public IEnumerable<Feedback> getAllOpenMobile()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.checkStatus == Models.Constants.OPEN).ToList();

        }



        public IPagedList<Feedback> getAllOpenWithDepartment(string usrid, int pageIndex, int pageSize)
        {
            //ApplicationUser user = db.Users.Find(usrid);
            //var param1 = new SqlParameter();
            //param1.ParameterName = "@depID";
            //param1.SqlDbType = SqlDbType.VarChar;
            //param1.SqlValue = user.DepartmentId;

            //var param2 = new SqlParameter();
            //param2.ParameterName = "@status";
            //param2.SqlDbType = SqlDbType.VarChar;
            //param2.SqlValue = Constants.ASSIGNED;

            //List<Feedback> feedlist = new List<Feedback>();
            //var result = db.Feedbacks.SqlQuery("getAllOpenWithDepart @depID,@status", param1,param2).ToList();
            //foreach (var r in result)
            //{
            //    feedlist.Add(r);
            //}

           // return feedlist.ToPagedList(pageIndex, pageSize);

            return db.Feedbacks.OrderByDescending(m=>m.createDate).Where(m=>m.departUserId==usrid && m.checkStatus==Constants.ASSIGNED).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllRespondedWithDepartment(string usrid, int pageIndex, int pageSize)
        {
            ApplicationUser user = db.Users.Find(usrid);
            var param1 = new SqlParameter();
            param1.ParameterName = "@depID";
            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = user.DepartmentId;

            var param2 = new SqlParameter();
            param2.ParameterName = "@CommentedByID";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = usrid;

            var param3 = new SqlParameter();
            param3.ParameterName = "@status";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = Constants.RESPONDED;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllRespondedWithDepart @depID,@CommentedByID", param1,param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.OrderByDescending(m=>m.createDate).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> OpenWithoutDepart(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).Where(m => m.departmentID == null  && m.checkStatus==Constants.OPEN).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllResolved(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.resolvedDate).Where(m =>  m.status == Constants.RESOLVED && m.type.name==Constants.Complaints).ToPagedList(pageIndex, pageSize);

        }
        public IPagedList<Feedback> getAllOpen(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).Where(m => m.checkStatus == Constants.OPEN && m.type.name==Constants.Complaints).ToPagedList(pageIndex, pageSize);

        }

        public IPagedList<Feedback> getAllResponded(int pageIndex, int pageSize)
        {
            

            return db.Feedbacks.OrderByDescending(m=>m.createDate).Where(m=>m.checkStatus==Constants.RESPONDED).ToPagedList(pageIndex, pageSize);

        }

        public IEnumerable<Feedback> GetAllResponded()
        {


            return db.Feedbacks.OrderByDescending(m => m.createDate).Where(m => m.checkStatus == Constants.RESPONDED).ToList();

        }



        public IEnumerable<Feedback> GetAllRespondedForDepart(string id)
        {
                                                                                                                      

            return db.comments.OrderByDescending(m => m.feedback.createDate).Where(m => m.commentedById == id).Select(m=>m.feedback).ToList().Distinct();

        }



        public IEnumerable<Feedback> GetAllRespondedMobile()
        {


            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.checkStatus == Constants.RESPONDED).ToList();

        }

        public IPagedList<Feedback> getAllClosed(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.closedDate).Where(m => m.status == Constants.CLOSED && m.type.name == Constants.Complaints).ToPagedList(pageIndex, pageSize);
        }
        public IEnumerable<Feedback> getAllClosed()
        {
            return db.Feedbacks.OrderByDescending(m => m.closedDate).Where(m => m.status == Constants.CLOSED && m.type.name==Constants.Complaints).ToList();
        }

        public IEnumerable<Feedback> GETAllClosed()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.status == Constants.CLOSED ).ToList();
        }


        public IEnumerable<Feedback> getAllResolved()
        {
            return db.Feedbacks.OrderByDescending(m => m.resolvedDate).Where(m =>  m.status == Constants.RESOLVED && m.type.name==Constants.Complaints).ToList();
        }

        public IEnumerable<Feedback> GetAllResolvedmobile()
        {
            return db.Feedbacks.OrderByDescending(m => m.id).Where(m => m.status == Constants.RESOLVED).ToList();
        }


        public IEnumerable<Feedback> GetAllAssigned()
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).Where(m => m.checkStatus == Constants.ASSIGNED).ToList();
        }


        public IPagedList<Feedback> searchHR(string d1, string d2, string status, int pageIndex, int pageSize)
        {
            
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Status";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = status;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("searchHR @D1,@D2,@Status", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.OrderByDescending(m=>m.createDate).ToPagedList(pageIndex, pageSize);
        }


        public IEnumerable<Feedback> searchHR(string d1, string d2, string status)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Status";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = status;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("searchHR @D1,@D2,@Status", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.OrderByDescending(m=>m.createDate).ToList();
        }

        public IEnumerable<Feedback> chartsFeedbackTypes(string d1, string d2, string type)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Type";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = type;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackTypes @D1,@D2,@Type", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }

        public IEnumerable<Feedback> chartsFeedbackSource(string d1, string d2, string source)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Source";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = source;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackSource @D1,@D2,@Source", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }

        public IEnumerable<Feedback> chartsFeedbackDepartment(string d1, string d2, string department)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Department";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = department;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackDepartment @D1,@D2,@Department", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }

        public IEnumerable<Feedback> chartsFeedbackPosition(string d1, string d2, string position)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Position";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = position;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackPosition @D1,@D2,@Position", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }


        public IEnumerable<Feedback> chartsFeedbackNationalitySaudi(string d1, string d2)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackNationalitySaudi @D1,@D2", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }

        public IEnumerable<Feedback> chartsFeedbackNationalityExpatriates(string d1, string d2)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackNationalityExpartriates @D1,@D2", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }


        /* chartsFeedbackSatisfaction */
        public IEnumerable<Feedback> chartsFeedbackSatisfaction(string d1, string d2, string satisfaction)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Satisfaction";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = satisfaction;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackSatisfaction @D1,@D2,@Satisfaction", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }


        //chartsFeedbackEscalation

        public IEnumerable<Feedback> chartsFeedbackEscalation(string d1, string d2, string escalation)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Escalation";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = escalation;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackEscalation @D1,@D2,@Escalation", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }


        public IEnumerable<Feedback> chartsFeedbackSalaryTimeSheet(string d1, string d2)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackSalaryTimeSheet @D1,@D2", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }
        public IEnumerable<Feedback> chartsFeedbackHousing(string d1, string d2)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackHousing @D1,@D2", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }
        public IEnumerable<Feedback> chartsFeedbackPoorTreatment(string d1, string d2)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackPoorTreatment @D1,@D2", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }

        public IEnumerable<Feedback> chartsFeedbackAccomodationSupplies(string d1, string d2)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackAccomodationSupplies @D1,@D2", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }


        public IEnumerable<Feedback> chartsFeedbackSalaryIssuesReasons(string d1, string d2, string salaryissuesreasons)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@SalaryIssuesReasons";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = salaryissuesreasons;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackSalaryIssuesReasons @D1,@D2,@SalaryIssuesReasons", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }




        public string[] chartsFeedbackMostFrequentLocations(string d1, string d2)
        {


            var total = 1;
            var locationid = 1;
            var locationname = "";


            try
            {
                var sql = "select top 1 COUNT(*) as c From Feedbacks inner join AspNetUsers on Feedbacks.userId=AspNetUsers.Id inner join Locations on AspNetUsers.LocationId=Locations.Id Where    month(createDate)=" + d1 + " and YEAR(createDate)=" + d2 + " group by AspNetUsers.LocationId order by c asc";

                total = db.Database.SqlQuery<int>(sql).First();
                sql = "select top 1 LocationId From Feedbacks inner join AspNetUsers on Feedbacks.userId=AspNetUsers.Id inner join Locations on AspNetUsers.LocationId=Locations.Id Where    month(createDate)=" + d1 + " and YEAR(createDate)=" + d2 + " group by AspNetUsers.LocationId order by count(*) asc";

                locationid = db.Database.SqlQuery<int>(sql).First();
                sql = "select name From Locations  Where  Id=" + locationid;

                locationname = db.Database.SqlQuery<string>(sql).First();
            }
            catch
            {

                total = 0;
                locationname = "No Locations";
            }
            string[] locationnamec = new string[2];
            locationnamec[0] = total.ToString();
            locationnamec[1] = locationname;

            return locationnamec;
        }

        public IEnumerable<Feedback> chartsFeedbackAll(string d1, string d2)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;






            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackAll @D1,@D2", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }

        public IEnumerable<Feedback> getAllAssigned()
        {
            var param = new SqlParameter();
            param.ParameterName = "@status";
            param.SqlDbType = SqlDbType.VarChar;
            param.SqlValue = Models.Constants.ASSIGNED;
            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllAssigned @status",param).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.OrderByDescending(m=>m.createDate).ToList();
        }

        public IEnumerable<Feedback> getAllResponded()
        {
            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllResponded").ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }
            return feedlist.OrderByDescending(m => m.createDate).ToList();
        }

        public IEnumerable<Feedback> OpenWithoutDepart()
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).Where(m => m.departmentID == null && m.checkStatus == Constants.OPEN).ToList();
        }

        public IPagedList<Feedback> getAllRejected(int pageIndex, int pageSize)
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).Where(m => m.checkStatus == Constants.REJECTED).ToPagedList(pageIndex,pageSize);

        }

      
        public IEnumerable<Feedback> getAllRespondedWithDepartment(string usrid)
        {
            ApplicationUser user = db.Users.Find(usrid);
            var param1 = new SqlParameter();
            param1.ParameterName = "@depID";
            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = user.DepartmentId;

            var param2 = new SqlParameter();
            param2.ParameterName = "@CommentedByID";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = usrid;

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllRespondedWithDepart @depID,@CommentedByID", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }
            return feedlist.OrderByDescending(m => m.createDate).ToList(); ;

        }



       

        public List<Comments> getCOmments(string id)
        {
            return db.comments.Include("commentedBy").Where(m => m.feedbackId == id).ToList();
        }

        public List<Category> getCategories(int deptId,int type)
        {
          return  db.Categories.Where(m => m.DepartmentId == deptId && m.FeedBackTypeId==type).ToList();
        }

        public List<SubCategory> getSubCategories(int categoryId,int type)
        {
            return db.SubCategories.Where(m => m.CategoryId == categoryId && m.FeedBackTypeId==type).ToList();
        }

        public List<string> getEmails()
        {
            List<string> emailList = new List<string>();
             List<ApplicationUser> u= db.Users.OrderBy(m => m.Id).Where(m=>m.forwarDeptEmailCCStatus==true).ToList();

            
            foreach (ApplicationUser uu in u) {
                emailList.Add(uu.bussinessEmail);
                System.Diagnostics.Debug.WriteLine(uu.Email+"---------------------jjjj");
            }
            return emailList;
        }

        public IPagedList<Feedback> getListBasedOnType(int pageIndex, int pageSize,string typeId)
        {
            return db.Feedbacks.OrderByDescending(m=>m.createDate).Where(m => m.type.name==typeId).ToPagedList(pageIndex,pageSize);
        }


        public IEnumerable<Feedback> GetListBasedOnType(string type)
        {
            return db.Feedbacks.OrderByDescending(m => m.createDate).Where(m => m.type.name == type).ToList();
        }
        public List<Department> getDepartmentsOnType(string fORWARD)
        {
            return db.Departments.OrderBy(m => m.name).Where(m => m.type == fORWARD).ToList();
        }

        public List<Priority> getPriorties()
        {
            return db.Priorities.OrderBy(m => m.priorityId).ToList();
        }

        public List<FeedBackType> getFeedbackTypes()
        {
            return db.FeedbackTypes.OrderBy(m=>m.name).ToList();
        }

        public ApplicationUser getEmpDetails(int id)
        {
            return db.Users.Where(u => u.EmployeeId == id).FirstOrDefault();
        }

        public IEnumerable<Feedback> getAllByDept(string id)
        {
            //ApplicationUser user = db.Users.Find(id);
            //var param1 = new SqlParameter();
            //param1.ParameterName = "@depID";
            //param1.SqlDbType = SqlDbType.VarChar;
            //param1.SqlValue = user.DepartmentId;


            //List<Feedback> feedlist = new List<Feedback>();
            //var result = db.Feedbacks.SqlQuery("getAllWithDepart @depID", param1).ToList();
            //foreach (var r in result)
            //{
            //    feedlist.Add(r);
            //}

            return db.Feedbacks.Where(m => m.departUserId == id).OrderByDescending(m=>m.createDate).ToList();

        }

        public IEnumerable<Feedback> getAllOpenByDept(string id)
        {
            //ApplicationUser user = db.Users.Find(id);
            //var param1 = new SqlParameter();
            //param1.ParameterName = "@depID";
            //param1.SqlDbType = SqlDbType.VarChar;
            //param1.SqlValue = user.DepartmentId;

            //var param2 = new SqlParameter();
            //param2.ParameterName = "@status";
            //param2.SqlDbType = SqlDbType.VarChar;
            //param2.SqlValue = Constants.OPEN;



            //List<Feedback> feedlist = new List<Feedback>();
            //var result = db.Feedbacks.SqlQuery("getAllWithDepartStatus @depID,@status", param1, param2).ToList();
            //foreach (var r in result)
            //{
            //    feedlist.Add(r);
            //}
            //return feedlist;

            return db.Feedbacks.Where(m => m.departUserId == id && m.status == Constants.OPEN && m.type.name==Constants.Complaints).OrderByDescending(m => m.createDate).ToList();
        }

        public IEnumerable<Feedback> getAllClosedByDept(string id)
        {
            //ApplicationUser user = db.Users.Find(id);
            //var param1 = new SqlParameter();
            //param1.ParameterName = "@depID";
            //param1.SqlDbType = SqlDbType.VarChar;
            //param1.SqlValue = user.DepartmentId;

            //var param2 = new SqlParameter();
            //param2.ParameterName = "@Status";
            //param2.SqlDbType = SqlDbType.VarChar;
            //param2.SqlValue = Constants.CLOSED;



            //List<Feedback> feedlist = new List<Feedback>();
            //var result = db.Feedbacks.SqlQuery("getAllWithDepartStatus @depID,@Status", param1, param2).ToList();
            //foreach (var r in result)
            //{
            //    feedlist.Add(r);
            //}
            //return feedlist;

            return db.Feedbacks.Where(m=>m.departUserId==id && m.status==Constants.CLOSED && m.type.name==Constants.Complaints).OrderByDescending(m => m.closedDate).ToList();

        }

        public IEnumerable<Feedback> getAllResolvedByDept(string id)
        {

            // ApplicationUser user = db.Users.Find(id);
            // var param1 = new SqlParameter();
            // param1.ParameterName = "@depID";
            // param1.SqlDbType = SqlDbType.VarChar;
            // param1.SqlValue = user.DepartmentId;

            // var param2 = new SqlParameter();
            // param2.ParameterName = "@status";
            // param2.SqlDbType = SqlDbType.VarChar;
            // param2.SqlValue = Constants.RESOLVED;

            ///* var param3 = new SqlParameter();
            // param3.ParameterName = "@CommentedByID";
            // param3.SqlDbType = SqlDbType.VarChar;
            // param3.SqlValue = id;*/

            // List<Feedback> feedlist = new List<Feedback>();
            // var result = db.Feedbacks.SqlQuery("getAllWithDepartStatus @depID,@status", param1, param2).ToList();
            // foreach (var r in result)
            // {
            //     feedlist.Add(r);
            // }
            // return feedlist;
            return db.Feedbacks.Where(m=>m.departUserId==id && m.status==Constants.RESOLVED && m.type.name==Constants.Complaints).OrderByDescending(m => m.resolvedDate).ToList();
        }

        public IPagedList<Feedback> getAllOpenByDept(string v, int pageIndex, int pageSize)
        {
            
            ApplicationUser user = db.Users.Find(v);
            var param1 = new SqlParameter();
            param1.ParameterName = "@depID";
            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = user.DepartmentId;

            var param2 = new SqlParameter();
            param2.ParameterName = "@status";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = Constants.OPEN;

          /*  var param3 = new SqlParameter();
            param3.ParameterName = "@CommentedByID";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = v;*/

            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("getAllWithDepartStatus @depID,@status", param1, param2).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }
            return feedlist.OrderByDescending(m=>m.createDate).ToPagedList(pageIndex, pageSize);
        }

        public ApplicationUser getEscalationUser(int? departmentId, int? categoryId)
        {
            var query = from e in db.EscalationUsers
                        join Category in db.Categories on e.Id equals Category.EscalationUserId
                        where e.DepartmentId == departmentId && Category.Id == categoryId
                        select e;
                  EscalationUser user=(EscalationUser)query.FirstOrDefault();
                  return db.Users.Find(user.firstEscalationUserId);
        }

        public IEnumerable<Medium> getSourceList()
        {
            return db.medium.OrderByDescending(m=>m.name).ToList();
        }

        public ApplicationUser getOperationsEscalationUser(int? costCenterId)
        {
            EscalationUser escUser = db.EscalationUsers.Where(m => m.CostCenterId == costCenterId).FirstOrDefault();
            ApplicationUser user = db.Users.Find(escUser.firstEscalationUserId);
            return user;
        }

        public IEnumerable<Feedback> chartsFeedbackRegion(string d1, string d2, string region)
        {
            var param1 = new SqlParameter();
            param1.ParameterName = "@D1";

            param1.SqlDbType = SqlDbType.VarChar;
            param1.SqlValue = d1;

            var param2 = new SqlParameter();
            param2.ParameterName = "@D2";
            param2.SqlDbType = SqlDbType.VarChar;
            param2.SqlValue = d2;


            var param3 = new SqlParameter();
            param3.ParameterName = "@Region";
            param3.SqlDbType = SqlDbType.VarChar;
            param3.SqlValue = region;



            List<Feedback> feedlist = new List<Feedback>();
            var result = db.Feedbacks.SqlQuery("chartsFeedbackRegion @D1,@D2,@Region", param1, param2, param3).ToList();
            foreach (var r in result)
            {
                feedlist.Add(r);
            }

            return feedlist.ToList();
        }

        public List<Comments> getDeptCOmments(string id)
        {
            string deptComment = Constants.commentType[1];
            string hrComment = Constants.commentType[0];
            return db.comments.Where(m => m.feedbackId == id &&  (m.commentFor== deptComment || m.commentFor== hrComment)).ToList();
        }

        public bool findByCostCentr(int? costCentrId)
        {

            if (db.Feedbacks.Where(m => m.user.CostCenterId == costCentrId).ToList().Count() > 0)
                return true;
            else
                return false;
        }

        public bool getTicketsOnCategory(int id)
        {
            EscalationUser escUser = db.EscalationUsers.Find(id);
           
            var categories =db.Categories.Where(c => c.EscalationUserId == (db.EscalationUsers.FirstOrDefault(e => e.firstEscalationUserId == escUser.firstEscalationUserId)).Id).Select(c=>c.Id).ToList(); ;
            int tickets=db.Feedbacks.Where(f=>f.categoryId!=null).Where(f => categories.Contains(f.categoryId.Value)).ToList().Count();
            if (tickets > 0)           
                return true;
            else
         return false;
            }

        public IPagedList<Feedback> getOpenAssignedToDpt(string id, int pageIndex, int pageSize)
        {
            //select* from Feedbacks f where f.userId in(select  Id from AspNetUsers where CostCenterId in (select CostCenterId from EscalationUsers where
            // firstEscalationUserId = 'a5f947e5-5876-4eee-9c39-55eac39c8795')) and f.typeId = 1 and f.departmentID = 1 and checkStatus = 'Assigned'

            int type_Id = db.FeedbackTypes.Where(t => t.name == Constants.Complaints).Select(m=>m.Id).FirstOrDefault();
            int? dept_Id = db.Users.Find(id).DepartmentId;
           
           
            List<int?> costcentr_ids = (db.EscalationUsers.Where(m =>m.firstEscalationUserId == id)).Select(m => m.CostCenterId).ToList();
            List<int> esc_user_ids = (db.EscalationUsers.Where(m => m.firstEscalationUserId == id)).Select(m => m.Id).ToList();
            List<int> category_ids= (db.Categories.Where(m => m.EscalationUserId != null).Where(m=> esc_user_ids.Contains(m.EscalationUserId.Value))).Select(m => m.Id).ToList();


            //List<string> userIds = db.Users.Where(m => ids.Contains(m.CostCenterId)).Select(m => m.Id).ToList();

            return db.Feedbacks.OrderByDescending(f=>f.createDate)
                            .Where(f => (costcentr_ids.Contains(f.user.CostCenterId) || category_ids.Contains(f.categoryId.Value)) && f.typeId == type_Id && f.departmentID == dept_Id && f.checkStatus == Constants.ASSIGNED)
                            .ToPagedList(pageIndex,pageSize);
       }

        public IPagedList<Feedback> getRespondedTicketsbyDpt(string id, int pageIndex, int pageSize)
        {
           
             int type_Id = db.FeedbackTypes.Where(t => t.name == Constants.Complaints).Select(m => m.Id).FirstOrDefault();
            int? dept_Id = db.Users.Find(id).DepartmentId;


            List<int?> costctr_ids = (db.EscalationUsers.Where(m => m.firstEscalationUserId == id)).Select(m => m.CostCenterId).ToList();
            List<int> esc_user_ids = (db.EscalationUsers.Where(m => m.firstEscalationUserId == id)).Select(m => m.Id).ToList();
            List<int> category_ids = (db.Categories.Where(m => m.EscalationUserId != null).Where(m => esc_user_ids.Contains(m.EscalationUserId.Value))).Select(m => m.Id).ToList();

            return db.Feedbacks.OrderByDescending(f => f.createDate)
                             .Where(f => (costctr_ids.Contains(f.user.CostCenterId) || category_ids.Contains(f.categoryId.Value)) && f.typeId == type_Id && f.departmentID == dept_Id && f.checkStatus!= Constants.ASSIGNED)
                             .ToPagedList(pageIndex, pageSize);

        }
    }
}