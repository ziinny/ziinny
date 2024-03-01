namespace ITHELPDESK.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tbCardColor")]
    public partial class tbCardColor
    {
        public int ID { get; set; }

        public int? ColorID { get; set; }

        [StringLength(100)]
        public string ColorName { get; set; }
    }
}
