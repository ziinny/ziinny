namespace ITHELPDESK.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tbCardViewModel")]
    public partial class tbCardViewModel
    {
        public int ID { get; set; }

        [StringLength(100)]
        public string ComputerName { get; set; }
        [StringLength(100)]
        public string Username { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        public int? OrderBy { get; set; }

        public string Description { get; set; }

        public int? Status { get; set; }

        public int? Color { get; set; }

        [StringLength(100)]
        public string Location { get; set; }

        [StringLength(200)]
        public string Image { get; set; }

        [StringLength(200)]
        public string LicenseKey { get; set; }

        [StringLength(1)]
        public string delflag { get; set; }

        [StringLength(100)]
        public string UserCreate { get; set; }

        [StringLength(100)]
        public string UserEdit { get; set; }


        [StringLength(100)]
        public string UserDelete { get; set; }

        public DateTime? dateCreate { get; set; }

        public DateTime? dateEdit { get; set; }

        public DateTime? dateDelete { get; set; }
    }
}
