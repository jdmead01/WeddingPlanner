using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WeddingPlanner.Models{
    public class Login
    {
        [Key]
        public int id {get; set;}
        [Required]
        [EmailAddress]
        public string Email {get; set;}
        [Required]
        public string Password {get; set;}
        public DateTime createdAt {get; set;} = DateTime.Now;
        public DateTime updatedAt{get; set;} = DateTime.Now;
    }
}