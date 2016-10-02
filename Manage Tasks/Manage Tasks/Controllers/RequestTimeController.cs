using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Manage_Tasks.Models;

namespace Manage_Tasks.Controllers
{
    [Authorize]
    public class RequestTimeController : Controller
    {
        private dbManageTasks db = new dbManageTasks();

        // GET: RequestTime
        public ActionResult Index()
        {
            var getName = User.Identity.Name;
            var getId = db.AspNetUsers.FirstOrDefault(x => x.UserName == getName);
            var requestTime = db.RequestTime.Where(x => x.TaskConfig.Pending == true && x.TaskConfig.CreatedByUserId == getId.Id && x.IsAccepted == false).Include(r => r.TaskConfig);
            return View(requestTime.ToList());
        }

        // GET: RequestTime/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestTime requestTime = db.RequestTime.Find(id);
            if (requestTime == null)
            {
                return HttpNotFound();
            }
            return View(requestTime);
        }

        //// GET: RequestTime/Create
        //public ActionResult Create()
        //{
        //    ViewBag.TaskID = new SelectList(db.TaskConfig, "TaskID", "TaskName");
        //    return View();
        //}

        //// POST: RequestTime/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "ID,PersonWhoRequested,DateRequired,DateRequested,IsAccepted,TaskID")] RequestTime requestTime)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.RequestTime.Add(requestTime);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.TaskID = new SelectList(db.TaskConfig, "TaskID", "TaskName", requestTime.TaskID);
        //    return View(requestTime);
        //}

        // GET: RequestTime/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestTime requestTime = db.RequestTime.Find(id);
            if (requestTime == null)
            {
                return HttpNotFound();
            }
            ViewBag.TaskID = new SelectList(db.TaskConfig, "TaskID", "TaskName", requestTime.TaskID);
            return View(requestTime);
        }

        // POST: RequestTime/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,PersonWhoRequested,DateRequired,DateRequested,IsAccepted,TaskID")] RequestTime requestTime)
        {
            if (ModelState.IsValid)
            {
                var accepted = Request["checkedAcc"].ToString();
                RequestTime r1 = db.RequestTime.Find(requestTime.ID);
                RequestTime r2 = db.RequestTime.Find(requestTime.ID);
                r2.DateRequested = requestTime.DateRequested;
                r2.DateRequired = requestTime.DateRequired;
                r2.IsAccepted = Convert.ToBoolean(accepted);
                r2.TaskID = requestTime.TaskID;
                db.Entry(r1).CurrentValues.SetValues(r2);
                db.Entry(r2).State = EntityState.Modified;
                db.SaveChanges();
                if (Convert.ToBoolean(accepted) == true)
                {
                    TaskConfig t1 = db.TaskConfig.Find(requestTime.TaskID);
                    TaskConfig t2 = db.TaskConfig.Find(requestTime.TaskID);
                    t2.IsRequestAccepted = true;
                    t2.Pending = false;
                    t2.TaskEndDate = requestTime.DateRequested;
                    db.Entry(t1).CurrentValues.SetValues(t2);
                    db.Entry(t1).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            //ViewBag.TaskID = new SelectList(db.TaskConfig, "TaskID", "TaskName", requestTime.TaskID);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult AcceptRequest(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestTime requestTime = db.RequestTime.Find(id);
            if (requestTime == null)
            {
                return HttpNotFound();
            }
            return View(requestTime);
        }

        [HttpPost]
        public ActionResult AcceptRequest(int id)
        {

            RequestTime requestTime = db.RequestTime.Find(id);
            RequestTime requestTime1 = db.RequestTime.Find(id);
            requestTime1.IsAccepted = true;
            db.Entry(requestTime).CurrentValues.SetValues(requestTime1);
            db.Entry(requestTime).State = EntityState.Modified;
            db.SaveChanges();
            TaskConfig t1 = db.TaskConfig.Find(requestTime.TaskID);
            TaskConfig t2 = db.TaskConfig.Find(requestTime.TaskID);
            t2.IsRequestAccepted = true;
            t2.Pending = false;
            t2.TaskEndDate = requestTime.DateRequested;
            db.Entry(t1).CurrentValues.SetValues(t2);
            db.Entry(t1).State = EntityState.Modified;
            db.SaveChanges();
            //db.RequestTime.Remove(requestTime);
            //db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult RejectRequest(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestTime requestTime = db.RequestTime.Find(id);
            if (requestTime == null)
            {
                return HttpNotFound();
            }
            return View(requestTime);
        }

        [HttpPost]
        public ActionResult RejectRequest(int id)
        {

            RequestTime requestTime = db.RequestTime.Find(id);
            RequestTime requestTime1 = db.RequestTime.Find(id);
            requestTime1.IsAccepted = false;
            db.Entry(requestTime).CurrentValues.SetValues(requestTime1);
            db.Entry(requestTime).State = EntityState.Modified;
            db.SaveChanges();
            TaskConfig t1 = db.TaskConfig.Find(requestTime.TaskID);
            TaskConfig t2 = db.TaskConfig.Find(requestTime.TaskID);
            t2.IsRequestAccepted = false;
            t2.Pending = false;
            db.Entry(t1).CurrentValues.SetValues(t2);
            db.Entry(t1).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        // GET: RequestTime/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestTime requestTime = db.RequestTime.Find(id);
            if (requestTime == null)
            {
                return HttpNotFound();
            }
            return View(requestTime);
        }

        // POST: RequestTime/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RequestTime requestTime = db.RequestTime.Find(id);
            db.RequestTime.Remove(requestTime);
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
