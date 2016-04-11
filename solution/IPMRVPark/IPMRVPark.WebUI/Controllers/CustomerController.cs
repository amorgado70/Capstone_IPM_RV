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
        IRepositoryBase<customer> customers;
        IRepositoryBase<person> persons;
        IRepositoryBase<provincecode> provincecodes;
        IRepositoryBase<countrycode> countrycodes;
        IRepositoryBase<session> sessions;
        SessionService sessionService;

        public CustomerController(IRepositoryBase<customer_view> customers_view, 
                IRepositoryBase<person> persons, 
                IRepositoryBase<customer> customers,
                IRepositoryBase<session> sessions,
                IRepositoryBase<provincecode> provincecodes, 
                IRepositoryBase<countrycode> countrycodes)
        {
            this.customers_view = customers_view;
            this.customers = customers;
            this.persons = persons;
            this.provincecodes = provincecodes;
            this.countrycodes = countrycodes;
            this.sessions = sessions;
            sessionService = new SessionService(
                this.sessions,
                this.customers_view
                );
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var customer_view = customers_view.GetAll().OrderBy(q => q.fullName);

            if (!String.IsNullOrEmpty(searchString))
            {
                customer_view = customer_view.Where(s => s.fullName.Contains(searchString)).OrderBy(r =>r.fullName);
            }

            return View(customer_view);
        }

        // GET: /Details/5
        public ActionResult CustomerDetails(int? id)
        {
            customer_view customer_view = customers_view.GetAll().
                Where(c => c.id == id).FirstOrDefault();
            //var customer_view = customers_view.GetById(id);
            if (customer_view == null)
            {
                return HttpNotFound();
            }
            return View(customer_view);
        }

        // GET: /Create
        public ActionResult CreateCustomer()
        {
            //Dropdown list for country
            var country = countrycodes.GetAll();
            ViewBag.Country = country.OrderBy(q => q.name);

            //Dropdown list for province
            //ViewBag.Province = provincecodes.GetAll().OrderBy(s => s.name);
            var province = provincecodes.GetAll();
            ViewBag.Province = province.OrderBy(s => s.name);

            var customer_view = new customer_view();
            return View(customer_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCustomer(customer_view customer_form_page)
        {
            var _person = new person();
            _person.firstName = customer_form_page.firstName;
            _person.lastName = customer_form_page.lastName;
            _person.mainPhone = customer_form_page.mainPhone;
            _person.email = customer_form_page.email;
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

            session _session = sessions.GetById(sessionService.GetSession(this.HttpContext).ID);
            _session.idCustomer = _customer.ID;
            sessions.Update(_session);
            sessions.Commit();

            return RedirectToAction("CustomerDetails", new { id = _customer.ID});
        }

        // GET: /Edit/5
        public ActionResult EditCustomer(int id)
        {
            //Dropdown list for country
            var country = countrycodes.GetAll();
            ViewBag.Country = country.OrderBy(q => q.name);

            //Dropdown list for province
            var province = provincecodes.GetAll();
            ViewBag.Province = province.OrderBy(s => s.name);

            customer_view customer_view = customers_view.GetAll().
                Where(c => c.id == id).FirstOrDefault();

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

            return RedirectToAction("Index");
        }


        // GET: /Delete/5
        public ActionResult DeleteCustomer(int id)
        {
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
            //customer_view customer_view = customers_view.GetAll().
                    //Where(c => c.id == id).FirstOrDefault();
            //var _person = persons.GetById(customer_form_page.id);
            //persons.Delete(customers_view.GetAll().
            //        Where(c => c.id == id).FirstOrDefault().id);
            persons.Delete(customers_view.GetById(customers_view.GetAll().
                    Where(c => c.id == id).FirstOrDefault().id));
            persons.Commit();

            //var _customer = customers.GetById(customer_form_page.id);
            //customers.Delete(customers_view.GetAll().
            //        Where(c => c.id == id).FirstOrDefault().id);
            customers.Delete(customers_view.GetById(customers_view.GetAll().
                    Where(c => c.id == id).FirstOrDefault().id));
            customers.Commit();



            //customers_view.Delete(customers_view.GetById(id));
            //customers_view.Commit();
            return RedirectToAction("Index");
        }
    }
}
