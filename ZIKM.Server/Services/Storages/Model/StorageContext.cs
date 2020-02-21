using Microsoft.EntityFrameworkCore;

namespace ZIKM.Server.Services.Storages.Model {
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

            new DefaultDbData().AddData(modelBuilder);
        }
    }
}
