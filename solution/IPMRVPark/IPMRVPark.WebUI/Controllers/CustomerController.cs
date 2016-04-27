using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using System.Collections.Generic;
using IPMRVPark.Services;

namespace IPMRVPark.WebUI.Controllers
{
    public class CustomerController : Controller
    {
        IRepositoryBase<customer_view> customers_view;
        IRepositoryBase<staff_view> users;
        IRepositoryBase<customer> customers;
        IRepositoryBase<person> persons;
        IRepositoryBase<provincecode> provincecodes;
        IRepositoryBase<countrycode> countrycodes;
        IRepositoryBase<session> sessions;
        SessionService sessionService;

        public CustomerController(IRepositoryBase<customer_view> customers_view,
                IRepositoryBase<staff_view> users,
                IRepositoryBase<person> persons,
                IRepositoryBase<customer> customers,
                IRepositoryBase<session> sessions,
                IRepositoryBase<provincecode> provincecodes,
                IRepositoryBase<countrycode> countrycodes)
        {
            this.customers_view = customers_view;
            this.users = users;
            this.customers = customers;
            this.persons = persons;
            this.provincecodes = provincecodes;
            this.countrycodes = countrycodes;
            this.sessions = sessions;
            sessionService = new SessionService(
                this.sessions,
                this.customers_view,
                this.users
                );
        }//end Constructor

        // GET: list with filter
        public ActionResult IndexCustomer(string searchString)
        {
            var _session = sessionService.GetSession(this.HttpContext, true, false);
            var customer_view = customers_view.GetAll().OrderBy(q => q.fullName);

            if (!String.IsNullOrEmpty(searchString))
            {
                customer_view = customer_view.Where(s => s.fullName.Contains(searchString)).OrderBy(r => r.fullName);
            }

            return View(customer_view);
        }

        public ActionResult SearchCustomer()
        {
            var _session = sessionService.GetSession(this.HttpContext, true, false);
            return View();
        }

        // GET: /Details/5
        public ActionResult CustomerDetails(long id)
        {
            var _session = sessionService.GetSession(this.HttpContext, true, false);

            ViewBag.CustomerID = id;
            customer_view customer_view = customers_view.GetByKey("id", id);
            if (customer_view == null)
            {
                return HttpNotFound();
            }
            return View(customer_view);
        }

        public ActionResult ErrorMessage()
        {
            return View();
        }

        // Configure dropdown list items
        private void countryCodes(string defaultCountry)
        {
            var countries = countrycodes.GetAll().OrderBy(s => s.name);
            List<SelectListItem> selectCountry = new List<SelectListItem>();
            foreach (var item in countries)
            {
                SelectListItem selectList1Item = new SelectListItem();
                selectList1Item.Value = item.code.ToString();
                selectList1Item.Text = item.name;
                string selectedText = defaultCountry;
                selectList1Item.Selected =
                 (selectList1Item.Text.Contains(selectedText));
                selectCountry.Add(selectList1Item);
            }
            ViewBag.countryCode = selectCountry;
        }
        private void provinceCodes(string defaultProvince)
        {
            var provinces = provincecodes.GetAll().OrderBy(s => s.name);
            List<SelectListItem> selectProvince = new List<SelectListItem>();
            foreach (var item in provinces)
            {
                SelectListItem selectList1Item = new SelectListItem();
                selectList1Item.Value = item.code.ToString();
                selectList1Item.Text = item.name;
                string selectedText = defaultProvince;
                selectList1Item.Selected =
                 (selectList1Item.Text.Contains(selectedText));
                selectProvince.Add(selectList1Item);
            }
            ViewBag.provinceCode = selectProvince;
        }

        // GET: /Create
        public ActionResult CreateCustomer()
        {
            var _session = sessionService.GetSession(this.HttpContext, true, false);

            //Dropdown list for country
            countryCodes("CANADA");

            //Dropdown list for province
            provinceCodes("ONTARIO");

            var customer_view = new customer_view();
            return View(customer_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCustomer(customer_view customer_form_page)
        {
            var _session = sessionService.GetSession(this.HttpContext, true, false);

            //validation check
            var personfirstname = persons.GetAll().Where(s => s.firstName.ToUpper().Contains(customer_form_page.firstName.ToUpper())).ToList();
            var personlastname = persons.GetAll().Where(s => s.lastName.ToUpper().Contains(customer_form_page.lastName.ToUpper())).ToList();
            var personmainphone = persons.GetAll().Where(s => s.mainPhone.ToUpper().Contains(customer_form_page.mainPhone.ToUpper())).ToList();

            var _person = new person();
            _person.firstName = customer_form_page.firstName;
            _person.lastName = customer_form_page.lastName;
            _person.mainPhone = customer_form_page.mainPhone;
            _person.email = customer_form_page.email;

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
            //else if (personfirstname.Count() > 0 && personlastname.Count() > 0 && personmainphone.Count() > 0)
            else if (personfirstname.Count() > 0 && personlastname.Count() > 0)
            {
                return RedirectToAction("ErrorMessage");
            }

            _person.createDate = DateTime.Now;
            _person.lastUpdate = DateTime.Now;

            persons.Insert(_person);
            persons.Commit();

            var _customer = new customer();
            _customer.ID = _person.ID;
            _customer.cellPhone = customer_form_page.cellPhone;
            _customer.homePhone = customer_form_page.homePhone;
            _customer.faxNumber = customer_form_page.faxNumber;
            _customer.comments = customer_form_page.comments;
            _customer.street = customer_form_page.street;
            _customer.city = customer_form_page.city;
            _customer.postalCode = customer_form_page.postalCode;
            _customer.provinceCode = customer_form_page.provinceCode;
            _customer.countryCode = customer_form_page.countryCode;
            _customer.isEmailReceipt = customer_form_page.isEmailReceipt;
            _customer.isPartyMember = customer_form_page.isPartyMember;
            _customer.createDate = DateTime.Now;
            _customer.lastUpdate = DateTime.Now;
            customers.Insert(_customer);
            customers.Commit();

            _session.idCustomer = _customer.ID;
            sessions.Update(_session);
            sessions.Commit();

            return RedirectToAction("CustomerDetails", new { id = _customer.ID });
        }

        // GET: /Edit/5
        public ActionResult EditCustomer(int id)
        {
            var _session = sessionService.GetSession(this.HttpContext, true, false);

            customer_view customer_view = customers_view.GetAll().
                Where(c => c.id == id).FirstOrDefault();

            //Dropdown list for country
            countryCodes(customer_view.countryName);
            sessionService.GetSession(this.HttpContext, true, false);
            //Dropdown list for province
            provinceCodes(customer_view.provinceName);

            if (customer_view == null)
            {
                return HttpNotFound();
            }
            return View(customer_view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCustomer(customer_view customer_form_page)
        {
            var _session = sessionService.GetSession(this.HttpContext, true, false);

            var _person = persons.GetById(customer_form_page.id);

            _person.firstName = customer_form_page.firstName;
            _person.lastName = customer_form_page.lastName;
            _person.mainPhone = customer_form_page.mainPhone;
            _person.email = customer_form_page.email;
            _person.lastUpdate = DateTime.Now;
            persons.Update(_person);
            persons.Commit();

            var _customer = customers.GetById(customer_form_page.id);
            //_customer.ID = _person.ID;
            _customer.cellPhone = customer_form_page.cellPhone;
            _customer.homePhone = customer_form_page.homePhone;
            _customer.faxNumber = customer_form_page.faxNumber;
            _customer.comments = customer_form_page.comments;
            _customer.street = customer_form_page.street;
            _customer.city = customer_form_page.city;
            _customer.postalCode = customer_form_page.postalCode;
            _customer.provinceCode = customer_form_page.provinceCode;
            _customer.countryCode = customer_form_page.countryCode;
            _customer.isEmailReceipt = customer_form_page.isEmailReceipt;
            _customer.isPartyMember = customer_form_page.isPartyMember;
            _customer.lastUpdate = DateTime.Now;
            customers.Update(_customer);
            customers.Commit();

            return RedirectToAction("CustomerDetails", new { id = _customer.ID });
        }

        // GET: /Delete/5
        public ActionResult DeleteCustomer(int id)
        {
            var _session = sessionService.GetSession(this.HttpContext, true, false);

            customer_view customer_view = customers_view.GetAll().
                    Where(c => c.id == id).FirstOrDefault();
            //customer_view customer_view = customers_view.GetById(id);
            if (customer_view == null)
            {
                return HttpNotFound();
            }
            return View(customer_view);
        }
        [HttpPost, ActionName("DeleteCustomer")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            var _session = sessionService.GetSession(this.HttpContext, true, false);

            persons.Delete(customers_view.GetById(customers_view.GetAll().
                    Where(c => c.id == id).FirstOrDefault().id));
            persons.Commit();

            customers.Delete(customers_view.GetById(customers_view.GetAll().
                    Where(c => c.id == id).FirstOrDefault().id));
            customers.Commit();

            return RedirectToAction("Index");
        }
    }
}
