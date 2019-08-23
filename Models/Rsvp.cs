using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WeddingPlanner.Models {
    public class Rsvp{
        [Key]

        // this is the Primary key id for associated rsvps 
        public int RsvpId { get; set; }

        //this is the id of the user rsvp'ing
        public int UserId{get;set;}

        // This is the "object' associated with the ID of the rsvp'er
        public User user {get;set;}

        // this is the id of the Wedding the rsvp'r is rsvp'ing to
        public int WeddingId{ get;set; }

        // this is the "objects" of the wedding associated with the id above 
        public Wedding wedding {get;set;}

    }
}