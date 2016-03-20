using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPMRVPark.Models.View
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
}
