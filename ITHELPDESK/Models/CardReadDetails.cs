using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITHELPDESK.Models
{
    public class CardReadDetails
    {
        public string ComputerName { get; set; }
        public int? TaskID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string StatusName { get; set; }
    }
}