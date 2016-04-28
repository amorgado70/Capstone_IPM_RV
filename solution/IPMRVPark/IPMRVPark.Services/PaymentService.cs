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
    public class PaymentService
    {
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<reservationitem> reservationitems;
        IRepositoryBase<payment> payments;
        IRepositoryBase<paymentreservationitem> paymentsreservationitems;

        public PaymentService(
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<reservationitem> reservationitems,
            IRepositoryBase<payment> payments,
            IRepositoryBase<paymentreservationitem> paymentsreservationitems
            )
        {
            this.selecteditems = selecteditems;
            this.reservationitems = reservationitems;
            this.payments = payments;
            this.paymentsreservationitems = paymentsreservationitems;
        }
        #region Common
        const long IDnotFound = -1;

        // Clean selected items
        const int cleanAll = 1;
        const int cleanNew = 2;
        const int cleanEdit = 3;

        public void CleanAllSelectedItems(long sessionID, long userID)
        {
            CleanSelectedItemList(sessionID, userID, cleanAll);
        }
        public void CleanNewSelectedItems(long sessionID, long userID)
        {
            CleanSelectedItemList(sessionID, userID, cleanNew);
        }
        public void CleanEditSelectedItems(long sessionID, long userID)
        {
            CleanSelectedItemList(sessionID, userID, cleanEdit);
        }
        public void CleanSelectedItem(long sessionID, long userID, long selectedID)
        {
            cleanSelected(sessionID, userID, selectedID);
        }
        private void cleanSelected(long sessionID, long userID, long selectedID)
        {
            var _selecteditem = selecteditems.GetById(selectedID);
            _selecteditem.idSession = sessionID;
            if (userID != IDnotFound)
            {
                _selecteditem.idStaff = userID;
            }
            _selecteditem.isSiteChecked = false;
            _selecteditem.duration = 0;
            _selecteditem.weeks = 0;
            _selecteditem.days = 0;
            _selecteditem.amount = 0;
            _selecteditem.total = 0;
            _selecteditem.lastUpdate = DateTime.Now;
            _selecteditem.reservationCheckInDate = DateTime.MinValue;
            _selecteditem.reservationCheckOutDate = DateTime.MinValue;
            _selecteditem.reservationAmount = 0;
            _selecteditem.reservationAdditionalServAmount = 0;

            selecteditems.Update(_selecteditem);
            selecteditems.Commit();
        }

        private void CleanSelectedItemList(long sessionID, long userID, int cleanCode)
        {
            // Clean edit items that are in selected table
            var _olditems_to_be_removed = selecteditems.GetAll().
                Where(c => c.idSession == sessionID && c.isSiteChecked == true).ToList();
            foreach (var _olditem in _olditems_to_be_removed)
            {
                if (cleanCode == cleanAll ||
                    (cleanCode == cleanNew && _olditem.reservationAmount == 0) ||
                    (cleanCode == cleanEdit && _olditem.reservationAmount != 0))
                {
                    cleanSelected(sessionID, userID, _olditem.ID);
                }
            }
        }

        public void CleanOldSelectedItem(long IPMEventID, long userID)
        {
            DateTime timeStampHigh = DateTime.ParseExact(
                DateTime.UtcNow.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss"),
                "yyyy-MM-dd HH:mm:ss", null).ToUniversalTime();
            DateTime timeStampLow = DateTime.ParseExact(
                DateTime.UtcNow.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss"),
                "yyyy-MM-dd HH:mm:ss", null).ToUniversalTime();

            // Clean edit items that are in selected table
            var _olditems_to_be_removed = selecteditems.GetAll().
                Where(c => c.isSiteChecked == true && c.idIPMEvent == IPMEventID).ToList();
            foreach (var _selecteditem in _olditems_to_be_removed)
            {
                DateTime timeStamp = DateTime.ParseExact(_selecteditem.timeStamp, "yyyy-MM-dd HH:mm:ss", null).ToUniversalTime();

                if (timeStamp > timeStampHigh || timeStamp < timeStampLow)
                {
                    _selecteditem.idStaff = userID;
                    _selecteditem.isSiteChecked = false;
                    _selecteditem.duration = 0;
                    _selecteditem.weeks = 0;
                    _selecteditem.days = 0;
                    _selecteditem.amount = 0;
                    _selecteditem.total = 0;
                    _selecteditem.lastUpdate = DateTime.Now;
                    _selecteditem.timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    _selecteditem.reservationCheckInDate = DateTime.MinValue;
                    _selecteditem.reservationCheckOutDate = DateTime.MinValue;
                    _selecteditem.reservationAmount = 0;
                    _selecteditem.reservationAdditionalServAmount = 0;

                    selecteditems.Update(_selecteditem);
                    selecteditems.Commit();
                }

            }
        }

        #endregion
        #region Payments & Refunds
        public decimal GetProvinceTax(long sessionID)
        {
            return 13; // HST value for Ontario
        }
        public decimal GetCancelationFee(long sessionID)
        {
            return 50; // Value for 2016
        }

        // Sum and Count for Selected Items
        public decimal CalculateNewSelectedTotal(long sessionID, out int count)
        {
            var _selecteditem = selecteditems.GetAll().
                Where(q => q.idSession == sessionID && q.isSiteChecked == true).
                OrderByDescending(o => o.ID);

            count = 0;
            decimal sum = 0;
            if (_selecteditem != null)
            {
                foreach (var i in _selecteditem)
                {
                    count = count + 1;
                    sum = sum + i.total;
                }
            }

            return sum;
        }

        // Sum and Count for Reserved Items
        public decimal CalculateReservedTotal(long customerID)
        {
            var _reserveditems = reservationitems.GetAll().
                Where(q => q.idCustomer == customerID && q.isCancelled != true).
                    OrderByDescending(o => o.ID);

            int count = 0;
            decimal sum = 0;
            foreach (var i in _reserveditems)
            {
                count = count + 1;
                sum = sum + i.total;
            }

            return sum;
        }

        public payment CalculateEditSelectedTotal(long sessionID, long IPMEventID, long customerID)
        {
            payment _payment = new payment();

            var _selecteditems = selecteditems.GetAll();
            _selecteditems = _selecteditems.Where(q => q.idSession == sessionID).
                OrderByDescending(o => o.ID);
            int count = 0;
            decimal selectionTotal = 0; // Thhis selection or edit reservation total
            decimal reservationTotal = 0; // Previous reservation total
            foreach (var _selecteditem in _selecteditems)
            {
                if (_selecteditem.isSiteChecked == true)
                {
                    count = count + 1;
                    selectionTotal = selectionTotal + _selecteditem.total;
                }
                // Check if selected item was a reserved item,
                // this means the selected item is in edit reservation mode
                if (_selecteditem.idReservationItem != null && _selecteditem.idReservationItem != IDnotFound)
                {
                    reservationTotal = reservationTotal + _selecteditem.reservationAmount;
                }
            }

            // *****
            decimal dueAmount = Math.Max((selectionTotal - reservationTotal), 0);
            decimal refundAmount = Math.Max((reservationTotal - selectionTotal), 0);
            // Check if a cancellation fee applies
            decimal cancelationFee = GetCancelationFee(sessionID);
            if (selectionTotal < reservationTotal)
            {
                if ((reservationTotal - selectionTotal) < cancelationFee)
                {
                    refundAmount = 0;
                    dueAmount = cancelationFee - (reservationTotal - selectionTotal);
                }
                else
                {
                    refundAmount = refundAmount - cancelationFee;
                }
            }
            else
            {
                cancelationFee = 0;
            }

            // Value of previous reservation, just before edit reservation mode started
            _payment.primaryTotal = reservationTotal;
            _payment.selectionTotal = selectionTotal;
            _payment.cancellationFee = cancelationFee;
            /// Suggested value for payment
            _payment.amount = dueAmount - refundAmount - CustomerAccountBalance(IPMEventID, customerID);
            _payment.tax = Math.Round((dueAmount * GetProvinceTax(sessionID) / 100), 2, MidpointRounding.AwayFromZero);
            _payment.withoutTax = dueAmount - _payment.tax;

            return _payment;
        }

        public decimal CustomerAccountBalance(long IPMEventID, long customerID)
        {
            if (customerID == IDnotFound)
            {
                return 0;
            };

            var _payments = payments.GetAll().
                Where(p => p.idCustomer == customerID && p.idIPMEvent == IPMEventID).OrderBy(p => p.ID);

            var _last = _payments.ToList().LastOrDefault();
            decimal finalBalance = (_last != null) ? _last.balance : 0;

            return finalBalance;
        }

        public decimal CustomerPreviousBalance(long customerID, long paymentID)
        {
            if (customerID == IDnotFound)
            {
                return 0;
            };

            var _payments = payments.GetAll().
                Where(p => p.idCustomer == customerID && p.ID < paymentID).OrderByDescending(p => p.ID);
            var _p = _payments.ToList();

            if (_p.Count() < 1)
            {
                return 0;
            }
            else
            {
                return _p.First().balance;
            };
        }



        #endregion
    }
}
