using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shard.Shared.Core;

namespace Shard.UNGNUNES.Entities
{
    public class Building
    {
        public string Id { get; }

        public string Type { get; }
        
        [IgnoreDataMember]
        public string ResourceCategory { get; }
        
        public string BuilderId { get; }
        
        public string System { get; }

        public string Planet { get; }
        
        public bool IsBuilt { get; set; }
        
        public string EstimatedBuildTime { get; set; }

        public Building(string id, string type, string resourceCategory, string builderId, string system, string planet ,DateTime now)
        {
            Id = id;
            Type = type;
            if (type == "mine")
                ResourceCategory = resourceCategory;
            BuilderId = builderId;
            System = system;
            Planet = planet;
            IsBuilt = false;
            EstimatedBuildTime = now.AddMinutes(5).ToString();
        }
    }
}