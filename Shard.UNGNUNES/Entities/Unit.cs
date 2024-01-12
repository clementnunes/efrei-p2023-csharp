using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Shard.UNGNUNES.Entities
{
    public class Unit
    {
        public string Id { get; }
        public string Type { get; }
        public string System { set; get; }
        public string Planet { set; get; }
        
        public string DestinationSystem { set; get; }
        
        public string DestinationPlanet { set; get; }
        
        public string EstimatedTimeOfArrival { set; get; }

        public int Health { set; get; }
        
        [IgnoreDataMember]
        public Dictionary<string,int> ResourcesQuantity { set; get; }
        
        private UnitLocation _unitLocation;

        public Unit(string id, string type, UnitLocation unitLocation)
        {
            Id = id;
            Type = type;
            _unitLocation = unitLocation;
            System = unitLocation.System;
            
            if (unitLocation.Planet is not null) 
            { 
                Planet = unitLocation.Planet;
            }
        }
        public Unit(string id, string type)
        {
            Id = id;
            Type = type;
            _unitLocation = null;
            System = null;
            Planet = null;
        }

        public UnitLocation GetUnitLocation()
        {
            return _unitLocation;
        }

        public UnitLocation GetHiddenUnitLocation()
        {
            return new UnitLocation(_unitLocation.Description, _unitLocation.System, _unitLocation.Planet, null);
        }

        public void SetUnitLocation(UnitLocation unitLocation)
        {
            _unitLocation = unitLocation;
        }
    }
}