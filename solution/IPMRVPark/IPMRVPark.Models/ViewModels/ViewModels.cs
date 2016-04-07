using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPMRVPark.Models
{
    public class SelectionOptionID
    {
        public long ID { get; set; }
        public string Label { get; set; }
        public SelectionOptionID(long id, string label)
        {
            ID = id;
            Label = label;
        }
    }
    public class SelectionOptionCode
    {
        public string ID { get; set; }
        public string Label { get; set; }
        public SelectionOptionCode(string id, string label)
        {
            ID = id;
            Label = label;
        }
    }

    // Calculate Total for Selected Site
    public class CalcSiteTotal
    {
        public int duration;
        public int weeks;
        public int days;
        public decimal amount;
        public decimal total;

        public CalcSiteTotal(DateTime checkInDate, DateTime checkOutDate,
            decimal weeklyRate, decimal dailyRate, bool isChecked)
        {
            duration = (int)(checkOutDate - checkInDate).TotalDays;
            weeks = duration / 7;
            days = duration % 7;
            amount = weeklyRate * weeks +
                dailyRate * days;
            total = (isChecked) ? amount : 0;
        }
    }
}