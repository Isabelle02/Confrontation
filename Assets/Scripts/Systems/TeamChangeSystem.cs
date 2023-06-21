using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using FuryLion;
using Interfaces;

namespace Systems
{
    public class TeamChangeSystem : BaseSystem<IRegion, IBuilding>
    {
        private List<IRegion> _regions = new List<IRegion>();
        private List<IBuilding> _buildings = new List<IBuilding>();

        public event Action<List<ICell>> ChangedTeamID;
        public event Action PassedLevel;
        public event Action LostLevel;

        protected override void AddActor(IBuilding actor)
        {
            if (actor is ISettlement)
                actor.ChangedTeamID += OnSettlementFall;
            
            if (actor is ICapital)
                actor.ChangedTeamID += OnCapitalFall;
            
            _buildings.Add(actor);
        }

        protected override void RemoveActor(IBuilding actor)
        {
            if (actor is ISettlement)
                actor.ChangedTeamID -= OnSettlementFall;
            
            if (actor is ICapital)
                actor.ChangedTeamID -= OnCapitalFall;
            
            _buildings.Remove(actor);
        }

        protected override void AddActor(IRegion warehouse)
        {
            _regions.Add(warehouse);
        }

        protected override void RemoveActor(IRegion actor)
        {
            _regions.Remove(actor);
        }

        private void OnSettlementFall(IBuilding building, int teamID, int newTeamID)
        {
            if (newTeamID == 1)
                Vibration.Vibrate(250);
            else if (teamID == 1)
                Vibration.Vibrate(500);
            
            foreach (var r in _regions)
                if (r.ContainsBuilding(building))
                {
                    r.ChangeTeamID(newTeamID);
                    ChangedTeamID?.Invoke(r.GetCells());
                    break;
                }
        }

        private void OnCapitalFall(IBuilding building, int teamID, int newTeamID)
        {
            var capitals = _buildings.FindAll(b => b.TeamID == teamID && b is ICapital).ToList();
            if (capitals.Count > 0)
                return;
            
            if (teamID != 0)
                SoundManager.PlaySound(Sounds.Music.CannonShot);
            
            foreach (var r in _regions)
                if (r.GetTeamID() == teamID)
                {
                    r.ChangeTeamID(0);
                    ChangedTeamID?.Invoke(r.GetCells());
                }

            UnitUtility.OnTeamKill(teamID);
            if (_regions.All(r => r.GetTeamID() == 0 || r.GetTeamID() == 1))
            {
                PassedLevel?.Invoke();
                Deactivate();
            }
            else if (_regions.All(r => r.GetTeamID() != 1))
            {
                LostLevel?.Invoke();
                Deactivate();
            }
        }

        private void Deactivate()
        {
            PassedLevel = null;
            LostLevel = null;
        }
    }
}
