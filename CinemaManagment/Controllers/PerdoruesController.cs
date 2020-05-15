using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using System.Diagnostics;
using System.Data.Entity.Validation;
using CinemaManagment.Models;
using NLog;
using System.Web.Helpers;

namespace CinemaManagment.Controllers
{
    public class PerdoruesController : Controller
    {
        private KinemaDBEntities db = new KinemaDBEntities();
        Logger logger = LogManager.GetCurrentClassLogger();
      
        // Registration action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] Perdorue P)
        {
            bool Status = false;
            string Message = "";
            if (ModelState.IsValid)
            {
                #region //is email valid
                var IsExists = IsEmailVerified(P.Email);
                if (IsExists)
                {
                    ModelState.AddModelError("EmailExists", "Ky email ekziston");
                    return View(P);

                }
                #endregion
                #region generate activationcode
                P.ActivationCode = Guid.NewGuid();
                #endregion
                #region password hashing
                P.Fjalekalimi = Crypto.Hash(P.Fjalekalimi);
                P.IsEmailVerified = true;
                P.ID_Roli = 1;
                P.Statusi = "Activ";
                #endregion
                #region save data
                try
                {

                    db.Perdorues.Add(P);
                    db.SaveChanges();

                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}",
                                                    validationError.PropertyName,
                                                    validationError.ErrorMessage);
                        }
                    }
                    logger.ErrorException("Error u gjend ne Controller PErdorues, Registration Action", dbEx);
                }
                #endregion
            }
            else
            {
                Message = "Invalid request";
            }
            ViewBag.Message = Message;
            ViewBag.Status = Status;
            return View(P);
        }
        //Verify account
       [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            try
            {
                bool status = false;
                db.Configuration.ValidateOnSaveEnabled = false;
                var v = db.Perdorues.Where(x => x.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    db.SaveChanges();
                    status = true;

                }
                else
                {
                    ViewBag.Message = "Invalid request";
                }
                ViewBag.Status = status;
                return View();
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Kategori Controller, Edit Action", ex);
            }
            return View();
        }
        //login
        [HttpGet]
        public ActionResult LogIn()
        {
            return View();
        }
        //loginpost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogIn(UserLogin login, string ReturnUrl = "")
        {
            try
            {
                if (Session["perdorues"] != null)
                {
                    return RedirectToAction("Index", "Kategoris");
                }

                else
                {

                    string message = "";
                    var v = db.Perdorues.Where(x => x.Email == login.Email).FirstOrDefault();
                    if (v != null)
                    {
                        if (string.Compare(Crypto.Hash(login.Fjalekalimi), v.Fjalekalimi) == 0)
                        {
                            int timeout = login.RememberMe ? 52600 : 1;
                            var ticket = new FormsAuthenticationTicket(login.Email, login.RememberMe, timeout);
                            string encrypted = FormsAuthentication.Encrypt(ticket);
                            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                            cookie.Expires = DateTime.Now.AddMinutes(timeout);
                            cookie.HttpOnly = true;
                            Response.Cookies.Add(cookie);
                            if (Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);

                            }
                            else
                            {
                                int roli = Convert.ToInt32(db.Perdorues.Where(x => x.Email == login.Email).Select(x => x.ID_Roli).FirstOrDefault());
                                string statusi = db.Perdorues.Where(x => x.Email == login.Email).Select(x => x.Statusi).FirstOrDefault();
                                if (statusi == "Pasiv")
                                {
                                    TempData["mssg"] = "<script>alert('Ju keni kaluar ne statusin pasiv nuk mund te logoheni');</script>";
                                    return RedirectToAction("Index", "Kategoris");

                                }
                                else
                                {
                                    if (roli == 1)
                                    {
                                        Session["perdorues"] = db.Perdorues.Where(x => x.Email == login.Email).Select(x => x.ID_Perdorues).FirstOrDefault();

                                        return RedirectToAction("Index", "Kategoris");
                                    }
                                    //shkon tek faqja e administratorit
                                    else if (roli == 2)
                                    {
                                        Session["administrator"] = db.Perdorues.Where(x => x.Email == login.Email).Select(x => x.ID_Perdorues).FirstOrDefault();

                                        return RedirectToAction("Index", "Admin");
                                    }
                                    if (roli == 3)
                                    {
                                        Session["menaxher"] = db.Perdorues.Where(x => x.Email == login.Email).Select(x => x.ID_Perdorues).FirstOrDefault();

                                        return RedirectToAction("Index", "Menaxher");
                                    }
                                }
                            }
                        }
                        else
                        {
                            message = "invalid credentials";

                        }
                    }
                    else
                    {
                        message = "invalid credentials";
                    }
                    ViewBag.Message = message;

                    return View(login);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Kategori Controller, Login Action", ex);
            }
            return View();

        }

        public ActionResult LogOut()
        {

            if (Session["perdorues"] == null)
            {
                return View();
            }
            else
            {
                Session.Clear();
                return RedirectToAction("Index", "Kategoris");
            }
        }
        public ActionResult LogoutManager()
        {

            if (Session["menaxher"] == null)
            {
                return View();
            }
            else
            {
                Session.Clear();
                return RedirectToAction("Index", "Kategoris");
            }
        }
        public ActionResult LogoutAdmin()
        {

            if (Session["administrator"] == null)
            {
                return View();
            }
            else
            {
                Session.Clear();
                return RedirectToAction("Index", "Kategoris");
            }
        }


        [NonAction]
        public bool IsEmailVerified(String Email)
        {


            var v = db.Perdorues.Where(x => x.Email == Email).FirstOrDefault();
            return v != null;


        }



        [HttpGet]
        public ActionResult Rezervimet(int? id)
        {
            try
            {
                if (Session["perdorues"] == null)
                {
                    return RedirectToAction("Index", "Kategoris");
                }
                else
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    List<Rezervimi> R = new List<Rezervimi>();
                    R = db.Rezervimis.Where(x => x.ID_Perdorues == id && x.Anulluar != true && x.Refuzuar != true).ToList();

                    if (R == null)
                    {
                        return HttpNotFound();
                    }

                    return View(R);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Kategori Controller, Edit Action", ex);
            }
            return View();
        }
        public ActionResult Anullo(int? id, int id_filmi_salla)
        {
            if (Session["perdorues"] == null)
            {
                return RedirectToAction("Index", "Kategoris");
            }
            else
            {
                if (ModelState.IsValid)
                {

                    try
                    {

                        db.Rezervimis.Where(p => p.Id_Rezervim == id).ToList().ForEach(x => x.Anulluar = true);
                        db.SaveChanges();
                        db.Filma_Salla.Where(p => p.Id_filmi_salla == id_filmi_salla).ToList().ForEach(x => x.Salla.Statusi = "Available");
                        db.SaveChanges();
                        TempData["anullosuccess"] = "<script>alert('U anullua me sukses!');</script>";
                    }

                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Trace.TraceInformation("Property: {0} Error: {1}",
                                                        validationError.PropertyName,
                                                        validationError.ErrorMessage);
                            }
                        }
                        logger.ErrorException("Error u gjend ne Kontroller Perdorues,Anullo Action", dbEx);
                    }


                    return RedirectToAction("Rezervimet", "Perdorues", new { id = Session["perdorues"] });

                }
                else
                {

                    return RedirectToAction("Rezervimet", "Perdorues", new { id = Session["perdorues"] });
                }
            }
        }
    }
}