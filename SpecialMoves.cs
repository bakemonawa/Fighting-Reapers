using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWE;
using UnityEngine;

namespace FightingReapers
{
    public class SpecialMoves
    {
		public static Creature creature = new Creature();
		public static float num = UnityEngine.Random.Range(0.0f, 1.0f);

		private void GrabEnemy(Leviathan target, FightingReapers.EnemyType type)
		{
			target.GetComponent<Rigidbody>().isKinematic = true;
			var fb = FightBehavior.main;
			
			fb.holdingEnemy = target;
			fb.holdingEnemyType = type;
		
			creature.Aggression.Value = 0f;
			fb.timeEnemyGrabbed = Time.time;			
			fb.enemyInitialPosition = target.transform.position;
			
			InvokeRepeating("DamageVehicle", 1f, 1f);
			ReaperLeviathan.Invoke("ReleaseVehicle", 8f + UnityEngine.Random.value * 5f);

			if (fb.holdingEnemy != null)
			{
				float num = Mathf.Clamp01(Time.time - fb.timeEnemyGrabbed);
				if (num >= 1f)
				{
					fb.holdingEnemy.transform.position = fb.grabPoint.position;					
					return;
				}
				fb.holdingEnemy.transform.position = (fb.grabPoint.position - fb.enemyInitialPosition) * num + fb.enemyInitialPosition;				
			}
		}

		public static void FightMoves()

        {
            //50/50 chance to play either of the vehicle grab animations on enemy leviathans.

            SafeAnimator.SetBool(creature.GetAnimator(), "seamoth_attack", num>1.0);
            SafeAnimator.SetBool(creature.GetAnimator(), "exo_attack", num<1.0);

        }
        

    }


}
