using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Manage_Tasks.Models;
using Microsoft.AspNet.Identity.Owin;

namespace Manage_Tasks.Controllers
{
    [Authorize(Roles ="SuperAdmin")]
    public class DeleteUsersController : Controller
    {
        private dbManageTasks db = new dbManageTasks();
        private ApplicationUserManager _userManager;
        ApplicationDbContext context = new ApplicationDbContext();

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: DeleteUsers
        public ActionResult Index()
        {
            var userProfile = db.UserProfile.Include(u => u.AspNetUsers).Include(u => u.Group);
            return View(userProfile.ToList());
        }

      
        // GET: DeleteUsers/Edit/5
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
           
            ViewBag.GroupID = new SelectList(db.Group, "ID", "Name", userProfile.GroupID);
            return View(userProfile);
        }

        // POST: DeleteUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,Name, Surname,Email,PhoneNumber,Username,JobTitle,MainResponsibility,EducationCretification, GroupID")] UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var getSuper = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);
                    UserProfile up1 = db.UserProfile.Find(userProfile.UserID);
                    UserProfile up2 = db.UserProfile.Find(userProfile.UserID);
                    up2.Name = userProfile.Name;
                    up2.Surname = userProfile.Surname;
                    up2.FullName = userProfile.Name + " " + userProfile.Surname;

                    up2.PhoneNumber = userProfile.PhoneNumber;
                    up2.Username = userProfile.Username;
                    up2.JobTitle = userProfile.JobTitle;
                    up2.MainResponsibility = userProfile.MainResponsibility;
                    up2.EducationCretification = userProfile.EducationCretification;
                    up2.LastModifiedOnDate = DateTime.Now;
                    up2.LastModifiedByUserID = getSuper.UsID;
                    up2.GroupID = userProfile.GroupID;
                    db.Entry(up1).CurrentValues.SetValues(up2);
                    db.Entry(up1).State = EntityState.Modified;
                    db.SaveChanges();

                    AspNetUsers asp1 = db.AspNetUsers.Find(up1.UsID);
                    AspNetUsers asp2 = db.AspNetUsers.Find(up1.UsID);
                    asp2.FirstName = userProfile.Name;
                    asp2.LastName = userProfile.Surname;
                    asp2.UserName = userProfile.Username;
                    db.Entry(asp1).CurrentValues.SetValues(asp2);
                    db.Entry(asp1).State = EntityState.Modified;
                    db.SaveChanges();
                }catch(DataException ex)
                {
                    throw ex;
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        // GET: DeleteUsers/Delete/5
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

        // POST: DeleteUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserProfile userProfile = db.UserProfile.Find(id);
            var getUserRole = db.UserRole.Where(x => x.UserID == id).Select(x => x.ID);
            var getUserRoleForAspnet = db.UserRole.Where(x => x.UserID == id).Select(x => x.RoleID);
            List<AspNetRoles> aspnetrole = new List<AspNetRoles>();
            foreach(var roleid in getUserRoleForAspnet)
            {
                AspNetRoles a = db.AspNetRoles.Find(roleid.ToString());
                aspnetrole.Add(a);
            }
            foreach(var rolename in aspnetrole)
            {
                UserManager.RemoveFromRoleAsync(userProfile.UsID, rolename.ToString());
            }
            foreach (var user in getUserRole)
            {
                UserRole u = db.UserRole.Find(int.Parse(user.ToString()));
                db.UserRole.Remove(u);
            }
            db.SaveChanges();
            var getTaskUser = db.TaskCreatorUser.Where(x => x.UserID == id).Select(x => x.ID);
            foreach (var user in getTaskUser)
            {
                TaskCreatorUser t = db.TaskCreatorUser.Find(int.Parse(user.ToString()));
                db.TaskCreatorUser.Remove(t);
            }
            db.SaveChanges();
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
    }
}
