using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace WeddingPlanner.Models{
    public class Wedding
    {
        [Key]
        public int weddingId {get;set;}

        [Required]
        public string wedderOne {get;set;}
        [Required]
        public string wedderTwo{get;set;}

        [Required]
        public DateTime weddingDate{get;set;} 

        [Required]
        public string address {get;set;}

        public int id {get;set;}
        public User creator {get;set;}

    }
}