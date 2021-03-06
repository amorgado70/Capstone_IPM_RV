using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using System.Collections.Generic;
using IPMRVPark.Services;

namespace IPMRVPark.WebUI.Controllers
{
    public class StaffController : Controller
    {
        IRepositoryBase<customer_view> customers_view;
        IRepositoryBase<staff_view> staffs_view;
        IRepositoryBase<staff> staffs;
        IRepositoryBase<person> persons;
        IRepositoryBase<session> sessions;
        SessionService sessionService;

        public StaffController(
                IRepositoryBase<customer_view> customers_view,
                IRepositoryBase<staff_view> staffs_view,
                IRepositoryBase<staff> staffs,
                IRepositoryBase<person> persons,
                IRepositoryBase<session> sessions)
        {
            this.customers_view = customers_view;
            this.staffs_view = staffs_view;
            this.staffs = staffs;
            this.persons = persons;
            this.sessions = sessions;
            sessionService = new SessionService(
                this.sessions,
                this.customers_view,
                this.staffs_view
                );
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            sessionService.GetSessionID(this.HttpContext, true, true);

            var staff_view = staffs_view.GetAll().OrderBy(q => q.fullName);

            if (!String.IsNullOrEmpty(searchString))
            {
                staff_view = staff_view.Where(s => s.fullName.Contains(searchString)).OrderBy(r => r.fullName);
            }

            return View(staff_view);
        }

        // GET: /Details/5
        public ActionResult StaffDetails(int? id)
        {
            sessionService.GetSessionID(this.HttpContext, true, true);

            staff_view staff_view = staffs_view.GetAll().
                Where(c => c.id == id).FirstOrDefault();

            //var staff_view = staffs_view.GetById(id);
            if (staff_view == null)
            {
                return HttpNotFound();
            }
            return View(staff_view);
        }

        public ActionResult ErrorMessage()
        {
            sessionService.GetSessionID(this.HttpContext, true, true);

            return View();
        }

        // GET: /Create
        public ActionResult CreateStaff()
        {
            sessionService.GetSessionID(this.HttpContext, true, true);

            var staff_view = new staff_view();
            return View(staff_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateStaff(staff_view staff_form_page)
        {
            sessionService.GetSessionID(this.HttpContext, true, true);

            //validation check
            var personfirstname = persons.GetAll().Where(s => s.firstName.ToUpper().Contains(staff_form_page.firstName.ToUpper())).ToList();
            var personlastname = persons.GetAll().Where(s => s.lastName.ToUpper().Contains(staff_form_page.lastName.ToUpper())).ToList();
            var personmainphone = persons.GetAll().Where(s => s.mainPhone.ToUpper().Contains(staff_form_page.mainPhone.ToUpper())).ToList();


            var _person = new person();
            _person.firstName = staff_form_page.firstName;
            _person.lastName = staff_form_page.lastName;
            _person.mainPhone = staff_form_page.mainPhone;
            _person.email = staff_form_page.email;
            _person.password = sessionService.GetHash("012345");
            _person.createDate = DateTime.Now;
            _person.lastUpdate = DateTime.Now;

            //first, last name and main phone validation

            if (_person.firstName == null)
            {
                return RedirectToAction("ErrorMessage");
            }
            else if (_person.firstName.Trim().Length > 50)
            {
                return RedirectToAction("ErrorMessage");
            }
            else if (_person.lastName == null)
            {
                return RedirectToAction("ErrorMessage");
            }
            else if (_person.lastName.Trim().Length > 50)
            {
                return RedirectToAction("ErrorMessage");
            }
            else if (_person.mainPhone == null)
            {
                return RedirectToAction("ErrorMessage");
            }
            else if (_person.mainPhone.Trim().Length > 30)
            {
                return RedirectToAction("ErrorMessage");
            }
            else if (personfirstname.Count() > 0 && personlastname.Count() > 0 && personmainphone.Count() > 0)
            //else if (personfirstname.Count() > 0 && personlastname.Count() > 0)
            {
                return RedirectToAction("ErrorMessage");
            }

            persons.Insert(_person);
            persons.Commit();

            var _staff = new staff();
            _staff.ID = _person.ID;
            _staff.role = staff_form_page.role;
            _staff.createDate = DateTime.Now; 
            _staff.lastUpdate = DateTime.Now;
            staffs.Insert(_staff);
            staffs.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult EditStaff(int id)
        {
            sessionService.GetSessionID(this.HttpContext, true, true);

            staff_view staff_view = staffs_view.GetAll().
                Where(c => c.id == id).FirstOrDefault();
 
            if (staff_view == null)
            {
                return HttpNotFound();
            }
            return View(staff_view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStaff(staff_view staff_form_page)
        {
            sessionService.GetSessionID(this.HttpContext, true, true);

            var _person = persons.GetById(staff_form_page.id);

            _person.firstName = staff_form_page.firstName;
            _person.lastName = staff_form_page.lastName;
            _person.mainPhone = staff_form_page.mainPhone;
            _person.email = staff_form_page.email;
            _person.lastUpdate = DateTime.Now;
            persons.Update(_person);
            persons.Commit();

            var _staff = staffs.GetById(staff_form_page.id);
           // _staff.ID = _person.ID;
            _staff.role = staff_form_page.role;
            _staff.lastUpdate = DateTime.Now;
            staffs.Update(_staff);
            staffs.Commit();

            return RedirectToAction("Index");
        }

        public ActionResult ResetPassword()
        {
            sessionService.GetSessionID(this.HttpContext, true, true);

            var staff_view = staffs_view.GetAll().OrderBy(q => q.fullName);

            return View(staff_view);
        }

    }
}
