using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using System.Collections.Generic;

namespace IPMRVPark.WebUI.Controllers
{
    public class PartyMemberController : Controller
    {
        IRepositoryBase<partymember_view> partymembers_view;
        IRepositoryBase<partymember> partymembers;
        IRepositoryBase<person> persons1;

        public PartyMemberController(IRepositoryBase<partymember_view> partymembers_view, 
                IRepositoryBase<person> persons,
                IRepositoryBase<partymember> partymembers)
        {
            this.partymembers_view = partymembers_view;
            this.partymembers = partymembers;
            this.persons1 = persons;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var partymember_view = partymembers_view.GetAll().OrderBy(q => q.fullName);

            if (!String.IsNullOrEmpty(searchString))
            {
                partymember_view = partymember_view.Where(s => s.fullName.Contains(searchString)).OrderBy(r => r.fullName);
            }

            return View(partymember_view);
        }

        // GET: /Details/5
        public ActionResult PartyMemberDetails(int? id)
        {
            partymember_view partymember_view = partymembers_view.GetAll().
                Where(c => c.id == id).FirstOrDefault();

            if (partymember_view == null)
            {
                return HttpNotFound();
            }
            return View(partymember_view);
        }

        // GET: /Create
        public ActionResult CreatePartyMember()
        {
            var partymember_view = new partymember_view();
            return View(partymember_view);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePartyMember(partymember_view partymember_form_page)
        {
            var _person = new person();
            _person.firstName = partymember_form_page.firstName;
            _person.lastName = partymember_form_page.lastName;
            _person.mainPhone = partymember_form_page.mainPhone;
            _person.email = partymember_form_page.email;
            _person.createDate = DateTime.Now;
            _person.lastUpdate = DateTime.Now;
            persons1.Insert(_person);
            persons1.Commit();

            var _partymember = new partymember();
            _partymember.ID = _person.ID;
            _partymember.cellPhone = partymember_form_page.cellPhone;
            _partymember.petDescription = partymember_form_page.petDescription;
            _partymember.comments = partymember_form_page.comments;
            _partymember.createDate = DateTime.Now;
            _partymember.lastUpdate = DateTime.Now;
            partymembers.Insert(_partymember);
            partymembers.Commit();

            return RedirectToAction("Index");
        }
        // GET: /Edit/5
        public ActionResult EditPartyMember(int id)
        {
            partymember_view partymember_view = partymembers_view.GetAll().
                Where(c => c.id == id).FirstOrDefault();

            if (partymember_view == null)
            {
                return HttpNotFound();
            }
            return View(partymember_view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPartyMember(partymember_view partymember_form_page)
        {
            var _person = persons1.GetById(partymember_form_page.id);

            _person.firstName = partymember_form_page.firstName;
            _person.lastName = partymember_form_page.lastName;
            _person.mainPhone = partymember_form_page.mainPhone;
            _person.email = partymember_form_page.email;
            _person.lastUpdate = DateTime.Now;
            persons1.Update(_person);
            persons1.Commit();

            var _partymember = partymembers.GetById(partymember_form_page.id);
            _partymember.cellPhone = partymember_form_page.cellPhone;
            _partymember.petDescription = partymember_form_page.petDescription;
            _partymember.comments = partymember_form_page.comments;
            _partymember.lastUpdate = DateTime.Now;
            partymembers.Update(_partymember);
            partymembers.Commit();

            return RedirectToAction("Index");
        }
   }
}
