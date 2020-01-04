﻿using System.ComponentModel.DataAnnotations;

namespace ZIKM.Infrastructure.Storages.Model {
    public class DataFile {
        [Key]
        public int Id { get; set; }
        [Required]
        public uint Permission { get; set; }
        [Required]
        public byte[] Data { get; set; }
    }
}
