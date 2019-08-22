using WeddingPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models{
    public class IndexViewModel
    {
        public Login Login {get;set;}
        public User User { get;set; }
    }
}