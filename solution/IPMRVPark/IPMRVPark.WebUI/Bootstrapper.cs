using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc3;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Models;

namespace IPMRVPark.WebUI
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers
            
            // e.g. container.RegisterType<ITestService, TestService>();            
           
            container.RegisterType<IRepositoryBase<customer>, CustomerRepository>();
            container.RegisterType<IRepositoryBase<customer_view>, CustomerViewRepository>();
            container.RegisterType<IRepositoryBase<provincecode>, ProvinceRepository>();
            container.RegisterType<IRepositoryBase<province_view>, ProvinceViewRepository>();
            container.RegisterType<IRepositoryBase<countrycode>, CountryRepository>();
            container.RegisterType<IRepositoryBase<ipmevent>, IPMEventRepository>();
            container.RegisterType<IRepositoryBase<session>, SessionRepository>();
            container.RegisterType<IRepositoryBase<selecteditem>, SelectedItemRepository>();
            container.RegisterType<IRepositoryBase<staff>, StaffRepository>();
            container.RegisterType<IRepositoryBase<staff_view>, StaffViewRepository>();
            container.RegisterType<IRepositoryBase<person>, PersonRepository>();
            container.RegisterType<IRepositoryBase<partymember>, PartyMemberRepository>();
            container.RegisterType<IRepositoryBase<rvsite_available_view>, RVSiteAvailableViewRepository>();
            container.RegisterType<IRepositoryBase<placeinmap>, PlaceInMapRepository>();
            container.RegisterType<IRepositoryBase<reasonforpayment>, ReasonForPaymentRepository>();
            container.RegisterType<IRepositoryBase<paymentmethod>, PaymentMethodRepository>();
            container.RegisterType<IRepositoryBase<paydoctype>, PayDocTypeRepository>();
            container.RegisterType<IRepositoryBase<service>, ServiceRepository>();
            container.RegisterType<IRepositoryBase<sitesize>, SiteSizeRepository>();
            container.RegisterType<IRepositoryBase<reasonforpayment>, ReasonForPaymentRepository>();
            container.RegisterType<IRepositoryBase<payment>, PaymentRepository>();
            container.RegisterType<IRepositoryBase<reservationitem>, ReservationItemRepository>();
            container.RegisterType<IRepositoryBase<paymentreservationitem>, PaymentReservationItemRepository>();
            container.RegisterType<IRepositoryBase<site_description_rate_view>, SiteDescriptionRateViewRepository>();

            container.RegisterType<IRepositoryBase<coordinate>, CoordinateRepository>();
            container.RegisterType<IRepositoryBase<sitetype_service_rate_view>, SiteType_Rate_Repository>();
            container.RegisterType<IRepositoryBase<rvsite_status_view>, RvSiteStatusRepository>();
            container.RegisterType<IRepositoryBase<styleurl>, StyleurlRepository>();
            container.RegisterType<IRepositoryBase<sitetype>, SitetypeRepository>();
            container.RegisterType<IRepositoryBase<siterate>, SiterateRepository>();
			container.RegisterType<IRepositoryBase<payment_view>, PaymentViewRepository>();

            return container;
        }
    }
}