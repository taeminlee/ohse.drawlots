using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SQLite;
using System.Reflection;
using SQLite.CodeFirst;

namespace ohse.drawlots
{
    public class DrawlotsModel : DbContext
    {
        public DrawlotsModel(string connectionStr)
            : base(new SQLiteConnection() { ConnectionString = connectionStr }, true)
        {
            UpdateModel();
            Configuration.AutoDetectChangesEnabled = false;
            Configuration.EnsureTransactionsForFunctionsAndCommands = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<DrawlotsModel>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public void UpdateModel()
        {
            SQLiteCodeFirstMigrateHelper.UpdateModel(this);
        }

        public virtual DbSet<@class2> @class { get; set; }
        public virtual DbSet<forget2> forget { get; set; }
        public virtual DbSet<group2> group { get; set; }
        public virtual DbSet<history2> history { get; set; }
        public virtual DbSet<student2> student { get; set; }

    }

    [Table("class")]
    public class @class2
    {
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 1)]
        public int cid { get; set; }
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 2)]
        public int year { get; set; }
        public long class1 { get; set; }
    }

    [Table("forget")]
    public class forget2
    {
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 1)]
        public int cid { get; set; }
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 2)]
        public System.DateTime datetime { get; set; }
    }

    [Table("group")]
    public class group2
    {
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 1)]
        public int cid { get; set; }
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 2)]
        public int gid { get; set; }
        public string name { get; set; }
    }

    [Table("history")]
    public class history2
    {
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 1)]
        public int sid { get; set; }
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 2)]
        public System.DateTime date { get; set; }
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 3)]
        public int cid { get; set; }
    }

    [Table("student")]
    public class student2
    {
        public string idName => $"{num}. {name}";
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 1)]
        public int cid { get; set; }
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column(Order = 2)]
        public int sid { get; set; }
        public int num { get; set; }
        public string name { get; set; }
        public int gid { get; set; }
    }
}
