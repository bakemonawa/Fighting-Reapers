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
    public class AttackReaper : CreatureAction
    {
		public LastTarget lastTarget;
		private float maxDistToLeash = 100f;
		public float swimVelocity = 10f;		
		private float swimInterval = 0.3f;
		public float timeLastAttack;
		public CreatureTrait aggressiveToNoise;
		private bool isActive;
		internal GameObject currentTarget;		
		private bool currentTargetIsDecoy;
		private Vector3 targetAttackPoint;
		public float scratchTimer = 0f;
		public bool startTimer = false;
		


		public static AttackReaper main;
		
		public override void Awake()
        {
			base.Awake();
			main = this;

        }

		public void DesignateTarget(Transform transform)

        {
			var fb = creature.GetComponent<FightBehavior>();
			var swim = creature.GetComponent<SwimBehaviour>();
			
			this.currentTarget = transform.gameObject;
			
            		

			Logger.Log(Logger.Level.Debug, "Hostile detected");

        }

		public override void StartPerform(Creature creature)
		{
			var fb = creature.GetComponent<FightBehavior>();

			SafeAnimator.SetBool(creature.GetAnimator(), "attacking", true);
			
			//this.lastTarget.SetLockedTarget(this.currentTarget);
			//this.isActive = true;

			Logger.Log(Logger.Level.Debug, "Acquiring!");
										
		}

		public override void StopPerform(Creature creature)
		{
			SafeAnimator.SetBool(creature.GetAnimator(), "attacking", false);
			/*this.lastTarget.UnlockTarget();
			this.lastTarget.target = null;
			this.isActive = false;
			this.StopAttack();
			*/
		}

		protected void StopAttack()
		{
			this.aggressiveToNoise.Value = 0f;
			this.creature.Aggression.Value = 0f;
			this.timeLastAttack = Time.time;
		}		

		public void Approach()
		{			
			Vector3 targetPosition = this.currentTargetIsDecoy ? this.currentTarget.transform.position : this.currentTarget.transform.TransformPoint(this.targetAttackPoint);
			base.swimBehaviour.SwimTo(targetAttackPoint, this.swimVelocity * 2f);		
		}

		public void Charge()
		{
			this.swimVelocity *= 4f;
		}


		public void OnCollisionEnter(Collision collision)
		{
			var fb = creature.GetComponent<FightBehavior>();
			var rb = collision.gameObject.GetComponent<Rigidbody>();
			var velocity = rb.velocity.magnitude;
			var thisReaper = creature.GetComponent<ReaperLeviathan>();

			if (velocity >= 80f)
			{			
				
				this.currentTarget = collision.gameObject;
				this.aggressiveToNoise.Value = 15f;

				base.swimBehaviour.SwimTo(thisReaper.gameObject.transform.forward + new Vector3(0, 0, 50), this.swimVelocity * 4f);
			}
		}

		public void UpdateAttackPoint()
		{
			var fb = this.GetComponentInParent<FightBehavior>();
			var rm = this.GetComponentInParent<ReaperMeleeAttack>();
			bool isTarget = fb.eyeHit.collider.GetComponentInParent<ReaperLeviathan>();
			if (isTarget)
			{
				this.targetAttackPoint = fb.eyeHit.collider.ClosestPointOnBounds(rm.mouth.transform.position);
				Transform attackTransform = fb.eyeHit.transform;
				var thisReaper = GetComponentInParent<ReaperLeviathan>();

				if (!this.currentTargetIsDecoy && this.currentTarget != null)
				{
					Vector3 vector = this.currentTarget.transform.InverseTransformPoint(thisReaper.transform.position);
					this.targetAttackPoint.z = Mathf.Clamp(vector.z, -2.5f, 2.5f);
					this.targetAttackPoint.y = Mathf.Clamp(vector.y, -2.5f, 2.5f);
					base.swimBehaviour.LookAt(attackTransform);
				}

			}

			Logger.Log(Logger.Level.Debug, "UPDATING ATTACK POINT!");
		}
				

	}
}
