using System;
using Core;
using Data;
using Interfaces;
using UnityEngine;

namespace Entities
{
    public abstract class BuildingEntity<T> : BaseEntity<T>, IBuilding where T : BuildingData
    {
        public Vector3 Position
        {
            get => Data.Position; 
            set => Data.Position = value;
        }
        
        public event Action<IBuilding, int> ChangedLevel;
        public event Action<IBuilding, int, int> ChangedTeamID;
        
        public int Level
        {
            get => Data.Level;
            set
            {
                Data.Level = value;
                OnChangeLevel(value);
                ChangedLevel?.Invoke(this, TeamID);
            }
        }
        
        public int TeamID
        {
            get => Data.TeamID;
            set
            {
                var teamID = Data.TeamID;
                Data.TeamID = value;
                if (value != teamID)
                    ChangedTeamID?.Invoke(this, teamID, value);
            }
        }

        protected virtual void OnChangeLevel(int lvl) {}
        
        protected BuildingEntity(T data) : base(data)
        {
        }
    }
}
