namespace ITHELPDESK.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbCardStatus
    {
        public int ID { get; set; }

        public int? StatusID { get; set; }

        [StringLength(100)]
        public string StatusName { get; set; }
    }
}
