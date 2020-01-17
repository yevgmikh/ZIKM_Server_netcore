using System.ComponentModel.DataAnnotations;

namespace ZIKM.Services.Storages.Model {
    public class UserPassword {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public virtual User User { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public bool IsUsed { get; set; }

        public UserPassword() { }

        public UserPassword(User user) {
            UserId = user.Id;
        }
    }
}
