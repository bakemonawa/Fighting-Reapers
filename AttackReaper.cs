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
		public CreatureTrait aggressiveToNoise;
		private bool isActive;
		private GameObject currentTarget;
		private float timeLastAttack;
		private float timeNextSwim;
		private bool currentTargetIsDecoy;
		private Vector3 targetAttackPoint;
		private FightBehavior fb = FightBehavior.main;
		public static AttackReaper main;
		
		public override void Awake()
        {
			base.Awake();
			main = this;

        }

		public override void StartPerform(Creature creature)
		{
			SafeAnimator.SetBool(creature.GetAnimator(), "attacking", true);
			this.UpdateAttackPoint();
			this.lastTarget.SetLockedTarget(this.currentTarget);
			this.isActive = true;
		}

		public override void StopPerform(Creature creature)
		{
			SafeAnimator.SetBool(creature.GetAnimator(), "attacking", false);
			this.lastTarget.UnlockTarget();
			this.lastTarget.target = null;
			this.isActive = false;
			this.StopAttack();
		}

		protected void StopAttack()
		{
			this.aggressiveToNoise.Value = 0f;
			this.creature.Aggression.Value = 0f;
			this.timeLastAttack = Time.time;
		}

		public override void Perform(Creature creature, float deltaTime)
		{
			if (Time.time > this.timeNextSwim && this.currentTarget != null)
			{
				this.timeNextSwim = Time.time + this.swimInterval;
				Vector3 targetPosition = this.currentTargetIsDecoy ? this.currentTarget.transform.position : this.currentTarget.transform.TransformPoint(this.targetAttackPoint);
				base.swimBehaviour.SwimTo(targetPosition, this.swimVelocity);
			}
			creature.Aggression.Value = this.aggressiveToNoise.Value;
		}

		public void OnCollisionEnter(Collision collision)
		{
			if (fb.targetReaper != null && fb.targetReaper == collision.gameObject)
			{
				if (this.isActive)
				{
					return;
				}
				if (this.currentTarget != null && this.currentTargetIsDecoy)
				{
					return;
				}
				if (Vector3.Dot(collision.contacts[0].normal, collision.rigidbody.velocity) < 2.5f)
				{
					return;
				}
				this.currentTarget = collision.gameObject;
				this.aggressiveToNoise.Value = 1f;
			}
		}

		private void UpdateAttackPoint()
		{
			this.targetAttackPoint = Vector3.zero;
			if (!this.currentTargetIsDecoy && this.currentTarget != null)
			{
				Vector3 vector = this.currentTarget.transform.InverseTransformPoint(base.transform.position);
				this.targetAttackPoint.z = Mathf.Clamp(vector.z, -26f, 26f);
			}
		}

		public void SetCurrentTarget(GameObject target, bool isDecoy)
		{
			if (this.currentTarget != target)
			{
				this.currentTarget = target;
				this.currentTargetIsDecoy = isDecoy;
				if (this.isActive)
				{
					this.UpdateAttackPoint();
					this.lastTarget.SetLockedTarget(this.currentTarget);
				}
			}
		}

	}
}
