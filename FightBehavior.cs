using UWE;
using System;
using UnityEngine;
using QModManager.API.ModLoading;
using HarmonyLib;
using Logger = QModManager.Utility.Logger;

namespace FightingReapers
{
	public class FightBehavior
	{

		public bool targetFound = false;
		public static FightBehavior main;

		public Creature holdingEnemy;
		public EnemyType holdingEnemyType;
		public Transform grabPoint;
		public float timeEnemyGrabbed;
		public Vector3 enemyInitialPosition;

		public static GameObject targetGO = null;
		public GameObject targetReaper;
        public ReaperLeviathan thisReaper;
        public bool isAttacking;

		public void Awake()
		{
			main = this;
		}

		public enum EnemyType
		{
			None,

			ReaperLeviathan,

			GhostLeviathan

		}


		
	}    

    [HarmonyPatch(typeof(Creature))]
    [HarmonyPatch("Start")]

    public class AddAttackReaperBehavior
    {
        public static void AddBehavior(Creature __instance)
        {
            bool isReaper = __instance.GetComponentInChildren<ReaperLeviathan>();
            if (isReaper)
            {
                __instance.gameObject.AddComponent<AttackReaper>();
            }
        }
    }

    [HarmonyPatch(typeof(ReaperMeleeAttack))]
	[HarmonyPatch("CanEat")]

	public class CanEatPatch
    {
		[HarmonyPrefix]
		public static void CanEatFix(ReaperMeleeAttack __instance, ref bool __result, ref BehaviourType behaviourType)
        {
            __result = behaviourType == BehaviourType.Shark || behaviourType == BehaviourType.MediumFish || behaviourType == BehaviourType.SmallFish || behaviourType == BehaviourType.Leviathan;

		}

    }

    [HarmonyPatch(typeof(MoveTowardsTarget))]
    [HarmonyPatch("UpdateCurrentTarget")]
    internal class MoveTowardsTargetPatch
    {
        
        [HarmonyPrefix]
        public static bool Prefix(MoveTowardsTarget __instance, ref IEcoTarget ___currentTarget)
        {
            // ensure this is Reaper
            bool isReaper = __instance.gameObject.GetComponentInParent<ReaperLeviathan>();
            var fb = FightBehavior.main;

            if (!isReaper)
            {

                return true;

            }
            if (fb.targetFound)
            {

                ___currentTarget = fb.targetReaper.gameObject.GetComponent<IEcoTarget>();
                
                Logger.Log(Logger.Level.Debug, $"Attacking enemy reaper");

            }
            else
            {
                ___currentTarget = null;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(AggressiveWhenSeeTarget))]
    [HarmonyPatch("ScanForAggressionTarget")]

    internal class MaximizeAggressionPatch
    {
        
        [HarmonyPostfix]
        public static void WhenFightingReaper(AggressiveWhenSeeTarget __instance)

        {
            bool isReaper = __instance.gameObject.GetComponentInParent<ReaperLeviathan>();
            var fb = FightBehavior.main;

            if (isReaper && fb.targetReaper !=null)
            {
                __instance.aggressionPerSecond = 20f;
            }

        }


    }

    [HarmonyPatch(typeof(ReaperLeviathan))]
    [HarmonyPatch("Update")]

    internal class StartFight
    {
        [HarmonyPostfix]
        public static void CloseIn(ReaperLeviathan __instance)

        {            
            var fb = FightBehavior.main;
            var ar = AttackReaper.main;
            
            float attackDist = Vector3.Distance(__instance.transform.forward, fb.targetReaper.transform.position);

            if (fb.targetReaper != null)
            {
                ar.Perform(__instance, 120f);                                                                      
                if (attackDist <= 50f)
                {
                    ar.StartPerform(__instance);
                    Logger.Log(Logger.Level.Debug, $"Charging!");
                }
            }            

        }


    }

    [HarmonyPatch(typeof(MeleeAttack))]
    [HarmonyPatch("CanBite", new Type[] { typeof(GameObject) })]

    internal class InflictBite
    {

        [HarmonyPrefix]
        public static bool BiteEnemy(MeleeAttack __instance, GameObject target, ref bool __result)

        {
            ReaperLeviathan component2 = target.GetComponent<ReaperLeviathan>();

            if (component2 != null)
            {
                __result = true;
                return false;
                
            }
            return true;
        }


    }

    [QModCore]
        public static class FightPatcher
        {         

            [QModPatch]
            public static void Patch()
            {                
                var harmony = new Harmony("com.falselight.fightingreapers");
                harmony.PatchAll();
            }

        }


}



