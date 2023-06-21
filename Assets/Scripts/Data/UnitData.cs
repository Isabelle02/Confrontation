using Core;
using Entities;
using UnityEngine;

namespace Data
{
    public class UnitData : ObjectData
    {
        public int TeamID;
        public float Force;
        public float DebuffProtection;
        public float Speed;
        public Vector3 Position;
        public Vector3 TargetPosition;

        public UnitData(int teamID, Vector3 pos, Vector3 targetPos)
        {
            TeamID = teamID;
            Position = pos;
            TargetPosition = targetPos;
            Speed = teamID == 1 ? LevelManager.PlayerData.BaseSpeed : AIAcademy.Speed;
            Force = teamID == 1 ? LevelManager.PlayerData.BaseForce : AIAcademy.Force;
            DebuffProtection = teamID == 1 ? LevelManager.PlayerData.BaseDebuffProtection : AIAcademy.DebuffProtection;
        }
        
        protected override BaseEntity CreateEntity(IWorld world)
        {
            return new UnitEntity(this, world);
        }
    }
}