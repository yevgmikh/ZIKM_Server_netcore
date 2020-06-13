using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ZIKM.Server.Services.Storages.Model {
    class DefaultDbData {
        private readonly Random random = new Random();

        private string HashPassword(string password) {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(salt);
            }

            int iterationCount = random.Next(9000, 11000);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: iterationCount,
                numBytesRequested: 256 / 8));

            return $"{hashed}&8/1{iterationCount}&8/1{Convert.ToBase64String(salt)}";
        }

        private void AddFiles(ModelBuilder modelBuilder) {
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
                new FolderTree { Id = 1, Name = "root", ParentFolderId = folders[0].Id, FolderId = folders[0].Id },
                new FolderTree { Id = 2, Name = "User's folder", ParentFolderId = folders[0].Id, FolderId = folders[1].Id },
                new FolderTree { Id = 3, Name = "Kouhai's folder", ParentFolderId = folders[0].Id, FolderId = folders[2].Id },
                new FolderTree { Id = 4, Name = "Sempai's folder", ParentFolderId = folders[0].Id, FolderId = folders[3].Id },
                new FolderTree { Id = 5, Name = "Master folder", ParentFolderId = folders[0].Id, FolderId = folders[4].Id },
                new FolderTree { Id = 6, Name = "Some", ParentFolderId = folders[1].Id, FileId = files[0].Id },
                new FolderTree { Id = 7, Name = "Kouhai's file", ParentFolderId = folders[2].Id, FileId = files[1].Id },
                new FolderTree { Id = 8, Name = "Sempai's file", ParentFolderId = folders[3].Id, FileId = files[2].Id },
                new FolderTree { Id = 9, Name = "Master file", ParentFolderId = folders[4].Id, FileId = files[3].Id }
            });
        }

        private void AddUsers(ModelBuilder modelBuilder) {
            if (File.Exists("Accounts.json")) {
                var passwords = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(new ReadOnlySpan<byte>(File.ReadAllBytes("Accounts.json")));

                var users = new List<User>() {
                    new User { Id = 1, Name = "Master", AccessLevel = 4 },
                    new User { Id = 2, Name = "Senpai", AccessLevel = 3 },
                    new User { Id = 3, Name = "Kouhai", AccessLevel = 2 }
                };
                int i = 4;
                users.AddRange(passwords.Where(user => !users.Select(u => u.Name).Contains(user.Key))
                    .Select(user => new User { Id = i++, Name = user.Key, AccessLevel = 1 }));

                modelBuilder.Entity<User>().HasData(users);

                i = 1;
                modelBuilder.Entity<UserPassword>().HasData(passwords.SelectMany(user => user.Value, (user, pass) =>
                    new UserPassword(users.FirstOrDefault(u => u.Name == user.Key)) { Id = i++, Password = HashPassword(pass) }).ToArray());

                File.Delete("Accounts.json");
            }
        }

        public void AddData(ModelBuilder modelBuilder) {
            AddFiles(modelBuilder);

            AddUsers(modelBuilder);
        }
    }
}
