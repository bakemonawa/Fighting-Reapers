using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;
using HarmonyLib;
using Logger = QModManager.Utility.Logger;


namespace FightingReapers
{
    public class BasicFightingMoves : MeleeAttack        
    {
        
        public static BasicFightingMoves main;
        internal Collider collider2;
        public Vector3 reaperMouth;
        public new float biteDamage = 160f;
        public float critChance;
        private BehaviourType bt;
        public GameObject bitObject;
        public Collider bitCollider;
        public LiveMixin enemyLiveMixin;
        
        
        

        

        public override void OnTouch(Collider collider)
        {
            base.OnTouch(collider);
            collider2 = collider;
            
        }

      

        public void Bite(Collider collider)
        {
            
            var fb = this.GetComponentInParent<FightBehavior>();
            var thisReaper = GetComponentInParent<ReaperLeviathan>();
            Vector3 position = collider.ClosestPointOnBounds(this.mouth.transform.position);
            LiveMixin enemyLiveMixin = collider.gameObject.GetComponentInParent<LiveMixin>();

            critChance = UnityEngine.Random.Range(0, 1);

            if (critChance <= 0.85)
            {
                this.biteDamage = 3000f;
            }

            else if (critChance > 0.85)

            {
                this.biteDamage = 160f;
            }                    
                                   
                
            this.animator.SetBool(MeleeAttack.biteAnimID, true);

            VFXSurface component = bitObject.GetComponent<VFXSurface>(); 
            VFXSurfaceTypeManager.main.Play(component, VFXEventTypes.impact, position, Quaternion.identity, thisReaper.transform);            

            
            
            if (enemyLiveMixin)

            {
                enemyLiveMixin.TakeDamage(this.biteDamage, position, DamageType.Normal, thisReaper.gameObject);
                UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, position, enemyLiveMixin.damageEffect.transform.rotation);
                
            }          

        }

        public void Claw()
        {

            Animation anim = new Animation();

            SafeAnimator.SetBool(creature.GetAnimator(), "attacking", true);

            anim["attacking"].speed = 8f;

            var fb = this.GetComponent<FightBehavior>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            
            
            GameObject clawedObject = null;
            
            RaycastHit clawPoint;            

            Physics.Raycast(thisReaper.transform.position, thisReaper.transform.forward, out clawPoint, 4f);

            Vector3 clawRange = clawPoint.transform.position;


            if (clawPoint.transform.gameObject != null)
            {
                clawedObject = clawPoint.transform.gameObject;
            }

            if (clawedObject != null)

            {
                Collider clawedCollider = clawedObject.GetComponentInParent<Collider>();
                LiveMixin enemyLiveMixin = clawedObject.GetComponentInParent<LiveMixin>();
                BreakableResource breakable = clawedObject.GetComponentInParent<BreakableResource>();

                Vector3 position = clawedCollider.ClosestPointOnBounds(clawRange);

                if (enemyLiveMixin)
                {                    
                    var ra = this.GetComponentInParent<ReaperMeleeAttack>();

                    enemyLiveMixin.TakeDamage(this.biteDamage, clawRange, DamageType.Normal, thisReaper.gameObject);
                    UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, clawRange, Quaternion.identity);
                    enemyLiveMixin.damageEffect.transform.localScale += new Vector3(20, 20, 20);
                    global::Utils.PlayEnvSound(ra.playerAttackSound, thisReaper.transform.forward, 20f);
                }

                if (breakable)
                {
                    breakable.HitResource();
                    breakable.HitResource();
                    breakable.HitResource();
                }

                VFXSurface component = clawedObject.GetComponent<VFXSurface>();
                VFXSurfaceTypeManager.main.Play(component, VFXEventTypes.knife, position, Quaternion.identity, thisReaper.transform);

            }

            ErrorMessage.AddMessage($"CLAW ATTACK!");

        }

        public void Charge()
        {
            var fb = creature.GetComponent<FightBehavior>();
            
            var swim = this.GetComponentInParent<SwimBehaviour>();
            float swimVelocity = 10f;
            var thisReaper = creature.GetComponentInChildren<ReaperLeviathan>();

            swim.SwimTo(fb.targetReaper.transform.position, swimVelocity * 8f);            

        }



        public void Twist()

        {
            var fb = this.GetComponent<FightBehavior>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();

            thisReaperBody.AddTorque(thisReaperBody.transform.forward * 300, ForceMode.VelocityChange);
            ErrorMessage.AddMessage($"TWISTING");

        }

        public void Lunge()
        {
            var fb = creature.GetComponent<FightBehavior>();
            var ar = this.GetComponentInParent<AttackReaper>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            var thisReaperBody = creature.GetComponentInChildren<Rigidbody>();

            thisReaperBody.AddForce(thisReaperBody.transform.forward * 100f, ForceMode.VelocityChange);
            
            ErrorMessage.AddMessage($"LUNGING AT {thisReaperBody.velocity.magnitude} M/S");


        }
       

        public void Reel()
        {
            var fb = creature.GetComponent<FightBehavior>();
            var ar = this.GetComponentInParent<AttackReaper>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();

            thisReaperBody.AddForce(thisReaperBody.transform.forward + new Vector3 (0, 100, -100), ForceMode.VelocityChange);

            if (ar.currentTarget != null)
            {
                Invoke("Lunge", 2);
            }


        }


        public void Tackle()
        {

            var fb = this.GetComponentInParent<FightBehavior>();

            

            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();

            var thisReaperBody = this.GetComponentInParent<Rigidbody>();

            var velocity = thisReaperBody.velocity.magnitude;

            
            Vector3 position = collider2.ClosestPointOnBounds(this.mouth.transform.up);
            Rigidbody hitBody = bitObject.GetComponentInParent<Rigidbody>();
            var enemyLiveMixin = bitObject.GetComponentInParent<LiveMixin>();            

            if (velocity > 100f)

            {
                if (hitBody)
                {
                    hitBody.AddForceAtPosition(thisReaperBody.transform.forward * velocity * 8, this.mouth.transform.position, ForceMode.Impulse);
                }

                if (enemyLiveMixin)
                {
                    enemyLiveMixin.TakeDamage(velocity, position, DamageType.Collide, thisReaper.gameObject);
                    UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, position, Quaternion.identity);
                }               
                
                VFXSurface component = bitObject.GetComponent<VFXSurface>();
                VFXSurfaceTypeManager.main.Play(component, VFXEventTypes.impact, fb.bitepoint.transform.position, Quaternion.identity, thisReaper.transform);
                
            }



        }

        public void Reap()
        {

            var fb = this.GetComponent<FightBehavior>();
            

            if (!Player.main && (bt == BehaviourType.Shark || bt == BehaviourType.MediumFish || bt == BehaviourType.SmallFish))
            {

                Vector3 position = collider2.ClosestPointOnBounds(this.mouth.transform.position * 3f);
                Animation anim = new Animation();

                SafeAnimator.SetBool(creature.GetAnimator(), "attacking", true);
                this.animator.SetBool(MeleeAttack.biteAnimID, true);
                anim["attacking"].speed = 10f;

                liveMixin.TakeDamage(1200, reaperMouth * 1.5f, DamageType.Normal, null);

                if (this.damageFX != null)
                {
                    UnityEngine.Object.Instantiate<GameObject>(this.damageFX, position, this.damageFX.transform.rotation);
                    this.damageFX.transform.localScale = new Vector3(20f, 20f, 20f);
                }
                if (this.attackSound != null)
                {
                    global::Utils.PlayEnvSound(this.attackSound, position, 20f);
                }
            }
            
        }
    }
}
