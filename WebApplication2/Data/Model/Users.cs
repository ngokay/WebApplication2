using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Data.Model
{
    [Table("users")]
    public class Users
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("username")]
        public string Username { get; set; } = string.Empty;
        [Column("email")]
        public string Email { get; set; } = string.Empty;
        [Column("passwordhash")]
        public string Passwordhash { get; set; } = string.Empty;
        [Column("salt")]
        public string Salt { get; set; } = string.Empty;
        [Column("createdat")]
        public DateTime Createdat { get; set; }
        [Column("lastloginat")]
        public DateTime Lastloginat { get; set; }
        [Column("isactive")]
        public bool Isactive { get; set; }
        [Column("test1")]
        public long Test1 { get; set; }
    }
}
