using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Manage_Tasks.Models;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Hosting;
using System.IO;
using System.Collections.Generic;
using System.Data;

namespace Manage_Tasks.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        ApplicationDbContext context = new ApplicationDbContext();
        dbManageTasks db = new dbManageTasks();

        public AccountController()
        {


        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        #region Variables
        public static string EConfUser { get; set; }
        public static string connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public static string command = null;
        public static string parameterName = null;
        public static string methodName = null;
        string codeType = null;

        public static string OEmail { get; set; }
        public static string OFname { get; set; }
        public static string OLname { get; set; }
        #endregion

        #region Login
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                bool getUserAccepted = true;
                if (model.Username != null)
                { getUserAccepted = db.UserProfile.FirstOrDefault(x => x.Username == model.Username).AcceptedFromSuperAdmin; }
                else
                {
                    getUserAccepted = false;
                }

                var custEmailConf = EmailConfirmation(model.Username);
                var custUserName = FindUserName(model.Username);
                var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);
                if (getUserAccepted == true)
                {
                    var getRoleSuperAdmin = context.Roles.FirstOrDefault(x => x.Name == "SuperAdmin");
                    var getSuper = db.UserProfile.FirstOrDefault(x => x.Username == "engjellahmeti").UsID;
                    if (getRoleSuperAdmin != null)
                    {
                        UserManager.AddToRole(getSuper, "SuperAdmin");
                    }
                    else
                    {
                        context.Roles.Add(new Microsoft.AspNet.Identity.EntityFramework.IdentityRole()
                        {
                            Name = "SuperAdmin"
                        });
                        context.SaveChanges();
                        UserManager.AddToRole(getSuper, "SuperAdmin");

                    }
                    try
                {
                        if (getSuper != null)
                        {

                        }
                        else
                        {
                            var getUserGroup = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);
                            var getRole = db.RoleGroup.FirstOrDefault(x => x.GroupID == getUserGroup.GroupID);
                            if (getRole != null)
                            {
                                var getRoleName = db.AspNetRoles.Find(getRole.RoleID);
                                if (getRoleName != null)
                                {
                                    UserManager.RemoveFromRole(getUserGroup.UsID, "Candidate");
                                    UserManager.AddToRole(getUserGroup.UsID, getRoleName.Name);
                                    UserRole ur = new UserRole
                                    {
                                        UserID = getUserGroup.UserID,
                                        RoleID = getRole.ToString()
                                    };
                                    db.UserRole.Add(ur);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                catch (DataException ex)
                {
                    throw ex;
                }
                if (custEmailConf == false && custUserName != null && result.ToString() == "Success")
                {
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    EConfUser = model.Username;
                    return RedirectToAction("EmailConfirmationFailer", "Account");
                }
                else
                {
                    ViewBag.ReturnUrl = returnUrl;
                    if (ModelState.IsValid)
                    {

                        // This doesn't count login failures towards account lockout
                        // To enable password failures to trigger account lockout, change to shouldLockout: true
                        switch (result)
                        {
                            case SignInStatus.Success:
                                UpdateLastLoginDate(model.Username);
                                return RedirectToLocal(returnUrl);
                            case SignInStatus.LockedOut:
                                return View("Lockout");
                            case SignInStatus.RequiresVerification:
                                return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                            case SignInStatus.Failure:
                            default:
                                ModelState.AddModelError("", "Invalid login attempt.");
                                return View("Login");
                        }



                    }
                    return View("Login");
                }
                }
                else
                {
                    return RedirectToAction("AcceptUser", "Account");
                }
            }
            return View();
        }
        #endregion


        [HttpGet]
        [AllowAnonymous]
        public ActionResult AcceptUser()
        {
            return View();
        }
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.Grupet = new SelectList(db.Group, "ID", "Name");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var custEmail = FindEmail(model.Email);
                    var custUserName = FindUserName(model.Username);
                    var user = new ApplicationUser
                    {
                        UserName = model.Username,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        JoinDate = DateTime.Now,
                        EmailLinkDate = DateTime.Now,
                        LastLoginDate = DateTime.Now
                    };

                    if (custEmail == null && custUserName == null)
                    {
                        var result = await UserManager.CreateAsync(user, model.Password);
                        var groupID = Request["IdEGrupit"];
                        int grId = int.Parse(groupID.ToString());
                        var findIDUser = db.AspNetUsers.FirstOrDefault(x => x.Email == model.Email).Id.ToString();
                        UserProfile u = new UserProfile()
                        {
                            Name = model.FirstName,
                            Surname = model.LastName,
                            FullName = model.FirstName + " " + model.LastName,
                            Email = model.Email,
                            AcceptedFromSuperAdmin = false,
                            PhoneNumber = model.PhoneNumber,
                            LastModifiedOnDate = DateTime.Now,
                            CreatedOnDate = DateTime.Now,
                            Username = model.Username,
                            JobTitle = model.JobTitle,
                            MainResponsibility = model.MainResponsibility,
                            EducationCretification = model.EducationCertification,
                            LastModifiedByUserID = findIDUser,
                            UsID = findIDUser,
                            GroupID = grId
                        };
                        db.UserProfile.Add(u);
                        db.SaveChanges();



                        if (result.Succeeded)
                        {
                            var getRoleCandidate = context.Roles.FirstOrDefault(x => x.Name == "Candidate");
                            if (getRoleCandidate != null)
                            {
                                UserManager.AddToRole(user.Id, "Candidate");
                                // Send an email with this link
                                codeType = "EmailConfirmation";
                                await SendEmail("ConfirmEmail", "Account", user, model.Email, "WelcomeEmail", "Confirm your account");
                                return RedirectToAction("ConfirmationEmailSent", "Account");
                            }
                            else
                            {
                                context.Roles.Add(new Microsoft.AspNet.Identity.EntityFramework.IdentityRole()
                                {
                                    Name = "Candidate"
                                });
                                context.SaveChanges();
                                UserManager.AddToRole(user.Id, "Candidate");

                                // Send an email with this link
                                codeType = "EmailConfirmation";
                                await SendEmail("ConfirmEmail", "Account", user, model.Email, "WelcomeEmail", "Confirm your account");
                                return RedirectToAction("ConfirmationEmailSent", "Account");

                            }
                        }
                        AddErrors(result);
                    }
                    else
                    {
                        if (custEmail != null)
                        {
                            ModelState.AddModelError("", "Email is already registered.");
                        }
                        if (custUserName != null)
                        {
                            ModelState.AddModelError("", "Username " + model.Username.ToString() + " is already taken.");
                        }
                    }

                }
                else
                {
                    ViewBag.Grupet = new SelectList(db.Group, "ID", "Name");

                    // If we got this far, something failed, redisplay form
                    return View();
                }
                ViewBag.Grupet = new SelectList(db.Group, "ID", "Name");

                // If we got this far, something failed, redisplay form
                return View();
            }
            catch
            {
                return View("Error on registering you as a user");
            }

        }

        [HttpGet]
        public async Task<string> returnCode(string id)
        {
            string code = null;

            code = await UserManager.GeneratePasswordResetTokenAsync(id);

            return code;
        }

        public async Task SendEmail(string actionName, string controllerName, ApplicationUser user, string email, string emailTemplate, string emailSubject)
        {
            string code = null;
            if (codeType == "EmailConfirmation")
            {
                code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            }
            else if (codeType == "ResetPassword")
            {
                code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            }
            var callbackUrl = Url.Action(actionName, controllerName, new { userId = user.Id, date = DateTime.Now, code = code }, protocol: Request.Url.Scheme);
            var message = await EmailTemplate(emailTemplate, codeType);
            message = message.Replace("@ViewBag.Name", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.FirstName));
            message = message.Replace("@ViewBag.Link", callbackUrl);

            await MessageServices.SendEmailAsync(email, emailSubject, message);
        }

        public static async Task<string> EmailTemplate(string template, string codeType)
        {
            if (codeType == "EmailConfirmation")
            {
                // var templateFilePath = HostingEnvironment.MapPath("~/Content/templates/") + template + ".cshtml";
                var templateFilePath = HostingEnvironment.MapPath("~/Content/templates/WelcomeEmail.cshtml");
                StreamReader objStreamReaderFile = new StreamReader(templateFilePath);
                var body = await objStreamReaderFile.ReadToEndAsync();
                return body;

            }
            else if (codeType == "ResetPassword")
            {
                // var templateFilePath = HostingEnvironment.MapPath("~/Content/templates/") + template + ".cshtml";
                var templateFilePath = HostingEnvironment.MapPath("~/Content/templates/ForgotPasswordEmail.cshtml");
                StreamReader objStreamReaderFile = new StreamReader(templateFilePath);
                var body = await objStreamReaderFile.ReadToEndAsync();
                return body;

            }
            return null;
        }
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, DateTime date, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("ConfirmationLinkExpired", "Account");
            }
            var emailConf = EmailConfirmationById(userId);
            if (emailConf == true)
            {
                return RedirectToAction("ConfirmationLinkUsed", "Account");
            }
            if (date != null)
            {
                if (date.AddMinutes(5) < DateTime.Now)
                {
                    return RedirectToAction("ConfirmationLinkExpired", "Account");
                }
                else
                {
                    var result = await UserManager.ConfirmEmailAsync(userId, code);
                    return View(result.Succeeded ? "ConfirmEmail" : "Error");
                }
            }
            else
            {
                return View("Error");
            }

        }


        [HttpGet]
        public JsonResult GetUsernameAjax(string email)
        {
            try
            {
                string recover = email.Substring(email.LastIndexOf('@'));

                return Json(recover, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendConfirmationMail()
        {
            string res = null;
            using (SqlConnection myConnection = new SqlConnection(connection))
            using (SqlCommand cmd = new SqlCommand("SELECT Email AS Email FROM AspNetUsers WHERE UserName = @UserName", myConnection))
            {
                cmd.Parameters.AddWithValue("@UserName", EConfUser);
                myConnection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        // Read advances to the next row.
                        if (reader.Read())
                        {
                            // To avoid unexpected bugs access columns by name.
                            res = reader["Email"].ToString();
                            var user = await UserManager.FindByEmailAsync(res);
                            UpdateEmailLinkDate(EConfUser);
                            codeType = "EmailConfirmation";
                            await SendEmail("ConfirmEmail", "Account", user, res, "WelcomeEmail", "Confirm your account");
                        }
                        myConnection.Close();
                    }
                }
            }
            return RedirectToAction("ConfirmationEmailSent", "Account");
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult EmailConfirmationFailed()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmationEmailSent()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmationLinkExpired()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmationLinkUsed()
        {
            return View();
        }


        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                codeType = "ResetPassword";
                await SendEmail("ResetPassword", "Account", user, model.Email, "ForgotPasswordEmail", "Confirm your account");
                //string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                //await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {

            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var r = await UserManager.RemovePasswordAsync(user.Id);
            if (r.Succeeded)
            {
                r = await UserManager.AddPasswordAsync(user.Id, model.Password);
                if (r.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation", "Account");

                }
            }
            //var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            //if (result.Succeeded)
            //{
            //    return RedirectToAction("ResetPasswordConfirmation", "Account");
            //}
            AddErrors(r);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public bool ReturnBool(string str)
        {
            bool econfOut = false;
            string res = null;
            using (SqlConnection con = new SqlConnection(connection))
            using (SqlCommand cmd = new SqlCommand(command, con))
            {
                cmd.Parameters.AddWithValue(parameterName, str);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            res = reader["EmailConfirmed"].ToString();
                            if (res == "False")
                            {
                                econfOut = false;
                            }
                            else
                            {
                                econfOut = true;
                            }
                        }
                        con.Close();
                    }
                    return econfOut;
                }
            }
        }

        public bool EmailConfirmation(string username)
        {
            command = "SELECT EmailConfirmed AS EmailConfirmed FROM AspNetUsers WHERE UserName= @UserName";
            parameterName = "@UserName";
            return ReturnBool(username);
        }

        public bool EmailConfirmationById(string userid)
        {
            command = "SELECT EmailConfirmed AS EmailConfirmed FROM AspNetUsers WHERE Id = @Id";
            parameterName = "@Id";
            return ReturnBool(userid);
        }

        public static int UpdateDatabase(string username)
        {
            using (SqlConnection con = new SqlConnection(connection))
            using (SqlCommand cmd = new SqlCommand(command, con))
            {
                cmd.Parameters.AddWithValue(parameterName, username);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static int UpdateEmailLinkDate(string username)
        {
            command = "UPDATE AspNetUsers SET EmailLinkDate = '" + DateTime.Now + "'WHERE UserName = @UserName";
            parameterName = "@UserName";
            return UpdateDatabase(username);
        }

        public static int UpdateLastLoginDate(string username)
        {
            command = "UPDATE AspNetUsers SET LastLoginDate ='" + DateTime.Now + "'WHERE Username = @UserName";
            parameterName = "@UserName";
            return UpdateDatabase(username);
        }

        public static string ReturnString(string str)
        {
            string strOut = null;
            using (SqlConnection con = new SqlConnection(connection))
            using (SqlCommand cmd = new SqlCommand(command, con))
            {
                cmd.Parameters.AddWithValue(parameterName, str);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            if (methodName == "FindEmail")
                            {
                                strOut = reader["Email"].ToString();
                            }
                            else if (methodName == "FindUserName" || methodName == "FindUserNameById")
                            {
                                strOut = reader["UserName"].ToString();
                            }
                            else if (methodName == "FindUserById")
                            {
                                strOut = reader["UserId"].ToString();
                            }
                        }
                        con.Close();
                    }
                    return strOut;
                }
            }
        }

        public static string FindEmail(string email)
        {
            command = "SELECT Email AS Email FROM AspNetUsers WHERE Email = @Email";
            parameterName = "@Email";
            methodName = "FindEmaik";

            return ReturnString(email);
        }

        public static string FindUserName(string username)
        {
            command = "SELECT UserName as UserName From AspNetUsers WHERE UserName = @UserName";
            parameterName = "@UserName";
            methodName = "FindUserName";

            return ReturnString(username);
        }

        public string FindUserNameById(string userid)
        {
            command = "SELECT UserName AS UserName FROM AspNetUsers WHERE Id = @Id";
            parameterName = "@Id";
            methodName = "FindUserNameById";
            return ReturnString(userid);
        }
        public string FindUserId(string userprokey)
        {
            command = "SELECT UserId AS UserId FROM AspNetUserLogins WHERE ProviderKey = @ProviderKey";
            parameterName = "@ProviderKey";
            methodName = "FindUserId";
            return ReturnString(userprokey);
        }

        public static string GetConnectionString(string Connection)
        {
            return ConfigurationManager.ConnectionStrings[Connection].ConnectionString;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }

        }

        #endregion
    }
}