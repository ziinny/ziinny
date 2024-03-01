namespace ITHELPDESK.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tbLoggingADUser")]
    public partial class tbLoggingADUser
    {
        [Key]
        public int Rec { get; set; }
        [StringLength(50)]
        public string taskName { get; set; }

        [StringLength(100)]
        public string userName { get; set; }

        [StringLength(100)]
        public string submittedBy { get; set; }

        public DateTime? submittedDate { get; set; }

        [StringLength(30)]
        public string IP_DC_Locked { get; set; }
    }
}
