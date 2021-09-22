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
        
		public static FightBehavior main;

		public Creature holdingEnemy;
		public EnemyType holdingEnemyType;
		public Transform grabPoint;
		public float timeEnemyGrabbed;
		public Vector3 enemyInitialPosition;		
        public ReaperLeviathan thisReaper;
        public RaycastHit bitePoint;
        public RaycastHit clawPoint;
        public RaycastHit eyeHit;
        public Collider biteObject;
        public Collider clawObject;
        public GameObject targetReaper;
        public float targetDist;
        public float biteDist;
        public bool targetFound;
        public float nextNotif = 0.0f;
        public float notifRate = 4f;
        public float moveChance = UnityEngine.Random.Range(0.0f, 1.01f);
        public float attackChance = UnityEngine.Random.Range(0.0f, 1.01f);
        public float critChance = UnityEngine.Random.Range(0f, 1.0001f);
        public float nextMove = 0.0f;
        public float nextAttack = 0.0f;
        public float randomCooldown = UnityEngine.Random.Range(3f, 6f);
        public float attackCD = UnityEngine.Random.Range(1f, 4f);


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
            var rm =__instance.GetComponentInChildren<ReaperMeleeAttack>();           
            
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

    [HarmonyPatch(typeof(Creature), nameof(Creature.OnKill))]
    [HarmonyPatch("OnDestroy")]

    public class AnimatorKiller
    {
        [HarmonyPostfix]
        public static void StopMoving(Creature __instance)
        {
            bool isReaper = __instance.GetComponentInChildren<ReaperLeviathan>();

            if (isReaper)
            {

                SafeAnimator.SetBool(__instance.GetAnimator(), "attacking", false);
                this.animator.SetBool(MeleeAttack.biteAnimID, false);


                ErrorMessage.AddMessage($"ANIMATOR DEACTIVATED");
                Logger.Log(Logger.Level.Info, "ANIMATOR DEACTIVATED");
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
        private static float timeSnapAgain;
        private static float snapInterval = UnityEngine.Random.Range(0.5f, 2.3f);
        
        public static void Snap(Creature creature)
        {
            var rb = creature.GetComponentInParent<Rigidbody>();
            var rm = creature.GetComponentInParent<ReaperMeleeAttack>();

            if (Time.time > timeSnapAgain)
            {
                rb.AddForce(rm.mouth.transform.forward * 10f, ForceMode.VelocityChange);

                timeSnapAgain = Time.time + snapInterval;
            }
            
        }

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
            var liveMixin = __instance.GetComponentInParent<LiveMixin>();

            if (damageInfo.damage >= 80f)
            {
                //Look at damage dealer

                __instance.Aggression.Add(damageInfo.damage * 0.5f);
                __instance.Friendliness.Add(-aggro.friendlinessDecrement);
                __instance.Tired.Add(-aggro.tirednessDecrement);
                __instance.Happy.Add(-aggro.happinessDecrement);
                global::Utils.PlayEnvSound(rm.playerAttackSound, __instance.transform.forward, 40f);                
                swim.LookAt(damageInfo.dealer.transform);
                Logger.Log(Logger.Level.Debug, "NOTICEABLE DAMAGE!");
                ErrorMessage.AddMessage("NOTICEABLE DAMAGE!");
            }

            
            if (damageInfo.damage >= 140f)
            {
                //Reflexively snap at damage dealer 

                ar.DesignateTarget(damageInfo.dealer.transform);
                Snap(__instance);

                Logger.Log(Logger.Level.Debug, "REFLEXIVE SNAP!");
                ErrorMessage.AddMessage("REFLEXIVE SNAP!");
            }
            

            if (damageInfo.damage >= 1500f)
            {
                //Bleed profusely upon receiving excessive damage

                var startTime = DateTime.UtcNow;

                while (DateTime.UtcNow - startTime < TimeSpan.FromMinutes(0.5))
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
                
                Logger.Log(Logger.Level.Debug, "CRITICAL HIT!");
                ErrorMessage.AddMessage("CRITICAL HIT!");
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





