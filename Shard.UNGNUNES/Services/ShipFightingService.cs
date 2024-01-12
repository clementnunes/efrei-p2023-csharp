using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Shard.Shared.Core;
using Shard.UNGNUNES.Entities;

namespace Shard.UNGNUNES.Services
{
    public class ShipFightingService
    {
        private readonly UnitsService _unitsService;
        private static ShipAttack _attackDamages;
        private readonly IClock _clock;

        public ShipFightingService(UnitsService unitsService, IClock clock)
        {
            _unitsService = unitsService;
            _attackDamages = new ShipAttack();
            _clock = clock;

            _attackDamages.Add("fighter", 10, 6);
            _attackDamages.Add("bomber", 400, 60);
            _attackDamages.Add("cruiser", 40, 6);
        }

        public List<Unit> GetUnitsOnSamePlanet(StarSystem starSystem, Planet planet)
        {
            return (from userShips in _unitsService.UnitsList
                    from userUnit in userShips.Value
                    where
                        String.Equals(userUnit.System, starSystem.Name)
                        && String.Equals(userUnit.Planet, planet.Name)
                    select userUnit)
                .ToList();
        }

        private List<Unit> GetUnitsOnSameSystem(StarSystem starSystem)
        {
            return (from userShips in _unitsService.UnitsList
                    from userUnit in userShips.Value
                    where
                        String.Equals(userUnit.System, starSystem.Name)
                        && userUnit.Planet is null
                    select userUnit)
                .ToList();
        }

        public double GetAttackTimeByType(string type)
        {
            return
                (from attackDamage in _attackDamages
                    where String.Equals(attackDamage.Key, type)
                    select attackDamage.Value.LoadingTime)
                .FirstOrDefault();
        }
        
        #nullable enable
        private AttackByTime? GetDamageByType(string type)
        {
            return
                (from attackDamage in _attackDamages
                    where String.Equals(attackDamage.Key, type)
                    select attackDamage.Value)
                .FirstOrDefault();
        }

        #nullable enable
        public KeyValuePair<string, AttackByTime>? GetDamageAndTypeByType(string type)
        {
            AttackByTime? attackByTime = GetDamageByType(type);

            if (attackByTime is null)
                return null;

            return new KeyValuePair<string, AttackByTime>(type, attackByTime);
        }

        private List<Unit> PickByType(List<Unit> units, string unitType)
        {
            List<Unit> priorityUnits = new List<Unit>();

            foreach (var unit in units)
            {
                if (String.Equals(unit.Type, unitType) && unit.Health > 0)
                    priorityUnits.Add(unit);
            }

            return priorityUnits;
        }

        private List<Unit> PickByPriority(Unit mainUnit, List<Unit> units)
        {
            List<Unit> priorityUnits = new List<Unit>();

            switch (mainUnit.Type)
            {
                case "fighter":
                    priorityUnits = PickByType(units, "bomber");

                    if (priorityUnits.Count == 0)
                    {
                        priorityUnits = PickByType(units, "fighter");
                        priorityUnits.Remove(mainUnit);
                    }

                    if (priorityUnits.Count == 0)
                        priorityUnits = PickByType(units, "cruiser");

                    break;

                case "bomber":
                    priorityUnits = PickByType(units, "cruiser");

                    if (priorityUnits.Count == 0)
                    {
                        priorityUnits = PickByType(units, "bomber");
                        priorityUnits.Remove(mainUnit);
                    }

                    if (priorityUnits.Count == 0)
                        priorityUnits = PickByType(units, "fighter");

                    break;

                case "cruiser":
                    priorityUnits = PickByType(units, "fighter");

                    if (priorityUnits.Count == 0)
                    {
                        priorityUnits = PickByType(units, "cruiser");
                        priorityUnits.Remove(mainUnit);
                    }

                    if (priorityUnits.Count == 0)
                        priorityUnits = PickByType(units, "bomber");

                    break;

                default:
                    return priorityUnits;
            }

            return priorityUnits;
        }

        #nullable enable
        private static AttackByTime? GetAttackByTime(ShipAttack shipAttacks, string type)
        {
            foreach (var shipAttack in shipAttacks)
            {
                if (String.Equals(shipAttack.Key, type))
                    return shipAttack.Value;
            }

            return null;
        }

        public void ShipAttack(Unit unit1, Unit unit2, AutoResetEvent autoResetEvent)
        {
            AttackByTime? attackByTime = GetAttackByTime(_attackDamages, unit1.Type);

            if (attackByTime is null)
                return;

            unit2.Health = -attackByTime.Damage;

            if (unit2.Health <= 0)
            {
                _unitsService.DeleteUnit(unit2);
                autoResetEvent.Set();
            }
        }

        private void ShipAttack(Unit unit1, Unit unit2, AttackByTime attackByTime)
        {
            if (unit1.Type == "cruiser" && unit2.Type == "bomber")
                unit2.Health -= attackByTime.Damage / 10;
            else
                unit2.Health -= attackByTime.Damage;
        }
        
        public void ShipFight(IClock clock)
        {
            List<Unit> allUnits = _unitsService.GetAllUnits();
            List<Unit> firingUnits = new List<Unit>();
            
            if (clock.Now.Second % 6 == 0)
                firingUnits = firingUnits.Concat(from fetchedUnit in allUnits
                    where fetchedUnit.Type is "cruiser" or "fighter"
                    select fetchedUnit).ToList();
            
            if (clock.Now.Second % 60 == 0)
                firingUnits = firingUnits.Concat(from fetchedUnit in allUnits
                    where fetchedUnit.Type is "bomber"
                    select fetchedUnit).ToList();
            
            foreach (var firingUnit in firingUnits)
            {
                Fire(firingUnit);
            }
            
            foreach (Unit unit in allUnits)
            {
                if (!String.Equals(unit.Type,"scout") && !String.Equals(unit.Type, "builder") && unit.Health <= 0)
                    _unitsService.DeleteUnit(unit);
            }
        }

        #nullable enable
        private void Fire(Unit unit)
        {
            AttackByTime? attackByTime = GetAttackByTime(_attackDamages, unit.Type);

            if (attackByTime is null)
                return;

            List<Unit> sameLocationUnits = _unitsService.GetUnitsFromLocation(unit.GetUnitLocation());
            sameLocationUnits = PickByPriority(unit, sameLocationUnits);
                
            if (sameLocationUnits.Count > 0)
            {
                Unit unit2 = sameLocationUnits.First();
                ShipAttack(unit, unit2, attackByTime);
            }
        }
    }
}