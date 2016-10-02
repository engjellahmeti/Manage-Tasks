using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Manage_Tasks.Models;
using System.IO;
using System.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Hosting;

namespace Manage_Tasks.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class RoleController : Controller
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

        // GET: Role
        public ActionResult Index()
        {
            string[] names = { "Candidate", "SuperAdmin" };
            List<AspNetRoles> rolet = new List<AspNetRoles>();
            var roles = db.AspNetRoles.ToList();
            foreach (var r in roles)
            {
                if (r.Name == "Candidate" || r.Name == "SuperAdmin")
                {
                    continue;
                }
                else
                {
                    rolet.Add(r);
                }
            }

            return View(rolet);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Groups = new MultiSelectList(db.Group.ToList(), "ID", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection collection)
        {
            try
            {

                //var groupsSelected = Request["itemsselect"].Split(',').ToList();
                var values = Request["listItems"].Split(',').ToList();
                if (values.Count() == 1)
                {
                    int idGroup = 0;
                    foreach (var v in values)
                    {
                        idGroup = int.Parse(v.ToString());
                    }
                    var rolename = collection["RoleName"];
                    if (!db.AspNetRoles.Any(x => x.Name == rolename))
                    {
                        context.Roles.Add(new Microsoft.AspNet.Identity.EntityFramework.IdentityRole()
                        {
                            Name = rolename
                        });
                        context.SaveChanges();
                    }
                    string roleid = db.AspNetRoles.FirstOrDefault(x => x.Name == rolename).Id;
                    //string roleid = db.AspNetRoles.Max(item => item.Id);

                    foreach (var group in values)
                    {
                        RoleGroup rg = new RoleGroup()
                        {
                            RoleID = roleid,
                            GroupID = int.Parse(group.ToString())
                        };
                        db.RoleGroup.Add(rg);
                        db.SaveChanges();
                    }
                    
                    var getGroupid = db.UserProfile.Where(x => x.GroupID == idGroup);


                    foreach (var user in getGroupid)
                    {
                        UserManager.RemoveFromRole(user.UsID, "Candidate");
                        UserManager.AddToRole(user.UsID, rolename);
                        UserRole ur = new UserRole
                        {
                            UserID = user.UserID,
                            RoleID = roleid
                        };
                        db.UserRole.Add(ur);
                    }
                    db.SaveChanges();

                }
            }
            catch
            {
                return View("Error");
            }
            return RedirectToAction("Index", "Role");
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            try
            {
                var getGropusinRole = db.RoleGroup.Where(x => x.RoleID == id).Select(x => x.GroupID);
                if (getGropusinRole.Count() == 1)
                {
                    int GroupId = 0;
                    foreach(var group in getGropusinRole)
                    {
                        GroupId = int.Parse(group.ToString());
                    }
                    var getGroupEdit = db.Group.Where(x => x.ID == GroupId);
                    var getOtherGroups = db.Group.Where(x => x.ID != GroupId);
                    var getRoleName = db.AspNetRoles.FirstOrDefault(x => x.Id == id);
                   ViewBag.lGroup = new MultiSelectList(getOtherGroups.ToList(), "ID", "Name");
                    ViewBag.lGroupEdit = new MultiSelectList(getGroupEdit.ToList(), "ID", "Name");
                    ViewBag.name = getRoleName.Name;
                    ViewBag.id = getRoleName.Id;
                }
                else
                {
                    List<SelectTemp> listGroupsEdit = new List<SelectTemp>();
                    foreach (var gr in getGropusinRole)
                    {
                        var g = db.Group.FirstOrDefault(x => x.ID == gr).Name;
                        SelectTemp st = new SelectTemp()
                        {
                            ID = gr,
                            Name = g.ToString()
                        };
                        listGroupsEdit.Add(st);
                    }
                    var listGroups = (from temp in db.Group
                                      select new SelectTemp
                                      {
                                          ID = temp.ID,
                                          Name = temp.Name
                                      }).ToList();
                    var getRoleName = db.AspNetRoles.FirstOrDefault(x => x.Id == id);
                    List<SelectTemp> groupLeft = returnComparison(listGroups, listGroupsEdit);
                    ViewBag.lGroup = new MultiSelectList(groupLeft.ToList(), "ID", "Name");
                    ViewBag.lGroupEdit = new MultiSelectList(listGroupsEdit.ToList(), "ID", "Name");
                    ViewBag.name = getRoleName.Name;
                    ViewBag.id = getRoleName.Id;
                }
            }
            catch (DataException ex)
            {

            }


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection collection)
        {
            try
            {

                //var groupsSelected = Request["itemsselect"].Split(',').ToList();
                var values = Request["listItems"].Split(',').ToList();
                if (values.Count() == 1)
                {
                    int idGroup = 0;
                    foreach (var v in values)
                    {
                        idGroup = int.Parse(v.ToString());
                    }
                    var rolename = collection["RoleName"];
                    var id = collection["Id"];
                    var getOldRoleName = db.AspNetRoles.Find(id);
                    string OldRoleName = getOldRoleName.Name;
                    AspNetRoles anr = new AspNetRoles
                    {
                        Id = id,
                        Name = rolename
                    };
                    var findOldRole = db.AspNetRoles.Find(id);
                    db.Entry(findOldRole).CurrentValues.SetValues(anr);
                    db.Entry(findOldRole).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    var oldRG = db.RoleGroup.Where(x => x.RoleID == id);
                    foreach (var t in oldRG)
                    {
                        RoleGroup r = db.RoleGroup.Find(t.ID);
                        db.RoleGroup.Remove(r);
                    }
                    db.SaveChanges();
                    foreach (var group in values)
                    {
                        RoleGroup rg = new RoleGroup()
                        {
                            RoleID = id,
                            GroupID = int.Parse(group.ToString())
                        };
                        db.RoleGroup.Add(rg);
                        db.SaveChanges();
                    }
                    var OldUserRole = db.UserRole.Where(x => x.RoleID == id);
                    foreach(var userrole in OldUserRole)
                    {
                        var getUserProf = db.UserProfile.First(x => x.UserID == userrole.UserID);
                        UserManager.RemoveFromRole(getUserProf.UsID, OldRoleName);
                        UserRole ur = db.UserRole.Find(userrole.ID);
                        db.UserRole.Remove(ur);
                    }
                    db.SaveChanges();
                    var getGroupid = db.UserProfile.Where(x => x.GroupID == idGroup);

                    foreach (var user in getGroupid)
                    {
                        UserManager.AddToRole(user.UsID, rolename);
                        UserRole ur = new UserRole
                        {
                            UserID = user.UserID,
                            RoleID = id
                        };
                        db.UserRole.Add(ur);
                    }
                    db.SaveChanges();
                }
            }
            catch
            {
                return View("Error");
            }
            return RedirectToAction("Index", "Role");

        }

        public List<SelectTemp> returnComparison(List<SelectTemp> listGroup, List<SelectTemp> listGroupEdit)
        {
            List<SelectTemp> retList = new List<SelectTemp>();
            List<SelectTemp> recoveryList = listGroup;
            //if(listGroupEdit.Count == listGroup.Count)
            //{
            //    return retList;
            //}
            //else
            //{
            //    for (int i = 0; i < listGroupEdit.Count; i++)
            //    {
            //       if(listGroup.Contains(listGroupEdit[i]))
            //        {

            //        }
            //       else
            //        {
            //            retList.Add(listGroupEdit[i]);
            //        }
            //    }

            //    return retList;
            //}           
            //int id = 150;
            if (listGroupEdit.Count == 1)
            {
                for (int i = 0; i < listGroupEdit.Count; i++)
                {
                    foreach (var u in listGroup)
                    {
                        if (listGroupEdit[i].ID == u.ID)
                        {
                            continue;
                        }
                        else
                        {
                            retList.Add(u);
                        }
                    }
                }
                return retList;
            }
            else if (listGroup.Count == listGroupEdit.Count)
            {
                return retList;
            }
            else
            {
                List<int> id = new List<int>();
                for (int i = 0; i < recoveryList.Count; i++)
                {
                    foreach (var u in listGroupEdit)
                    {
                        if (recoveryList[i].ID != u.ID)
                        {

                            if (id.Contains(recoveryList[i].ID))
                            {
                                continue;
                            }
                            else
                            {
                                retList.Add(recoveryList[i]);
                                id.Add(recoveryList[i].ID);
                            }
                        }
                        else
                        {
                            //recoveryList.Remove(recoveryList[i]);
                            i++;
                        }
                    }
                }
                return retList;
            }
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            try
            {

                var getGropusinRole = db.RoleGroup.Where(x => x.RoleID == id).Select(x => x.GroupID);
                if (getGropusinRole.Count() == 1)
                {
                    int GroupId = 0;
                    foreach (var group in getGropusinRole)
                    {
                        GroupId = int.Parse(group.ToString());
                    }
                    var getGroupEdit = db.Group.Where(x => x.ID == GroupId);
                    var getOtherGroups = db.Group.Where(x => x.ID != GroupId);
                    var getRoleName = db.AspNetRoles.FirstOrDefault(x => x.Id == id);
                    ViewBag.lGroup = new MultiSelectList(getOtherGroups.ToList(), "ID", "Name");
                    ViewBag.lGroupEdit = new MultiSelectList(getGroupEdit.ToList(), "ID", "Name");
                    ViewBag.name = getRoleName.Name;
                    ViewBag.id = getRoleName.Id;
                }
                else
                {
                    List<SelectTemp> listGroupsEdit = new List<SelectTemp>();
                    foreach (var gr in getGropusinRole)
                    {
                        var g = db.Group.FirstOrDefault(x => x.ID == gr).Name;
                        SelectTemp st = new SelectTemp()
                        {
                            ID = gr,
                            Name = g.ToString()
                        };
                        listGroupsEdit.Add(st);
                    }
                    var listGroups = (from temp in db.Group
                                      select new SelectTemp
                                      {
                                          ID = temp.ID,
                                          Name = temp.Name
                                      }).ToList();
                    var getRoleName = db.AspNetRoles.FirstOrDefault(x => x.Id == id);
                    List<SelectTemp> groupLeft = returnComparison(listGroups, listGroupsEdit);
                    ViewBag.lGroup = new MultiSelectList(groupLeft.ToList(), "ID", "Name");
                    ViewBag.lGroupEdit = new MultiSelectList(listGroupsEdit.ToList(), "ID", "Name");
                    ViewBag.name = getRoleName.Name;
                    ViewBag.id = getRoleName.Id;
                }
            }
            catch (DataException e)
            {

            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection collection)
        {
            string id = collection["ID"];
            string rolename = db.AspNetRoles.Find(id).Name;
            var oldRG = db.RoleGroup.Where(x => x.RoleID == id);
            foreach (var t in oldRG)
            {
                RoleGroup r = db.RoleGroup.Find(t.ID);
                db.RoleGroup.Remove(r);
            }
            db.SaveChanges();
            var OldUserRole = db.UserRole.Where(x => x.RoleID == id);
            foreach (var userrole in OldUserRole)
            {
                var getUserProf = db.UserProfile.First(x => x.UserID == userrole.UserID);
                UserManager.RemoveFromRole(getUserProf.UsID, rolename);
                UserRole ur = db.UserRole.Find(userrole.ID);
                db.UserRole.Remove(ur);
            }
            db.SaveChanges();
            AspNetRoles anr = db.AspNetRoles.Find(id);
            db.AspNetRoles.Remove(anr);
            db.SaveChanges();
            return RedirectToAction("Index", "Role");

        }

        public static List<string> getRoleNameList(List<string> roleids)
        {
            dbManageTasks db1 = new dbManageTasks();
            List<string> namesCreated = new List<string>();
            foreach (var id in roleids)
            {
                string name = db1.AspNetRoles.FirstOrDefault(x => x.Id == id).Name;
                namesCreated.Add(name);
            }
            return namesCreated;
        }

    }

    public class SelectTemp
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}