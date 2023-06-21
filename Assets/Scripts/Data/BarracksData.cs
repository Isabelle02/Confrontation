﻿using System;
using Core;
using Entities;

namespace Data
{
    [Serializable]
    public class BarracksData : BuildingData
    {
        public int ArmyCount = 0;

        protected override BaseEntity CreateEntity(IWorld world)
        {
            return new BarracksEntity(this);
        }
    }
}