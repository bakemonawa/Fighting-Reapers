using UWE;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public RaycastHit bitePoint;
        public RaycastHit clawPoint;        
        public Collider biteObject;
        public Collider clawObject;
        public GameObject targetObj;
        public GameObject targetReaper;
        public float targetDist;
        public float biteDist;
        public bool targetFound;
        public float nextNotif = 0.0f;
        public float notifRate = 4f;
        public float moveChance;
        public float attackChance;
        public float critChance;
        public float nextMove = 0.0f;
        public float nextAttack = 0.0f;
        public float randomCooldown;
        public float attackCD;
        public GameObject bloodPrefab;
        internal GameObject CachedBloodPrefab;
        public float lifetimeScale = 2f;
        public float startSizeScale = 12f;
        public Transform mouth;
        public Transform LTMandible;
        public Transform RTMandible;
        public Transform LBMandible;
        public Transform RBMandible;

        public SphereCollider mouthCol;
        public SphereCollider ltm;
        public SphereCollider rtm;
        public SphereCollider lbm;
        public SphereCollider rbm;

        public Rigidbody mouthRB;
        public Rigidbody ltmRB;
        public Rigidbody rtmRB;
        public Rigidbody lbmRB;
        public Rigidbody rbmRB;

        public Collider clawable;

        public Dictionary<GameObject, Collider> clawObjects = new Dictionary<GameObject, Collider>();
        public List<Collider> biteObjects = new List<Collider>();

        private void Awake()
        {
            main = this;
        }

        public void FixedUpdate()

        {
            moveChance = UnityEngine.Random.Range(0.0f, 1.01f);
            attackChance = UnityEngine.Random.Range(0.0f, 1.01f);
            critChance = UnityEngine.Random.Range(0f, 1.0001f);        
            randomCooldown = UnityEngine.Random.Range(3f, 6f);
            attackCD = UnityEngine.Random.Range(1f, 4f);

        }


        public enum EnemyType
        {
            None,

            ReaperLeviathan,

            GhostLeviathan

        }

        public void BloodGen(Collider target)
        {
            if (target == null)
            {
                return;
            }
            LiveMixin lm = target.GetComponentInParent<LiveMixin>();
            if (lm == null)
            {
                return;
            }
            if (lm.data == null)
            {
                return;
            }
            bloodPrefab = lm.data.damageEffect;

            if (bloodPrefab == null)
            {
                return;
            }

            CachedBloodPrefab = Instantiate(bloodPrefab);
            Logger.Log(Logger.Level.Debug, "Blood generated!");
            CachedBloodPrefab.SetActive(false);
            foreach (ParticleSystem ps in CachedBloodPrefab.GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(main.startLifetime.constant * lifetimeScale);
                main.startSize = new ParticleSystem.MinMaxCurve(main.startSize.constant * startSizeScale);
            }
            VFXDestroyAfterSeconds destroyAfterSeconds = CachedBloodPrefab.GetComponent<VFXDestroyAfterSeconds>();
            DestroyImmediate(destroyAfterSeconds);
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
            bool isGhost = __instance.GetComponentInChildren<GhostLeviathan>();
            bool isDragon = __instance.GetComponentInChildren<SeaDragon>();
            var rm = __instance.GetComponentInChildren<ReaperMeleeAttack>();

            if (isReaper)
            {
                __instance.gameObject.AddComponent<FightBehavior>();
                __instance.gameObject.AddComponent<AttackReaper>();
                __instance.gameObject.AddComponent<BasicFightingMoves>();                
                __instance.gameObject.EnsureComponent<LiveMixin>();
                __instance.gameObject.EnsureComponent<VFXSurface>();
                __instance.gameObject.EnsureComponent<AggressiveWhenSeeTarget>();
                __instance.gameObject.EnsureComponent<AggressiveOnDamage>();
                //__instance.gameObject.EnsureComponent<NibbleMeat>();
                //__instance.gameObject.AddComponent<SwimToMeat>();
                __instance.gameObject.AddComponent<ListOfLeviathans>();

                var fb =__instance.gameObject.GetComponent<FightBehavior>();
                ListOfLeviathans.LeviathanList.Add(__instance);

                var baseCol = __instance.GetComponentInChildren<SphereCollider>();
                baseCol.radius = 0.80f * baseCol.radius;                
                baseCol.center += -2 * baseCol.transform.forward;

                fb.mouth = __instance.transform.Find("reaper_leviathan.root/neck/head/mouth_damage_trigger");
                fb.LBMandible = __instance.transform.Find("reaper_leviathan/root/neck/head/LB_mandable");
                fb.RBMandible = __instance.transform.Find("reaper_leviathan/root/neck/head/LB_mandable6");
                fb.LTMandible = __instance.transform.Find("reaper_leviathan/root/neck/head/LT_mandable");
                fb.RTMandible = __instance.transform.Find("reaper_leviathan/root/neck/head/RT_mandable");

                Logger.Log(Logger.Level.Info, "START CHECK 1 PASSED");

                fb.mouthCol = fb.mouth.gameObject.AddComponent<SphereCollider>();
                fb.mouthCol.radius = 1f;
                fb.mouthCol.center = fb.mouth.transform.forward;
                fb.lbm = fb.LBMandible.gameObject.AddComponent<SphereCollider>();
                fb.lbm.radius = 0.5f;
                fb.lbm.center += 2 * fb.LBMandible.transform.forward;
                fb.rbm = fb.RBMandible.gameObject.AddComponent<SphereCollider>();
                fb.rbm.radius = 0.5f;
                fb.rbm.center += 2 * fb.RBMandible.transform.forward;
                fb.ltm = fb.LTMandible.gameObject.AddComponent<SphereCollider>();
                fb.ltm.radius = 0.5f;
                fb.ltm.center += 2 * fb.LTMandible.transform.forward;
                fb.rtm = fb.RTMandible.gameObject.AddComponent<SphereCollider>();
                fb.rtm.radius = 0.5f;
                fb.rtm.center += 2 * fb.RTMandible.transform.forward;

                Logger.Log(Logger.Level.Info, "START CHECK 2 PASSED");

                fb.mouthRB = fb.mouth.gameObject.AddComponent<Rigidbody>();
                fb.lbmRB = fb.LBMandible.gameObject.AddComponent<Rigidbody>();
                fb.rbmRB = fb.RBMandible.gameObject.AddComponent<Rigidbody>();
                fb.ltmRB = fb.LTMandible.gameObject.AddComponent<Rigidbody>();
                fb.rtmRB = fb.RTMandible.gameObject.AddComponent<Rigidbody>();

                Logger.Log(Logger.Level.Info, "START CHECK 3 PASSED");

                fb.mouth.gameObject.AddComponent<MouthTriggerController>();
                fb.LBMandible.gameObject.AddComponent<TriggerController>();
                fb.RBMandible.gameObject.AddComponent<TriggerController>();
                fb.LTMandible.gameObject.AddComponent<TriggerController>();
                fb.RTMandible.gameObject.AddComponent<TriggerController>();

                Logger.Log(Logger.Level.Info, "START CHECK 4 PASSED");

                fb.mouthRB.isKinematic = true;
                fb.lbmRB.isKinematic = true;
                fb.rbmRB.isKinematic = true;
                fb.ltmRB.isKinematic = true;
                fb.rtmRB.isKinematic = true;

                Logger.Log(Logger.Level.Info, "START CHECK 5 PASSED");

                fb.mouthCol.isTrigger = true;
                fb.lbm.isTrigger = true;
                fb.rbm.isTrigger = true;
                fb.ltm.isTrigger = true;
                fb.rtm.isTrigger = true;

                Logger.Log(Logger.Level.Info, "START CHECK 6 PASSED, REAPER SPAWNED");

                /*
                var liveMixin = __instance.GetComponentInParent<LiveMixin>();

                SafeAnimator.SetBool(__instance.GetAnimator(), "attacking", true);
                global::Utils.PlayEnvSound(rm.playerAttackSound, __instance.transform.forward, 20f);

                UnityEngine.Object.Instantiate(liveMixin.damageEffect, __instance.transform.position, Quaternion.identity);
                */

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
                ListOfLeviathans.LeviathanList.Remove(__instance);
                ErrorMessage.AddMessage($"REAPER DESTROYED");
                Logger.Log(Logger.Level.Info, "REAPER DESTROYED");
            }
        }
    }

    [HarmonyPatch(typeof(Creature), nameof(Creature.OnKill))]

    public class AnimatorKiller
    {
        

        [HarmonyPostfix]
        public static void StopMoving(Creature __instance)
        {
            bool isReaper = __instance.GetComponentInChildren<ReaperLeviathan>();

            IEnumerator DeathThroes(Creature creature)
            {

                var animator = creature.GetComponentInChildren<Animator>();
                animator.enabled = false;
                yield return new WaitForSeconds(0.5f);
                animator.enabled = true;
                yield return new WaitForSeconds(0.5f);
                animator.enabled = false;
                yield return new WaitForSeconds(0.5f);
                animator.enabled = true;

            }

            if (isReaper)
            {
                ListOfLeviathans.LeviathanList.Remove(__instance);
                var rm = __instance.GetComponentInChildren<ReaperMeleeAttack>();
                SafeAnimator.SetBool(__instance.GetAnimator(), "attacking", false);
                rm.animator.SetBool(MeleeAttack.biteAnimID, false);

                CoroutineHost.StartCoroutine(DeathThroes(__instance));

                ErrorMessage.AddMessage($"ANIMATOR DEACTIVATED");
                Logger.Log(Logger.Level.Info, "ANIMATOR DEACTIVATED");
            }
        }
    }

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
                timeSnapAgain = Time.time + snapInterval;
                rb.AddForce(rm.mouth.transform.forward * 10f, ForceMode.VelocityChange);
            }

        }

        [HarmonyPostfix]
        public static void ReactToDamage(ReaperLeviathan __instance, DamageInfo damageInfo)
        {
            var fb = __instance.GetComponentInParent<FightBehavior>();
            var collider = __instance.GetComponentInParent<Collider>();
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

            if (damageInfo.damage >= 60f)
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


            if (damageInfo.damage >= 80f)
            {
                //Reflexively snap at damage dealer 

                ar.DesignateTarget(damageInfo.dealer.transform);
                Snap(__instance);

                Logger.Log(Logger.Level.Debug, "REFLEXIVE SNAP!");
                ErrorMessage.AddMessage("REFLEXIVE SNAP!");
            }


            if (damageInfo.damage >= 250f)
            {
                //Bleed profusely upon receiving excessive damage

                IEnumerator CritBleeding()
                {
                    var startTime = DateTime.UtcNow;

                    while (DateTime.UtcNow - startTime < TimeSpan.FromMinutes(0.5))
                    {                     
                        if (Time.time >= timeBleedAgain)
                        {
                            Vector3 position = __instance.transform.InverseTransformPoint(damageInfo.position);
                            fb.BloodGen(collider);
                            GameObject blood = UnityEngine.Object.Instantiate(fb.CachedBloodPrefab, position, Quaternion.identity);
                            blood.SetActive(true);
                            UnityEngine.Object.Destroy(blood, 4f);
                        }
                        yield return null;
                    }
                }

                CoroutineHost.StartCoroutine(CritBleeding());

                Logger.Log(Logger.Level.Debug, "CRITICAL HIT!");
                ErrorMessage.AddMessage("CRITICAL HIT!");
            }

            //TO DO: WRITE A STAGGERING/WEAKENED STATE 

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





