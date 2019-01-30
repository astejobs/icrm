﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using icrm.Models;
using PagedList.Mvc;
using PagedList;

namespace icrm.RepositoryInterface
{
    interface IFeedback : IDisposable
    {
        void Save(Feedback feedback);
        Feedback Find(string id);
        IPagedList<Feedback> Pagination(int pageIndex, int pageSize,string userId);
        IPagedList<Feedback> getAllAssigned(int pageIndex, int pageSize);
        IEnumerable<Feedback> getAllAssigned();
        IEnumerable<Feedback> GetAllAssigned();
        IPagedList<Feedback> search(string d1,string d2,string status,int id,int pageIndex, int pageSize);
        IEnumerable<Feedback> getAll();
        IPagedList<Feedback> getAllOpenWithDepartment(string id, int pageIndex, int pageSize);
        IPagedList<Feedback> OpenWithoutDepart(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllResolved(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllResponded(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllClosed(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllRespondedWithDepartment(string id, int pageIndex, int pageSize);
        IEnumerable<Feedback> getAllRespondedWithDepartment(string id);
        IEnumerable<Feedback> getAllOpen();
        IEnumerable<Feedback> getAllClosed();
        IEnumerable<Feedback> getAllResolved();
        IEnumerable<Feedback> getAllResponded();
        IEnumerable<Feedback> GetAllResponded();
        IEnumerable<Feedback> OpenWithoutDepart();
        IEnumerable<Feedback> searchlist(DateTime d1, DateTime d2);
        IEnumerable<Feedback> searchHR(string d1, string d2, string status);
        IPagedList<Feedback> searchHR(string v1, string v2, string status, int pageIndex, int pageSize);
        IPagedList<Feedback> getAllRejected(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllOpen(int pageIndex, int pageSize);


        List<Comments> getCOmments(string id);

        List<Category> getCategories(Int32 deptId);
        List<SubCategory> getSubCategories(Int32 categoryId);

        List<string> getEmails();
        IPagedList<Feedback> getListBasedOnType(int pageIndex, int pageSize, int type);
       
    }
}
