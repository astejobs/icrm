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
        Feedback Find(int? id);
        IPagedList<Feedback> Pagination(int pageIndex, int pageSize,string userId);
        IPagedList<Feedback> getAllAssigned(int pageIndex, int pageSize);
        IPagedList<Feedback> search(DateTime d1,DateTime d2,int pageIndex, int pageSize);
        IEnumerable<Feedback> getAll();
        IPagedList<Feedback> getAllOpenWithDepartment(string id, int pageIndex, int pageSize);
        IPagedList<Feedback> OpenWithoutDepart(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllResolved(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllResponded(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllClosed(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllRespondedWithDepartment(string id, int pageIndex, int pageSize);
    }
}
