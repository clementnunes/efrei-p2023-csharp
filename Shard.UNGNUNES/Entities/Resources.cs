using System.Collections.Generic;

namespace Shard.UNGNUNES.Entities
{
    public class Resources
    {
        private List<Resource> _resources;
        private int _number;

        public Resources()
        {
            _resources = new List<Resource>();
            _number = 0;
        }
        
        public Resources(List<Resource> resourcesList, int number)
        {
            _resources = resourcesList;
            _number = number;
        }

        public void AddResource(Resource resource)
        {
            _resources.Add(resource);
            _number++;
        }
    }
}