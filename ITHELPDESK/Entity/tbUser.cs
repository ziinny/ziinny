namespace ITHELPDESK.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbUser
    {
        [Key]
        public int Rec { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        [StringLength(30)]
        public string Location { get; set; }

        [StringLength(1)]
        public string Delflag { get; set; }

        public DateTime? LastLogin { get; set; }

        [StringLength(50)]
        public string Role { get; set; }
    }
}
