using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using System.Collections.Generic;

namespace IPMRVPark.WebUI.Controllers
{
    public class StaffController : Controller
    {
        IRepositoryBase<staff_view> staffs_view;
        IRepositoryBase<staff> staffs;
        IRepositoryBase<person> persons;

        public StaffController(IRepositoryBase<staff_view> staffs_view, 
                IRepositoryBase<staff> staffs,
                IRepositoryBase<person> persons)
        {
            this.staffs_view = staffs_view;
            this.staffs = staffs;
            this.persons = persons;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
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
            staff_view staff_view = staffs_view.GetAll().
                Where(c => c.id == id).FirstOrDefault();

            //var staff_view = staffs_view.GetById(id);
            if (staff_view == null)
            {
                return HttpNotFound();
            }
            return View(staff_view);
        }

        // GET: /Create
        public ActionResult CreateStaff()
        {
            var staff_view = new staff_view();
            return View(staff_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateStaff(staff_view staff_form_page)
        {
            var _person = new person();
            _person.firstName = staff_form_page.firstName;
            _person.lastName = staff_form_page.lastName;
            _person.mainPhone = staff_form_page.mainPhone;
            _person.email = staff_form_page.email;
            _person.createDate = DateTime.Now;
            _person.lastUpdate = DateTime.Now;
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
   }
}
