using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWE;
using UnityEngine;
using HarmonyLib;
using Logger = QModManager.Utility.Logger;

namespace FightingReapers 
{
    public class ListOfReapers : MonoBehaviour
    {
        public static List<Creature> ReaperList = new List<Creature>();
        
        public GameObject FindNearbyReaper(ReaperLeviathan me)
        {

            float maxDist = 150f;
                        
            foreach (Creature rl in ReaperList)
            {
                
                float dist = Vector3.Distance(rl.transform.position, me.transform.position);
                if (dist > 0.05f && dist < maxDist && rl.gameObject != null)
                {
                    maxDist = dist;
                    ErrorMessage.AddMessage("CLOSEST REAPER ACQUIRED");
                    return rl.gameObject;                  
                                                                                                       
                }
            }
            return null;
        }

    }


    [HarmonyPatch(typeof(ReaperLeviathan))]
    [HarmonyPatch("Update")]
    internal class FightCheck
    {
        
        [HarmonyPostfix]
        public static void SeekEnemyReaper(ReaperLeviathan __instance)
        {
            

            var fb = __instance.GetComponentInParent<FightBehavior>();
            var ar = __instance.GetComponentInParent<AttackReaper>();
            var bm = __instance.GetComponentInParent<BasicFightingMoves>();            
            var ra = __instance.GetComponentInParent<ReaperMeleeAttack>();
            var at = __instance.GetComponentInParent<AggressiveWhenSeeTarget>();
            var rb = __instance.GetComponentInParent<Rigidbody>();
            var lor = __instance.GetComponentInParent<ListOfReapers>();            

            fb.targetReaper = lor.FindNearbyReaper(__instance);

            Physics.SphereCast(ra.mouth.transform.position + new Vector3 (0, 0.5f, 0), 1f, ra.mouth.transform.position + new Vector3(0, 0.5f, 1f), out fb.eyeHit, Mathf.Infinity);

            // Spherecast to detect biteable objects in front of the reaper's mouth

            Physics.SphereCast(ra.mouth.transform.position, 1f, __instance.transform.forward, out fb.bitePoint, 1f);

            // Spherecast to detect claw-able objects 2.5m in front of the mouth (estimated length of claws) and 3m in either direction (estimated span of claws)

            Physics.SphereCast(ra.mouth.transform.position + new Vector3 (-3f, 0, 2.5f), 2f, ra.mouth.transform.position + new Vector3(3f, 0, 2.5f), out fb.clawPoint, 6f);
            fb.targetDist = Vector3.Distance(__instance.transform.position, fb.targetReaper.transform.position);
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            

            Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 1");

            if (fb.bitePoint.collider != null)
            {
                fb.biteObject = fb.bitePoint.collider;
                fb.biteDist = Vector3.Distance(fb.bitePoint.transform.position, fb.biteObject.transform.position);
            }

            if (fb.clawPoint.collider != null)
            {
                fb.clawObject = fb.clawPoint.collider;
            }

            //bm.OnTouch(biteObject);

            Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 2");

            // 50 percent of the time, if the enemy is within twice the bite distance, the Reaper will do a pincer attack with its claws
            if (fb.clawObject != null && fb.attackChance <= 0.5f && fb.biteDist <= 8f && Time.time > fb.nextAttack)

            {
                                               
                bm.Claw();
                fb.nextAttack = Time.time + fb.attackCD;
                global::Utils.PlayEnvSound(ra.playerAttackSound, __instance.transform.forward, 35f);

            } 
            
            if (fb.biteObject != null && fb.biteDist <= 2f && Time.time > fb.nextAttack)
            {
                bm.Bite();
                fb.nextAttack = Time.time + fb.attackCD;
                global::Utils.PlayEnvSound(ra.playerAttackSound, __instance.transform.forward, 35f);
            }
            
            Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 3");

            if (fb.targetReaper != null)

            {
                
                fb.targetFound = true;
                __instance.Aggression.Add(UnityEngine.Random.Range(15f, 31f));
                ar.DesignateTarget(fb.targetReaper.transform);
                ar.StartPerform(__instance);
                ar.UpdateAttackPoint();


                if (fb.targetFound == true)
                {
                    __instance.Aggression.Add(Time.deltaTime * 0.3f);                                        
                    
                    ar.Approach();

                    if(__instance.Aggression.Value >= 30f && __instance.Tired.Value < 20f)
                    {
                        ar.Charge();
                        ErrorMessage.AddMessage("RAMMING SPEED");
                    }

                    Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 4");

                    /*

                    if (fb.targetDist <= 25f)
                    {                                                

                        // 50 percent of the time, a Reaper will twist its body in order to fit its claws around the enemy Reaper's body
                        if (fb.moveChance <= 0.90f && Time.time > fb.nextMove)
                        {
                            bm.Twist();
                            fb.nextMove = Time.time + fb.randomCooldown;

                        }

                        // 50 percent of the time, a Reaper will reel back to prepare for a lunge

                        if (fb.moveChance < 0.30f && Time.time > fb.nextMove)
                        {
                            bm.Reel();
                            fb.nextMove = Time.time + fb.randomCooldown;
                        }
                        Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 5");

                    }

                    */

                    // If the Reaper is within 30m of the enemy, it will lunge at it.

                    if (fb.targetDist < 30f && fb.moveChance >= 0.9f && Time.time > fb.nextMove)
                    {
                        bm.Lunge();
                        fb.nextMove = Time.time + fb.randomCooldown;
                        Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 6");

                    }

                    if (Time.time > fb.nextNotif)
                    {
                        fb.nextNotif = Time.time + fb.notifRate;
                        Logger.Log(Logger.Level.Debug, $"ENEMY REAPER IS : {fb.targetDist} AWAY FROM ME");
                    }
                }
            }
            else
            {
                                
                if (Time.time > fb.nextNotif)
                {
                    fb.nextNotif = Time.time + fb.notifRate;
                    Logger.Log(Logger.Level.Debug, $"NO ENEMY REAPERS IN VICINITY");
                    ErrorMessage.AddMessage($"NO ENEMY REAPERS IN VICINITY");
                }
                //ar.StopPerform(__instance);
                Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 7");
            }

            

            //bm.Tackle();

            Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 8");
        }

        


    }

    /*
    [HarmonyPatch(typeof(AggressiveWhenSeeTarget))]
    [HarmonyPatch("IsTargetValid", new Type[] { typeof(GameObject) })]

    internal class ValidTargetPatch
    {
        [HarmonyPrefix]
        public static void Prefix(AggressiveWhenSeeTarget __instance, GameObject target, ref bool __result)
        {

            bool isReaper = __instance.gameObject.GetComponentInParent<ReaperLeviathan>();

            if (isReaper)
            {

                __instance.ignoreSameKind = false;

                if (CraftData.GetTechType(target) == TechType.Reefback)
                {

                    __result = false;
                }
            }

            else
            {
                __instance.ignoreSameKind = true;
            }


        }

    } */

}


