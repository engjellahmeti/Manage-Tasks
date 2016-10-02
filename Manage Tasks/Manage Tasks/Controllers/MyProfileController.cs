using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data;
using Microsoft.AspNet.Identity.Owin;
using System.Net;
using System.Data.Entity;
using Manage_Tasks.Models;
using Manage_Tasks;

namespace Manage_Tasks.Controllers
{
    [Authorize]
    public class MyProfileController : Controller
    {
        dbManageTasks db = new dbManageTasks();
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

        // GET: MyProfile

        [HttpGet]
        public ActionResult Index(string id)
        {
            //var ID = User.Identity.GetUserId();
            var userResult = db.UserProfile.FirstOrDefault(x => x.UsID == id);
            //var userResult = db.User.Find(id);

            return View(userResult);
        }



        [HttpPost]
        public JsonResult usersProfile(string Prefix)
        {
            var getUsers = (from n in db.UserProfile
                            where n.FullName.Contains(Prefix) && n.FullName != "Engjell Ahmeti"
                            select new { n.FullName, n.UserID });

            return Json(getUsers, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult showUsersSearched()
        {
            try
            {
                var getNames = Request["NamesOfUsers"];
                var userProfilesEnd = db.UserProfile.Where(x => x.UserID == 0);
                if (getNames != null && getNames != "")
                {
                    var userProfiles = db.UserProfile.Where(p => p.FullName.Contains(getNames));
                    if (userProfiles.Count() == 1)
                    {
                        string id = db.UserProfile.FirstOrDefault(x => x.FullName.Contains(getNames)).UsID.ToString();
                        return RedirectToAction("Index", "MyProfile", new { id = id });
                    }
                    else if (userProfiles.Count() > 1)
                    {
                        return View(userProfiles.ToList());
                    }
                }
                return View(userProfilesEnd.ToList());
            }
            catch (DataException ex)
            {
                throw ex;
            }
        }

        public async System.Threading.Tasks.Task<string> getCode(string id)
        {
            string code = null;

            code = await UserManager.GeneratePasswordResetTokenAsync(id);

            return code;
        }


        [HttpGet]
        public async System.Threading.Tasks.Task<ActionResult> profileGiven(string id)
        {
            string code = null;

            code = await UserManager.GeneratePasswordResetTokenAsync(id);


            //var ID = User.Identity.GetUserId();
            ViewBag.GetPasswordCode = HttpUtility.UrlEncode(code);
            var userResult = db.UserProfile.FirstOrDefault(x => x.UsID == id);
            //var userResult = db.User.Find(id);

            return View(userResult);
        }

        [HttpGet]
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

            //ViewBag.GroupID = new SelectList(db.Group, "ID", "Name", userProfile.GroupID);
            return View(userProfile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,Name, Surname,Email,PhoneNumber,Username,JobTitle,MainResponsibility,EducationCretification")] UserProfile userProfile)
        {
            string id = "";
            string username = "";
            if (ModelState.IsValid)
            {
                try
                {

                    UserProfile up1 = db.UserProfile.Find(userProfile.UserID);
                    id = up1.UsID;
                    username = up1.Username;
                    UserProfile up2 = db.UserProfile.Find(userProfile.UserID);
                    up2.Name = userProfile.Name;
                    up2.Surname = userProfile.Surname;
                    up2.FullName = userProfile.Name + " " + userProfile.Surname;
                    up2.PhoneNumber = userProfile.PhoneNumber;
                    up2.JobTitle = userProfile.JobTitle;
                    up2.MainResponsibility = userProfile.MainResponsibility;
                    up2.EducationCretification = userProfile.EducationCretification;
                    up2.LastModifiedOnDate = DateTime.Now;
                    up2.LastModifiedByUserID = up1.UsID;
                    db.Entry(up1).CurrentValues.SetValues(up2);
                    db.Entry(up1).State = EntityState.Modified;
                    db.SaveChanges();

                    AspNetUsers asp1 = db.AspNetUsers.Find(up1.UsID);
                    AspNetUsers asp2 = db.AspNetUsers.Find(up1.UsID);
                    asp2.FirstName = userProfile.Name;
                    asp2.LastName = userProfile.Surname;
                    db.Entry(asp1).CurrentValues.SetValues(asp2);
                    db.Entry(asp1).State = EntityState.Modified;
                    db.SaveChanges();


                    return RedirectToAction("profileGiven", new { id = id });


                }
                catch (DataException ex)
                {
                    throw ex;
                }
            }

            return RedirectToAction("profileGiven", new { id = id });

        }
    }
}