namespace ITHELPDESK.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tbLogTask")]
    public partial class tbLogTask
    {
        [Key]
        public int Rec { get; set; }

        public int? TaskID { get; set; }

        [StringLength(50)]
        public string TaskName { get; set; }

        [StringLength(1)]
        public string delflag { get; set; }
    }
}
