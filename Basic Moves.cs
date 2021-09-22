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
        private BehaviourType bt;        
        public LiveMixin enemyLiveMixin;
        public float timeToUnfreeze = 0f;
        public float freezeCD = 3f;
        public Animation anim;




        public override void OnTouch(Collider collider)
        {
            base.OnTouch(collider);
            collider2 = collider;
            
        }
        /*
        private void StartSetAnimParam(string paramName, float duration)
        {
            base.StartCoroutine(this.SetAnimParamAsync(paramName, false, duration));
        }

        private IEnumerator SetAnimParamAsync(string paramName, bool value, float duration)
        {
            yield return new WaitForSeconds(duration);
            this.animator.SetBool(paramName, value);
            yield break;
        }

        */



        public void Bite()
        {
            
            var fb = GetComponentInParent<FightBehavior>();
            var thisReaper = GetComponentInParent<ReaperLeviathan>();
            var melee = GetComponentInParent<MeleeAttack>();
            

            Logger.Log(Logger.Level.Info, "BITE PASSED CHECK 1");

            /*

            if (fb.critChance <= 0.40f)
            {
                this.biteDamage = 3000f;
            }

            else if (fb.critChance > 0.40f)

            {
                this.biteDamage = 160f;
            }

            Logger.Log(Logger.Level.Info, "BITE PASSED CHECK 2");


            if (this.biteDamage == 3000)
            {
                Logger.Log(Logger.Level.Info, "CRIT ROLL SUCCEEDED!");
            }

            */

            if (fb.biteObject != null)
            {
                Vector3 position = fb.biteObject.ClosestPointOnBounds(fb.bitePoint.transform.position);
                Vector3 bleedPoint = fb.biteObject.transform.InverseTransformPoint(position);
                LiveMixin enemyLiveMixin = fb.biteObject.GetComponentInParent<LiveMixin>();
                Logger.Log(Logger.Level.Info, "BITE PASSED CHECK 3");

                if (enemyLiveMixin)
                {
                    enemyLiveMixin.TakeDamage(this.biteDamage, position, DamageType.Normal, thisReaper.gameObject);
                    UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, bleedPoint, enemyLiveMixin.damageEffect.transform.rotation);
                    UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, bleedPoint + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                    UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, bleedPoint + new Vector3(-0.5f, 0.5f, 0), Quaternion.identity);
                    UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, bleedPoint + new Vector3(0.5f, -0.5f, 0), Quaternion.identity);
                    
                }

                VFXSurface component = fb.biteObject.GetComponent<VFXSurface>();
                VFXSurfaceTypeManager.main.Play(component, VFXEventTypes.impact, position, Quaternion.identity, thisReaper.transform);

                Logger.Log(Logger.Level.Info, "BITE PASSED CHECK 4");
            }
            
            this.animator.SetBool(MeleeAttack.biteAnimID, true);
            
            ErrorMessage.AddMessage($"BITE ATTACK!");
        }

        public void Claw()
        {            
            var fb = this.GetComponentInParent<FightBehavior>();
            var rm = this.GetComponentInParent<ReaperMeleeAttack>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            
            SafeAnimator.SetBool(thisReaper.GetAnimator(), "attacking", true);
            
            
            Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 1");

            if (fb.clawObject != null)

            {
                
                LiveMixin enemyLiveMixin = fb.clawObject.GetComponentInParent<LiveMixin>();
                BreakableResource breakable = fb.clawObject.GetComponentInParent<BreakableResource>();

                Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 1.1");
                              
                Vector3 position = fb.clawObject.ClosestPointOnBounds(fb.clawPoint.transform.position);
                Vector3 bleedPoint = fb.clawObject.transform.InverseTransformPoint(position);
                Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 2");

                    if (enemyLiveMixin)
                    {
                        enemyLiveMixin.TakeDamage(160f, position, DamageType.Normal, thisReaper.gameObject);

                        UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, bleedPoint, Quaternion.identity);                                                
                        UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, bleedPoint + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                        UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, bleedPoint + new Vector3(-0.5f, 0.5f, 0), Quaternion.identity);
                        UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, bleedPoint + new Vector3(0.5f, -0.5f, 0), Quaternion.identity);
                        Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 3");
                    }

                    if (breakable)
                    {
                        breakable.HitResource();
                        breakable.HitResource();
                        breakable.HitResource();
                    }

                    VFXSurface component = fb.clawObject.GetComponent<VFXSurface>();
                    VFXSurfaceTypeManager.main.Play(component, VFXEventTypes.impact, position, Quaternion.identity, thisReaper.transform);
                    Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 4");
                
            }

            ErrorMessage.AddMessage($"CLAW ATTACK!");
            
            global::Utils.PlayEnvSound(rm.playerAttackSound, thisReaper.transform.forward, 20f);

        }        

        internal void FreezeRotation()
        {
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();
            thisReaperBody.freezeRotation = true;

        }

        public void Twist()

        {
            var fb = this.GetComponent<FightBehavior>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();

            int directionChoose = UnityEngine.Random.Range(1, 3);
            

            switch (directionChoose)
            {
                //Roll right
                case 1: 
                    thisReaperBody.AddTorque(thisReaperBody.transform.forward * 2.5f, ForceMode.VelocityChange);
                    Invoke("FreezeRotation", 2f);
                    break;

                //Roll left
                case 2:
                    thisReaperBody.AddTorque(thisReaperBody.transform.forward * -2.5f, ForceMode.VelocityChange);
                    Invoke("FreezeRotation", 2f);
                    break;
            }
            
            if (thisReaperBody.freezeRotation = true && fb.biteObject == null && Time.time > timeToUnfreeze || fb.targetDist > 30)
            {
                thisReaperBody.freezeRotation = false;
                timeToUnfreeze = Time.time + freezeCD;
            }
            
            Logger.Log(Logger.Level.Debug, $"TWIST ROTATION: {thisReaperBody.rotation}");
        }

        public void Lunge()
        {
            var fb = this.GetComponent<FightBehavior>();
            var ar = this.GetComponentInParent<AttackReaper>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();

            thisReaperBody.AddForce(mouth.transform.forward *20f, ForceMode.VelocityChange);            
            
            Logger.Log(Logger.Level.Debug, $"LUNGING AT {thisReaperBody.velocity.magnitude} M/S");


        }
       

        public void Reel()
        {
            var fb = this.GetComponent<FightBehavior>();
            var ar = this.GetComponentInParent<AttackReaper>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();

            thisReaperBody.AddForce(thisReaper.transform.forward + new Vector3 (0, 5, -5), ForceMode.VelocityChange);
            Logger.Log(Logger.Level.Debug, $"REELING!");
            if (ar.currentTarget != null)
            {
                Invoke("Lunge", 1.5f);
            }


        }


        public void Tackle()
        {

            var fb = this.GetComponentInParent<FightBehavior>();            
            
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();

            var thisReaperBody = this.GetComponentInParent<Rigidbody>();

            var velocity = thisReaperBody.velocity.magnitude;

            RaycastHit hit;

            Physics.SphereCast(mouth.transform.position, 1f, mouth.transform.up, out hit, 2.5f);

            if (hit.collider != null)

            {
                Vector3 position = hit.collider.ClosestPointOnBounds(this.mouth.transform.up);
                Rigidbody hitBody = hit.collider.GetComponentInParent<Rigidbody>();
                
                LiveMixin enemyLiveMixin = hit.collider.GetComponentInParent<LiveMixin>();

                if (velocity > 70f)

                {
                    if (hitBody)
                    {
                        hitBody.AddForceAtPosition(thisReaperBody.transform.forward * velocity, this.mouth.transform.position, ForceMode.Impulse);
                    }

                    if (enemyLiveMixin)
                    {
                        enemyLiveMixin.TakeDamage(velocity/2, position, DamageType.Collide, thisReaper.gameObject);
                        UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, position, Quaternion.identity);
                    }

                    VFXSurface component = hit.collider.GetComponent<VFXSurface>();
                    VFXSurfaceTypeManager.main.Play(component, VFXEventTypes.impact, mouth.transform.position, Quaternion.identity, thisReaper.transform);

                }

            }

        }

        public void Reap()
        {

            var fb = this.GetComponent<FightBehavior>();
            var rm = this.GetComponentInParent<ReaperMeleeAttack>();

            if (!Player.main)
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
