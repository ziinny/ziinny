using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ITHELPDESK.Models
{
    public class TaskModel
    {
        [Required]
        public int? TaskID { get; set; }

        [Required]
        public string TaskName { get; set; }

        [Required]
        public string Description { get; set; }

        public string Image { get; set; }

        public string FileName { get; set; }
    }
}