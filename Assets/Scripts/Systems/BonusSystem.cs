using System.Collections.Generic;
using Core;
using Interfaces;

namespace Systems
{
    public class BonusSystem : BaseSystem<IFarm, IForge, IStable, IBonusDependent, IQuarry, IWorkshop>
    {
        private readonly List<IBuilding> _bonusGivers = new List<IBuilding>();
        private readonly List<IBonusDependent> _bonusDependents = new List<IBonusDependent>();

        protected override void AddActor(IBonusDependent bonusDependent)
        {
            UpdateBonus(bonusDependent);
            if (bonusDependent is IBuilding b)
            {
                b.ChangedLevel += UpdateBonus;
                b.ChangedTeamID += OnChangedTeamID;
            }

            _bonusDependents.Add(bonusDependent);
        }

        protected override void AddActor(IFarm warehouse)
        {
            UpdateBonus(warehouse, warehouse.TeamID);

            warehouse.ChangedLevel += UpdateBonus;
            warehouse.ChangedTeamID += OnChangedTeamID;
            
            _bonusGivers.Add(warehouse);
        }

        protected override void AddActor(IStable warehouse)
        {
            UpdateBonus(warehouse, warehouse.TeamID);

            warehouse.ChangedLevel += UpdateBonus;
            warehouse.ChangedTeamID += OnChangedTeamID;
            
            _bonusGivers.Add(warehouse);
        }

        protected override void AddActor(IForge warehouse)
        {
            UpdateBonus(warehouse, warehouse.TeamID);

            warehouse.ChangedLevel += UpdateBonus;
            warehouse.ChangedTeamID += OnChangedTeamID;
            
            _bonusGivers.Add(warehouse);
        }

        protected override void AddActor(IQuarry warehouse)
        {
            UpdateBonus(warehouse, warehouse.TeamID);

            warehouse.ChangedLevel += UpdateBonus;
            warehouse.ChangedTeamID += OnChangedTeamID;

            _bonusGivers.Add(warehouse);
        }

        protected override void AddActor(IWorkshop warehouse)
        {
            UpdateBonus(warehouse, warehouse.TeamID);

            warehouse.ChangedLevel += UpdateBonus;
            warehouse.ChangedTeamID += OnChangedTeamID;

            _bonusGivers.Add(warehouse);
        }

        protected override void RemoveActor(IBonusDependent bonusDependent)
        {
            if (bonusDependent is IBuilding b)
            {
                b.ChangedLevel -= UpdateBonus;
                b.ChangedTeamID -= OnChangedTeamID;
            }
            
            _bonusDependents.Remove(bonusDependent);
        }

        protected override void RemoveActor(IFarm actor)
        {
            actor.ChangedLevel -= UpdateBonus;
            actor.ChangedTeamID -= OnChangedTeamID;
            _bonusGivers.Remove(actor);
            UpdateBonus(actor, actor.TeamID);
        }

        protected override void RemoveActor(IStable actor)
        {
            actor.ChangedLevel -= UpdateBonus;
            actor.ChangedTeamID -= OnChangedTeamID;
            _bonusGivers.Remove(actor);
            UpdateBonus(actor, actor.TeamID);
        }
    
        protected override void RemoveActor(IForge actor)
        {
            actor.ChangedLevel -= UpdateBonus;
            actor.ChangedTeamID -= OnChangedTeamID;
            _bonusGivers.Remove(actor);
            UpdateBonus(actor, actor.TeamID);
        }
    
        protected override void RemoveActor(IQuarry actor)
        {
            actor.ChangedLevel -= UpdateBonus;
            actor.ChangedTeamID -= OnChangedTeamID;
            _bonusGivers.Remove(actor);
            UpdateBonus(actor, actor.TeamID);
        }

        protected override void RemoveActor(IWorkshop actor)
        {
            actor.ChangedLevel -= UpdateBonus;
            actor.ChangedTeamID -= OnChangedTeamID;
            _bonusGivers.Remove(actor);
            UpdateBonus(actor, actor.TeamID);
        }

        private void UpdateBonus(IBuilding building, int teamID)
        {
            foreach (var bd in _bonusDependents)
            {
                if (bd.TeamID != teamID)
                    continue;
                
                UpdateBonus(bd);
            }
        }

        private void UpdateBonus(IBonusDependent bd)
        {
            bd.Dispose();
            foreach (var bg in _bonusGivers)
            {
                if (bg.TeamID != bd.TeamID)
                    continue;
                    
                switch (bg)
                {
                    case IFarm farm:
                        bd.AddReproductionBonus(farm.GetReproductionBonus(farm.Level));
                        break;
                    case IStable stable:
                        bd.AddSpeedBonus(stable.GetSpeedBonus(stable.Level));
                        break;
                    case IForge forge:
                        bd.AddForceBonus(forge.GetForceBonus(forge.Level));
                        break;
                    case IQuarry quarry:
                        bd.AddProtectionBonus(quarry.GetProtectionBonus(quarry.Level));
                        break;
                    case IWorkshop workshop:
                        bd.AddDebuffProtectionBonus(workshop.GetDebuffProtectionBonus(workshop.Level));
                        break;
                }
            }
        }

        private void OnChangedTeamID(IBuilding building, int id, int newID)
        {
            UpdateBonus(building, id);
            UpdateBonus(building, newID);
        }
    }
}