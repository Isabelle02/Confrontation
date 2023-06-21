﻿using System.Collections.Generic;
using Core;
using Data;
using Entities;
using Interfaces;
using UnityEngine;

namespace Systems
{
    public class CurrencySystem : BaseSystem<IBankable>
    {
        private List<IBankable> _bankables = new List<IBankable>();
        private readonly List<CustomerController> _customers;

        public CurrencySystem(List<CustomerController> customers)
        {
            _customers = customers;
        }

        protected override void AddActor(IBankable warehouse)
        {
            warehouse.Updated += OnUpdate;
            _bankables.Add(warehouse);
        }

        protected override void RemoveActor(IBankable actor)
        {
            actor.Updated -= OnUpdate;
            _bankables.Remove(actor);
        }

        private void OnUpdate(IBankable bankable)
        {
            foreach (var c in _customers)
            {
                if (bankable.TeamID == c.TeamID)
                {
                    switch (bankable)
                    {
                        case MineEntity mine:
                            c.Money += bankable.GetCurrency(mine.Level);
                            break;
                        case CapitalEntity capital:
                            c.Money += bankable.GetCurrency(capital.Level);
                            break;
                        case WizardTowerEntity wizardTower:
                            c.Mana += bankable.GetCurrency(wizardTower.Level);
                            break;
                    }
                }
            }
        }
    }
}