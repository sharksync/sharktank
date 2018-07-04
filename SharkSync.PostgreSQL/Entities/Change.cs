using SharkSync.Interfaces.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharkSync.PostgreSQL.Entities
{
    [Table("change")]
    public class Change : IChange
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("accountid")]
        public Guid AccountId { get; set; }

        [Column("applicationid")]
        public Guid ApplicationId { get; set; }

        [Column("groupid")]
        public string GroupId { get; set; }

        [Column("entity")]
        public string Entity { get; set; }

        [Column("recordid")]
        public Guid RecordId { get; set; }

        [Column("property")]
        public string Property { get; set; }

        [Column("clientmodified")]
        public long ClientModified { get; set; }

        [Column("recordvalue")]
        public string RecordValue { get; set; }
    }
}
