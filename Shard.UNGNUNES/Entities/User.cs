using System;
using System.Collections.Generic;

namespace Shard.UNGNUNES.Entities
{
    public class User
    {
        public string Id { get; }
        public string Pseudo { get; set; }
        public DateTime DateOfCreation { get; }
        
        public IDictionary<string,int> ResourcesQuantity { get; set; }

        public User(string id, string pseudo)
        {
            Id = id;
            Pseudo = pseudo;
            DateOfCreation = DateTime.Now;
            ResourcesQuantity = new Dictionary<string, int>();
            
            ResourcesQuantity.Add("aluminium", 0);
            ResourcesQuantity.Add("carbon", 20);
            ResourcesQuantity.Add("gold", 0);
            ResourcesQuantity.Add("iron", 10);
            ResourcesQuantity.Add("oxygen", 50);
            ResourcesQuantity.Add("titanium", 0);
            ResourcesQuantity.Add("water", 50);
        }
        
        public User(string id, string pseudo, DateTime dateOfCreation) : this(id, pseudo)
        {
            DateOfCreation = dateOfCreation;
        }
    }
}