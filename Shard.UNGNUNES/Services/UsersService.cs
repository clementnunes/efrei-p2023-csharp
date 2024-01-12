using System;
using System.Collections.Generic;
using System.Linq;
using Shard.Shared.Core;
using Shard.UNGNUNES.Entities;

namespace Shard.UNGNUNES.Services
{
    public class UsersService
    {
        public List<User> UsersList {get; }
        
        public UsersService()
        {
            UsersList = new List<User>();
        }
        
        public User CreateUser(string id, string pseudo, String? date)
        {
            User createdUser;
            if (date is null)
                createdUser = new User(id, pseudo);
            else createdUser = new User(id, pseudo, DateTime.Parse(date));
            
            UsersList.Add(createdUser);
            return createdUser;
        }

        public User SearchUser(string id)
        {
            return UsersList.FirstOrDefault(user => String.Equals(id, user.Id));
        }

        public void IncreaseResource(User user, ResourceKind resourceKind)
        {
            if (user.ResourcesQuantity.ContainsKey(resourceKind.ToString().ToLower()))
            {
                user.ResourcesQuantity[resourceKind.ToString().ToLower()] += 1;
            }
        }
        
        public bool HasEnoughResource(User user, ResourceKind resourceKind, int quantity)
        {
            return (user.ResourcesQuantity.ContainsKey(resourceKind.ToString().ToLower())
                    && user.ResourcesQuantity[resourceKind.ToString().ToLower()] >= quantity);
        }
        public bool HasEnoughResources(User user, Dictionary<ResourceKind, int> resourcesQuantities)
        {
            foreach (var resourcesQuantity in resourcesQuantities)
            {
                if (HasEnoughResource(user, resourcesQuantity.Key, resourcesQuantity.Value) is false)
                    return false;
            }

            return true;
        }
        
        public bool DecreaseResource(User user, ResourceKind resourceKind, int quantity)
        {
            if (!HasEnoughResource(user, resourceKind, quantity))
                return false;
            
            user.ResourcesQuantity[resourceKind.ToString().ToLower()] -= quantity;
            return true;
        }
        
        public void IncreaseResource(User user, ResourceKind resourceKind, int quantity)
        {
            if (user.ResourcesQuantity.ContainsKey(resourceKind.ToString().ToLower()))
                user.ResourcesQuantity[resourceKind.ToString().ToLower()] += quantity;
            else user.ResourcesQuantity.Add(resourceKind.ToString().ToLower(),quantity);
        }

        public void SetResource(User user, ResourceKind resourceKind, int quantity)
        {
            var index = user.ResourcesQuantity[resourceKind.ToString().ToLower()] = quantity;
        }

        public void ClearResources(User user)
        {
            foreach (KeyValuePair<string,int> resource in user.ResourcesQuantity)
            {
                user.ResourcesQuantity[resource.Key] = 0;
            }
                
        }
    }
}