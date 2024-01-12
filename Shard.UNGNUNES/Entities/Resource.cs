namespace Shard.UNGNUNES.Entities
{
    public class Resource
    {
        private string resourceName { get; }
        private int resourceQuantity { get; }

        public Resource(string name, int quantity)
        {
            resourceName = name;
            resourceQuantity = quantity;
        }
    }
}