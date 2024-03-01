using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace ITHELPDESK.Entity
{
    public partial class ctxITHELPDESK : DbContext
    {
        public ctxITHELPDESK()
            : base("name=ctxITHELPDESK")
        {
        }

        public virtual DbSet<tbCardColor> tbCardColors { get; set; }
        public virtual DbSet<tbCardStatus> tbCardStatus { get; set; }
        public virtual DbSet<tbCardViewModel> tbCardViewModels { get; set; }
        public virtual DbSet<tbCardTaskKSP> tbCardTaskKSP { get; set; }
        public virtual DbSet<tbCardTaskHQ> tbCardTaskHQ { get; set; }
        public virtual DbSet<tbLoggingADUser> tbLoggingADUsers { get; set; }
        public virtual DbSet<tbLogTask> tbLogTasks { get; set; }
        public virtual DbSet<tbUser> tbUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<tbCardViewModel>()
                .Property(e => e.delflag)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<tbCardTaskKSP>()
                .Property(e => e.delflag)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<tbCardTaskHQ>()
                .Property(e => e.delflag)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<tbLogTask>()
                .Property(e => e.delflag)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<tbUser>()
                .Property(e => e.Delflag)
                .IsFixedLength()
                .IsUnicode(false);
        }
    }
}
