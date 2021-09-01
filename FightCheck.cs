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
    [HarmonyPatch(typeof(ReaperLeviathan))]
    [HarmonyPatch("Update")]
    internal class FightCheck
    {        

        [HarmonyPostfix]
        public static void SeekEnemyReaper(ReaperLeviathan __instance)
        {
            RaycastHit hit;
            var fb = FightBehavior.main;
            var ar = __instance.GetComponent<AttackReaper>();
            
            fb.thisReaper = __instance;
            
            Physics.SphereCast(__instance.transform.position, 100f, __instance.transform.forward, out hit, 10);                      

            float dist = Vector3.Distance(__instance.transform.position, fb.targetReaper.transform.position);
            float nextFire = 0.0f;
            float fireRate = 4f;
            

            if (hit.transform.gameObject !=null && CraftData.GetTechType(hit.transform.gameObject) == TechType.ReaperLeviathan)
            {
                fb.targetReaper = hit.transform.gameObject;
            }

            else

            {
                fb.targetReaper = null;
            }

            if (fb.targetReaper != null)

            {
                fb.targetFound = true;
                ar.SetCurrentTarget(fb.targetReaper, false); //Could set isDecoy to true to guarantee target lock?

                ar.Perform(__instance, 120f);
                if (dist <= 50f)
                {
                    ar.StartPerform(__instance);
                    Logger.Log(Logger.Level.Debug, $"Charging!");
                }

                while (fb.targetFound && Time.time > nextFire)
                {
                    nextFire = Time.time + fireRate;
                    Logger.Log(Logger.Level.Debug, $"Enemy reaper is: {dist} away from me");
                }
            }

            else if (fb.targetReaper = null)

            {
                fb.targetFound = false;
            }
            Collision collision = null;
            ar.OnCollisionEnter(collision);
            
        }


    }

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


        }

    }


    [HarmonyPatch(typeof(AggressiveWhenSeeTarget))]
    [HarmonyPatch("GetAggressionTarget")]
    internal class ReaperAggressionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(AggressiveWhenSeeTarget __instance, ref GameObject __result)
        {
            var fb = FightBehavior.main;

            // Attack other reaper 

            bool isReaper = __instance.gameObject.GetComponentInParent<ReaperLeviathan>();

            if (!isReaper)
            {

                return true;

            }

            if (fb.targetFound)

            {
                __result = fb.targetReaper.gameObject;
                Logger.Log(Logger.Level.Debug, $"Acquired enemy reaper");

            }

            else if (!fb.targetFound)
            {
                __result = null;
                return true;
            }
            return false;
        }
    }
}


