using UWE;
using System;
using UnityEngine;
using QModManager.API.ModLoading;
using HarmonyLib;
using Logger = QModManager.Utility.Logger;

namespace FightingReapers
{
	public class FightBehavior : MonoBehaviour
	{

		public bool targetFound = false;
		public static FightBehavior main;

		public Creature holdingEnemy;
		public EnemyType holdingEnemyType;
		public Transform grabPoint;
		public float timeEnemyGrabbed;
		public Vector3 enemyInitialPosition;

		public GameObject biteObject;
		public GameObject targetReaper;
        public ReaperLeviathan thisReaper;
        public bool isAttacking;
        public float targetDist;
        public bool tookCrit;
        public RaycastHit bitepoint;


        private void Awake()
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
        [HarmonyPostfix]
        public static void AddBehavior(Creature __instance)
        {
            bool isReaper = __instance.GetComponentInChildren<ReaperLeviathan>();
            
            var reaper = __instance.GetComponentInChildren<MeleeAttack>();
            var rm =__instance.GetComponentInChildren<ReaperMeleeAttack>();
            var aggro = __instance.GetComponentInChildren<AggressiveWhenSeeTarget>();
            var aggro2 = __instance.GetComponentInParent<AggressiveWhenSeeTarget>();
            

            if (isReaper)
            {
                __instance.gameObject.AddComponent<FightBehavior>();                
                __instance.gameObject.AddComponent<AttackReaper>();                
                __instance.gameObject.AddComponent<BasicFightingMoves>();
                __instance.gameObject.AddComponent<ListOfReapers>();
                __instance.gameObject.EnsureComponent<FMOD_StudioEventEmitter>();
                __instance.gameObject.EnsureComponent<FMOD_CustomEmitter>();
                __instance.gameObject.EnsureComponent<LiveMixin>();
                __instance.gameObject.EnsureComponent<VFXSurface>();
                __instance.gameObject.EnsureComponent<AggressiveWhenSeeTarget>();
                __instance.gameObject.EnsureComponent<AggressiveOnDamage>();

                ListOfReapers.ReaperList.Add(__instance);

                var bm = __instance.GetComponent<BasicFightingMoves>();
                var liveMixin = __instance.GetComponentInParent<LiveMixin>();

                SafeAnimator.SetBool(__instance.GetAnimator(), "attacking", true);
                global::Utils.PlayEnvSound(rm.playerAttackSound, __instance.transform.forward, 20f);               
                                
                UnityEngine.Object.Instantiate(liveMixin.damageEffect, __instance.transform.position, Quaternion.identity);
                UnityEngine.Object.Instantiate(liveMixin.damageEffect, __instance.transform.position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                UnityEngine.Object.Instantiate(liveMixin.damageEffect, __instance.transform.position + new Vector3(-0.5f, 0.5f, 0), Quaternion.identity);
                UnityEngine.Object.Instantiate(liveMixin.damageEffect, __instance.transform.position + new Vector3(0.5f, -0.5f, 0), Quaternion.identity);
                ErrorMessage.AddMessage($"REAPER SPAWNED");
                Logger.Log(Logger.Level.Info, "REAPER SPAWNED");
            }
        }
    }

    [HarmonyPatch(typeof(Creature))]
    [HarmonyPatch("OnDestroy")]

    public class Unlister
    {
        [HarmonyPostfix]
        public static void UnlistReaper(Creature __instance)
        {
            bool isReaper = __instance.GetComponentInChildren<ReaperLeviathan>();

            if (isReaper)
            {               

                ListOfReapers.ReaperList.Remove(__instance);

                
                ErrorMessage.AddMessage($"REAPER DESTROYED");
                Logger.Log(Logger.Level.Info, "REAPER DESTROYED");
            }
        }
    }



    /*

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

    */

    [HarmonyPatch(typeof(ReaperLeviathan))]
    [HarmonyPatch("OnTakeDamage", new Type[] { typeof(DamageInfo) })]

    public class OnTakeDamagePatch

    {
        private static float timeBleedAgain;
        private static float bleedInterval = 0.7f;       

        [HarmonyPostfix]
        public static void ReactToDamage(ReaperLeviathan __instance, DamageInfo damageInfo)
        {
            var fb = __instance.GetComponentInParent<FightBehavior>();
            var ar = __instance.GetComponentInParent<AttackReaper>();
            var reaperBody = __instance.GetComponentInParent<Rigidbody>();
            var creature = __instance.GetComponentInParent<Creature>();
            var aggro = __instance.GetComponentInParent<AggressiveOnDamage>();
            var swim = __instance.GetComponentInParent<SwimBehaviour>();
            var swimRandom = __instance.GetComponentInParent<SwimRandom>();
            var bm = __instance.GetComponentInParent<BasicFightingMoves>();
            var melee = __instance.GetComponentInParent<MeleeAttack>();
            var aggro2 = __instance.GetComponentInParent<AggressiveWhenSeeTarget>();
            var rm = __instance.GetComponentInChildren<ReaperMeleeAttack>();

            LiveMixin liveMixin = __instance.GetComponentInParent<LiveMixin>();

            if (damageInfo.damage >= 80f)
            {
                //Look at damage dealer

                __instance.Aggression.Add(damageInfo.damage * 0.5f);
                __instance.Friendliness.Add(-aggro.friendlinessDecrement);
                __instance.Tired.Add(-aggro.tirednessDecrement);
                __instance.Happy.Add(-aggro.happinessDecrement);
                global::Utils.PlayEnvSound(rm.playerAttackSound, __instance.transform.forward, 40f);
                swim.LookAt(damageInfo.dealer.transform);                
            }

            if (damageInfo.damage >= 140f)
            {
                //Reflexively snap at damage dealer 

                reaperBody.AddForce(damageInfo.dealer.transform.position * 160f, ForceMode.VelocityChange);
                ar.DesignateTarget(damageInfo.dealer.transform);

            }            

            if (damageInfo.damage >= 1500f)
            {
                //Bleed profusely upon receiving excessive damage

                var startTime = DateTime.UtcNow;

                while (DateTime.UtcNow - startTime < TimeSpan.FromMinutes(1))
                {
                    if (Time.time >= timeBleedAgain)
                    {
                        Vector3 position = __instance.transform.position;           //__instance.transform.InverseTransformPoint(damageInfo.position);
                        UnityEngine.Object.Instantiate<GameObject>(liveMixin.damageEffect, position, Quaternion.identity);
                        UnityEngine.Object.Instantiate<GameObject>(liveMixin.damageEffect, position + new Vector3(0, 0.5f, 0.5f), Quaternion.identity);
                        UnityEngine.Object.Instantiate<GameObject>(liveMixin.damageEffect, position + new Vector3(0, -0.5f, 0.5f), Quaternion.identity);
                        UnityEngine.Object.Instantiate<GameObject>(liveMixin.damageEffect, position + new Vector3(0, 0.5f, -0.5f), Quaternion.identity);
                        UnityEngine.Object.Instantiate<GameObject>(liveMixin.damageEffect, position + new Vector3(0, -0.5f, -0.5f), Quaternion.identity);


                        timeBleedAgain = Time.time + bleedInterval;

                    }

                }
                
                Logger.Log(Logger.Level.Debug, "Critical hit!");
            }

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





