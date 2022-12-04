﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public partial class ApplicationUser : IdentityUser
    {
        //override identity user, add new column
        public bool IsBaseUser { get; set; } = false;
        [MaxLength(100)]
        public string? UserFirstName { get; set; }
        [MaxLength(100)]
        public string? UserLastName { get; set; }
        [MaxLength(10)]
        public string? ApplicationTheme { get; set; }
        [MaxLength(5)]
        public string Culture { get; set; } = "en";
        public DateTime CreateDateTime { get; set; } = DateTime.Now;
        [MaxLength(100)]
        public string? CreateUser { get; set; }
        [MaxLength(100)]
        public DateTime ModDateTime { get; set; }
        public string? ModificationUser { get; set; }
    }
}