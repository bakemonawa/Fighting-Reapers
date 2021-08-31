using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;
using HarmonyLib;
using Logger = QModManager.Utility.Logger;

namespace FightingReapers
{
    public class BasicFightingMoves        
    {
        public static FightBehavior fb = FightBehavior.main;
        public static BasicFightingMoves main;
        private readonly CreatureAction creatureAction;

        public void Awake()
        {
            main = this;
        }
        public void Bite()
        {
            var reaperMeleeAttack = new ReaperMeleeAttack();
            
            if (fb.targetReaper != null)
            {
                creatureAction.swimBehaviour.SwimTo(fb.targetReaper.transform.position, 20f);
                reaperMeleeAttack.playerAttackSound.Play();
            }
        }
    }
}
