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
    [Authorize(Roles ="SuperAdmin")]
    public class AcceptUsersController : Controller
    {
        private dbManageTasks db = new dbManageTasks();

        // GET: AcceptUsers
        public ActionResult Index()
        {
           return View(db.UserProfile.Where(x => x.AcceptedFromSuperAdmin == false).ToList());
        }

     

        // GET: AcceptUsers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userProfile = db.UserProfile.Find(id);
            if (userProfile == null)
            {
                return HttpNotFound();
            }
            //ViewBag.UsID = new SelectList(db.AspNetUsers, "Id", "FirstName", userProfile.UsID);
            ViewBag.GroupID = new SelectList(db.Group, "ID", "Name", userProfile.GroupID);
            return View(userProfile);
        }

        // POST: AcceptUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,Name,Surname, Email,PhoneNumber,Username,JobTitle,MainResponsibility,EducationCretification, AcceptedFromSuperAdmin,GroupID,UsID")] UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                var getSuper = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);

                UserProfile up1 = db.UserProfile.Find(userProfile.UserID);
                UserProfile up2 = db.UserProfile.Find(userProfile.UserID);
                up2.Name = userProfile.Name;
                up2.Surname = userProfile.Surname;
                up2.FullName = userProfile.Name + " " + userProfile.Surname;
                up2.Email = userProfile.Email;
                up2.PhoneNumber = userProfile.PhoneNumber;
                up2.Username = userProfile.Username;
                up2.JobTitle = userProfile.JobTitle;
                up2.MainResponsibility = userProfile.MainResponsibility;
                up2.EducationCretification = userProfile.EducationCretification;
                up2.AcceptedFromSuperAdmin = userProfile.AcceptedFromSuperAdmin;
                up2.LastModifiedOnDate = DateTime.Now;
                up2.LastModifiedByUserID = getSuper.UsID;
                up2.GroupID = userProfile.GroupID;
                up2.UsID = userProfile.UsID;
                db.Entry(up1).CurrentValues.SetValues(up2);
                db.Entry(up1).State = EntityState.Modified;

                AspNetUsers asp1 = db.AspNetUsers.Find(userProfile.UsID);
                AspNetUsers asp2 = db.AspNetUsers.Find(userProfile.UsID);
                asp2.FirstName = userProfile.Name;
                asp2.LastName = userProfile.Surname;
                asp2.Email = userProfile.Email;
                asp2.UserName = userProfile.Username;

                db.Entry(asp1).CurrentValues.SetValues(asp2);
                db.Entry(asp1).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UsID = new SelectList(db.AspNetUsers, "Id", "FirstName", userProfile.UsID);
            ViewBag.GroupID = new SelectList(db.Group, "ID", "Name", userProfile.GroupID);
            return View(userProfile);
        }

        // GET: AcceptUsers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userProfile = db.UserProfile.Find(id);
            if (userProfile == null)
            {
                return HttpNotFound();
            }
            return View(userProfile);
        }

        // POST: AcceptUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserProfile userProfile = db.UserProfile.Find(id);

            AspNetUsers asp = db.AspNetUsers.Find(userProfile.UsID);

            db.UserProfile.Remove(userProfile);
            db.SaveChanges();
            db.AspNetUsers.Remove(asp);
            
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

        [HttpGet]
        public ActionResult UserIsAccepted(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userProfile = db.UserProfile.Find(id);
            if (userProfile == null)
            {
                return HttpNotFound();
            }
            return View(userProfile);
        }

        [HttpPost]
        public ActionResult UserIsAccepted(int id)
        {

            try
            {
                UserProfile up1 = db.UserProfile.Find(id);
                UserProfile up2 = db.UserProfile.Find(id);
                up2.AcceptedFromSuperAdmin = true;

                db.Entry(up1).CurrentValues.SetValues(up2);
                db.Entry(up1).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "AcceptUsers");
            }
            catch(DataException ex)
            {
                return View(ex);
               // throw ex;
            }
        }
    }
}
