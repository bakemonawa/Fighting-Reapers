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

            float minDist = Mathf.Infinity;
                        
            foreach (Creature rl in ReaperList)
            {
                
                float dist = Vector3.Distance(rl.transform.position, me.transform.position);
                if (dist > 0.05f && dist < minDist && rl.gameObject != null)
                {
                    minDist = dist;
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
            
            RaycastHit bitepoint;
            RaycastHit clawpoint;
            GameObject targetReaper;
            bool targetFound;            
            var fb = __instance.GetComponentInParent<FightBehavior>();
            var ar = __instance.GetComponentInParent<AttackReaper>();
            var bm = __instance.GetComponentInParent<BasicFightingMoves>();            
            var ra = __instance.GetComponentInParent<ReaperMeleeAttack>();
            var at = __instance.GetComponentInParent<AggressiveWhenSeeTarget>();
            var rb = __instance.GetComponentInParent<Rigidbody>();
            var lor = __instance.GetComponentInParent<ListOfReapers>();


            targetReaper = lor.FindNearbyReaper(__instance);

            // Detects objects in front of Reaper's mouth

            Physics.Raycast(ra.mouth.transform.position, __instance.transform.forward, out bitepoint, 1.5f);
            Physics.Raycast(ra.mouth.transform.position, __instance.transform.forward, out clawpoint, 3.5f);

            
            Collider biteObject = bitepoint.collider;
            Collider clawObject = clawpoint.collider;

            float nextFire = 0.0f;
            float fireRate = 4f;
            float moveChance = UnityEngine.Random.Range(0.0f, 1.0f);
            float attackChance = UnityEngine.Random.Range(0.0f, 1.0f);
            float targetDist = Vector3.Distance(__instance.transform.position, targetReaper.transform.position);
            float biteDist = Vector3.Distance(bitepoint.transform.position, biteObject.transform.position);



            bm.Tackle();
            //bm.OnTouch(biteObject);

            while (clawObject != null && biteDist <= 8f)

            {
                // 50 percent of the time, if the enemy is within twice the bite distance, the Reaper will do a pincer attack with its claws
                if (attackChance <= 0.5f)
                {
                    bm.Claw();
                    global::Utils.PlayEnvSound(ra.playerAttackSound, __instance.transform.forward, 35f);
                }
            }

            while (biteObject != null && biteDist <= 2f)
            {
                bm.Bite(biteObject);

            }

            if (targetReaper != null)

            {
                targetFound = true;
                __instance.Aggression.Add(15f);
                ar.DesignateTarget(targetReaper.transform); 
                at.sightedSound.PlayOneShotNoWorld(bm.mouth.transform.position, 50f);                
                ar.StartPerform(__instance);

                ErrorMessage.AddMessage($"ENEMY REAPER ACQUIRED");

                while (targetFound == true)
                {
                    __instance.Aggression.Add(Time.deltaTime * 0.3f);
                    ar.UpdateAttackPoint();
                    ar.Perform(__instance, 5f);
                    ErrorMessage.AddMessage($"CLOSING AT {rb.velocity.magnitude} M/S");

                    if (moveChance > 0.5f)
                    {
                        bm.Charge();
                    }
                }
                

                while (targetDist <= 50f)
                {
                    
                    bm.attackSound.PlayOneShotNoWorld(__instance.transform.position, 40f);

                    // 50 percent of the time, a Reaper will twist its body in order to fit its claws around the enemy Reaper's body
                    if (moveChance <= 0.5f)
                    {
                        bm.Twist();

                    }

                    // 50 percent of the time, a Reaper will reel back to prepare for a lunge

                    if (moveChance > 0.5f)
                    {
                        bm.Reel();
                    }
                    Logger.Log(Logger.Level.Debug, "Charging!");

                }

                // If the Reaper is within 30m of the enemy, it will lunge at it.

                while (biteDist < 50f)
                {
                    bm.Lunge();
                    
                }                

                while (targetFound && Time.time > nextFire)
                {
                    nextFire = Time.time + fireRate;
                    ErrorMessage.AddMessage($"ENEMY REAPER IS : {targetDist} AWAY FROM ME"); 
                }
            }           
            
            else 
            {
                ErrorMessage.AddMessage($"NO ENEMY REAPERS IN VICINITY");
            }
            
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


