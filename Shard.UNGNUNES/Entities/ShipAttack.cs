using System.Collections.Generic;

namespace Shard.UNGNUNES.Entities
{
    public class ShipAttack : Dictionary<string, AttackByTime>
    {
        public void Add(string ship, int damage, double reloadTime)
        {
            AttackByTime attackByTime = new AttackByTime();
            attackByTime.Damage = damage;
            attackByTime.LoadingTime = reloadTime;
            Add(ship, attackByTime);
        }
    }
}