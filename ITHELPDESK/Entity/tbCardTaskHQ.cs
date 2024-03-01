namespace ITHELPDESK.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tbCardTaskHQ")]
    public partial class tbCardTaskHQ
    {
        public int ID { get; set; }

        public int? TaskID { get; set; }

        [StringLength(200)]
        public string TaskName { get; set; }

        public string Description { get; set; }

        [StringLength(200)]
        public string Image { get; set; }

        [StringLength(1)]
        public string delflag { get; set; }
    }
}
