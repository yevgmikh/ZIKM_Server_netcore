using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ZIKM.Services.Storages.Model {
    class StorageContext : DbContext {

        public DbSet<User> Users { get; set; }
        public DbSet<DataFile> Files { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<FolderTree> FolderTrees { get; set; }
        public DbSet<UserPassword> UserPasswords { get; set; }

        public StorageContext(DbContextOptions<StorageContext> options) : base(options) {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<FolderTree>().HasAlternateKey(t => new { t.Name, t.ParentFolderId });

            var folders = new Folder[] {
                new Folder { Id = 1, Permission = 0 },
                new Folder { Id = 2, Permission = 1 },
                new Folder { Id = 3, Permission = 2 },
                new Folder { Id = 4, Permission = 3 },
                new Folder { Id = 5, Permission = 4 }
            };

            modelBuilder.Entity<Folder>().HasData(folders);

            byte[] example = Encoding.UTF8.GetBytes("This your file;)");
            var files = new DataFile[] {
                new DataFile { Id = 1, Permission = 1, Data = example },
                new DataFile { Id = 2, Permission = 2, Data = example },
                new DataFile { Id = 3, Permission = 3, Data = example },
                new DataFile { Id = 4, Permission = 4, Data = example }
            };
            modelBuilder.Entity<DataFile>().HasData(files);

            modelBuilder.Entity<FolderTree>().HasData(new FolderTree[] {
                new FolderTree("root", folders[0], folders[0]) { Id = 1 },
                new FolderTree("User's folder", folders[0], folders[1]) { Id = 2 },
                new FolderTree("Kouhai's folder", folders[0], folders[2]) { Id = 3 },
                new FolderTree("Sempai's folder", folders[0], folders[3]) { Id = 4 },
                new FolderTree("Master folder", folders[0], folders[4]) { Id = 5 },
                new FolderTree("Some", folders[1], files[0]) { Id = 6 },
                new FolderTree("Kouhai's file", folders[2], files[1]) { Id = 7 },
                new FolderTree("Sempai's file", folders[3], files[2]) { Id = 8 },
                new FolderTree("Master file", folders[4], files[3]) { Id = 9 }
            });

            var users = new User[] {
                new User { Id = 1, Name = "user1", AccessLevel = 1 },
                new User { Id = 2, Name = "Kouhai", AccessLevel = 2 },
                new User { Id = 3, Name = "Senpai", AccessLevel = 3 },
                new User { Id = 4, Name = "Master", AccessLevel = 4 }
            };
            modelBuilder.Entity<User>().HasData(users);

            modelBuilder.Entity<UserPassword>().HasData(new UserPassword[] {
                new UserPassword(users[3]) { Id = 1, Password = "ohmy", IsUsed = false },
                new UserPassword(users[3]) { Id = 2, Password = "azuregale", IsUsed = false },
                new UserPassword(users[2]) { Id = 3, Password = "hmmm", IsUsed = false },
                new UserPassword(users[2]) { Id = 4, Password = "perveredkouhai", IsUsed = false },
                new UserPassword(users[1]) { Id = 5, Password = "Senpai", IsUsed = false },
                new UserPassword(users[1]) { Id = 6, Password = "be_gentle", IsUsed = false },
                new UserPassword(users[0]) { Id = 7, Password = "ghfgh", IsUsed = false },
                new UserPassword(users[0]) { Id = 8, Password = "2123", IsUsed = false },
                new UserPassword(users[0]) { Id = 9, Password = "675", IsUsed = false },
            });
        }
    }
}
