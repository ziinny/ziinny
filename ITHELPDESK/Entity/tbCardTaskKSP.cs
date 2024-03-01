namespace ITHELPDESK.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    [Table("tbCardTaskKSP")]
    public partial class tbCardTaskKSP
    {
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [Required]
        public int? TaskID { get; set; }

        [StringLength(200)]
        [Required]
        public string TaskName { get; set; }

        [Required]
        public string Description { get; set; }

        [StringLength(200)]
        public string Image { get; set; }

        [StringLength(1)]
        public string delflag { get; set; }
    }
}
