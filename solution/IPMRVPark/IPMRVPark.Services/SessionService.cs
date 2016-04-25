using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IPMRVPark.Services
{
    public class SessionService
    {
        IRepositoryBase<session> sessions;
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<staff_view> users;

        public const string SessionName = "IPMRVPark";

        public SessionService(
            IRepositoryBase<session> sessions,
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<staff_view> users
            )
        {
            this.sessions = sessions;
            this.customers = customers;
            this.users = users;
        }

        private session createNewSession(HttpContextBase httpContext)
        {
            //create a new session.

            //first create a new cookie.
            HttpCookie cookie = new HttpCookie(SessionName);
            //now create a new session and set the creation date.
            session _session = new session();
            _session.createDate = DateTime.Now;
            _session.lastUpdate = DateTime.Now;
            //create a sessionID as Guid and convert to a string to be stored in database
            _session.sessionGUID = Guid.NewGuid().ToString();

            _session.idIPMEvent = 3;
            _session.isLoggedIn = false;
            _session.idStaff = null;
            _session.idCustomer = null;

            //add and persist in the database.
            sessions.Insert(_session);
            sessions.Commit();

            //add the session id to a cookie
            cookie.Value = _session.sessionGUID;
            cookie.Expires = DateTime.Now.AddDays(7);
            httpContext.Response.Cookies.Add(cookie);

            return _session;
        }

        public session GetSession(HttpContextBase httpContext, bool redirectUserLogin)
        {
            HttpCookie cookie = httpContext.Request.Cookies.Get(SessionName);
            session result;
            string _sessionID;
            Guid _sessionGUID;
            if (cookie != null)//checks if cookie is null
            {
                _sessionID = cookie.Value;
                if (Guid.TryParse(_sessionID, out _sessionGUID))
                {
                    if (_sessionGUID != null)//checks if Guid is null
                    {
                        var sessionList = sessions.GetAll();
                        session _session = new session();
                        bool tryResult = false;

                        try //checks if Guid is in database
                        {
                            _session = sessionList.Where(s => s.sessionGUID.Contains(_sessionID)).FirstOrDefault();
                            tryResult = !(_session.Equals(default(session)));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("An error occurred: '{0}'", e);
                        }

                        if (tryResult)//session found in database
                        {
                            //Redirect to login page if user is not authorized
                            if (redirectUserLogin)
                            {
                                RedirectUserLogin(_session, httpContext);
                            }
                            cookie.Expires = DateTime.Now.AddDays(7);//update cookie expiry date
                            return _session;
                        };
                    }
                }
            }
            //Session not found, create new
            result = createNewSession(httpContext);

            return result;
        }

        const long IDnotFound = -1;

        public long GetSessionID(HttpContextBase httpContext, bool redirectUserLogin)
        {
            session _session = GetSession(httpContext, redirectUserLogin);
            return _session.ID;
        }

        private void RedirectUserLogin(session _session, HttpContextBase httpContext)
        {
            if (_session.idStaff == null)
            {
                //Redirect to login page if user is not authorized
                var _Url = httpContext.Request.Url;
                string pathToLogin = _Url.Scheme + "://" + _Url.Authority +
                    "/Login/Login";
                httpContext.Response.Redirect(pathToLogin, false);
            }
        }

        public long GetSessionUserID(HttpContextBase httpContext, bool redirectUserLogin)
        {
            session _session = GetSession(httpContext, redirectUserLogin);

            if (_session.idStaff == null)
            {
                //return IDnotFound;
                return IDnotFound;
            }
            else
            {
                return _session.idStaff.Value;
            }
        }

        public string GetSessionUserName(long sessionID)
        {
            session _session = sessions.GetById(sessionID);

            if (_session.idStaff == null)
            {
                return string.Empty;
            }
            else
            {
                long userID = _session.idStaff.Value;
                staff_view _user = users.GetByKey("id",userID);
                return _user.fullName;
            }           
        }

        private bool GetSessionCustomer(ref customer_view customer, long sessionID)
        {
            // Read customer from session
            session _session = sessions.GetById(sessionID);
            bool customerFound = false;
            try //checks if customer is in database
            {
                customer = customers.GetAll().Where(c => c.id == _session.idCustomer).FirstOrDefault();
                customerFound = !(customer.Equals(default(session)));
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
            // Customer found in database
            return customerFound;
        }

        public long GetSessionCustomerID(long sessionID)
        {
            customer_view _customer = new customer_view();
            if (GetSessionCustomer(ref _customer, sessionID))
            {
                return (_customer.id);
            }
            else
            {
                return IDnotFound;
            }
        }
        public string GetSessionCustomerNamePhone(long sessionID)
        {
            customer_view _customer = new customer_view();
            if (GetSessionCustomer(ref _customer, sessionID))
            {
                return (_customer.fullName + ", " + _customer.mainPhone);
            }
            else
            {
                return string.Empty;
            }
        }
        // Reset session customer
        public void ResetSessionCustomer(long sessionID)
        {
            session _session = sessions.GetById(sessionID);
            _session.idCustomer = null;
            sessions.Update(_session);
            sessions.Commit();
        }

        public long GetSessionIPMEventID(long sessionID)
        {
            session _session = sessions.GetById(sessionID);
            return _session.idIPMEvent;
        }

        public void AdminRole(HttpContextBase httpContext)
        {
            long userID = GetSessionUserID(httpContext, true);
        }
    }
}
