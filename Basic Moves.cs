using System;
using System.Collections;
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
        public new float biteDamage = 100f;        
        private BehaviourType bt;        
        public LiveMixin enemyLiveMixin;
        public float timeToUnfreeze = 0f;
        public float freezeCD = 2f;
        public Animation anim;
        public GameObject prefab;
        public GameObject prefab2;
        




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
            var rm = GetComponentInParent<ReaperMeleeAttack>();
            Collider[] selfColliders = this.GetComponentsInParent<Collider>();
            Collider[] validBiteables = fb.biteObjects.Except(selfColliders).ToArray();

            Logger.Log(Logger.Level.Info, "BITE PASSED CHECK 1");

            

            if (fb.critChance <= 0.40f)
            {
                this.biteDamage = 300f;
            }

            else if (fb.critChance > 0.40f)

            {
                this.biteDamage = 100f;
            }

            Logger.Log(Logger.Level.Info, "BITE PASSED CHECK 2");


            if (this.biteDamage == 800)
            {
                Logger.Log(Logger.Level.Info, "CRIT ROLL SUCCEEDED!");
            }

            

            foreach (var validBiteable in validBiteables)
            {
                
                Vector3 position = fb.mouth.transform.position;
                Vector3 bleedPoint = validBiteable.transform.InverseTransformPoint(position);
                LiveMixin enemyLiveMixin = validBiteable.GetComponentInParent<LiveMixin>();

                Logger.Log(Logger.Level.Info, "BITE PASSED CHECK 3");

                if (enemyLiveMixin)
                {
                    enemyLiveMixin.TakeDamage(this.biteDamage, position, DamageType.Normal, thisReaper.gameObject);

                    fb.BloodGen(validBiteable);
                    GameObject blood = Instantiate(fb.CachedBloodPrefab, bleedPoint, Quaternion.identity);
                    blood.SetActive(true);
                    Destroy(blood, 4f);

                }

                VFXSurface component = validBiteable.GetComponent<VFXSurface>();
                VFXSurfaceTypeManager.main.Play(component, VFXEventTypes.impact, position, Quaternion.identity, thisReaper.transform);

                Logger.Log(Logger.Level.Info, "BITE PASSED CHECK 4");
            }
            
            rm.animator.SetBool(MeleeAttack.biteAnimID, true);
            
            ErrorMessage.AddMessage($"BITE ATTACK!");
        }

        public void Claw()
        {            
            var fb = this.GetComponentInParent<FightBehavior>();
            var rm = this.GetComponentInParent<ReaperMeleeAttack>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            Collider[] selfColliders = this.GetComponentsInParent<Collider>();
            Collider[] validClawables = fb.clawObjects.Except(selfColliders).ToArray();

            SafeAnimator.SetBool(thisReaper.GetAnimator(), "attacking", true);
            
            
            Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 1");

            foreach (KeyValuePair<GameObject, Collider> pair in fb.clawObjects)

            {
                GameObject mandible = pair.Key;
                Collider clawed = pair.Value;

                LiveMixin enemyLiveMixin = clawed.GetComponentInParent<LiveMixin>();
                BreakableResource breakable = clawed.GetComponentInParent<BreakableResource>();

                Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 1.1");
                              
                Vector3 position = clawed.ClosestPointOnBounds(mandible.transform.position);
                Vector3 bleedPoint = clawed.transform.InverseTransformPoint(position);

                Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 1.2");                

                Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 2");

                    if (enemyLiveMixin)
                    {
                        enemyLiveMixin.TakeDamage(80f, position, DamageType.Normal, thisReaper.gameObject);
                        

                        fb.BloodGen(clawed);

                        GameObject blood1 = Instantiate(fb.CachedBloodPrefab, bleedPoint, Quaternion.identity);
                        
                        blood1.SetActive(true);
                        
                        Destroy(blood1, 4f);
                        

                        Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 3");

                    }

                    if (breakable)
                    {
                        breakable.HitResource();
                        breakable.HitResource();
                        breakable.HitResource();
                    }

                    VFXSurface component1 = clawed.GetComponentInParent<VFXSurface>();
                    
                    VFXSurfaceTypeManager.main.Play(component1, VFXEventTypes.impact, position, Quaternion.identity, thisReaper.transform);
                    

                    Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 4");
                
            }

            ErrorMessage.AddMessage($"CLAW ATTACK!");
            
            global::Utils.PlayEnvSound(rm.playerAttackSound, thisReaper.transform.forward, 35f);

        }        

        internal void FreezeRotation()
        {
            IEnumerator FreezeForSeconds()
            {
                var thisReaperBody = this.GetComponentInParent<Rigidbody>();
                thisReaperBody.constraints = RigidbodyConstraints.FreezeRotationX;
                yield return new WaitForSeconds(3);
                thisReaperBody.constraints = RigidbodyConstraints.None;

            }

            StartCoroutine(FreezeForSeconds());            
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
                    Invoke("FreezeRotation", 3f);
                    break;

                //Roll left
                case 2:
                    thisReaperBody.AddTorque(thisReaperBody.transform.forward * -2.5f, ForceMode.VelocityChange);
                    Invoke("FreezeRotation", 3f);
                    break;
            }           
                        
            Logger.Log(Logger.Level.Debug, $"TWIST ROTATION: {thisReaperBody.rotation}");
        }

        public void Lunge(Creature creature)
        {
            var fb = this.GetComponent<FightBehavior>();
            var ar = this.GetComponentInParent<AttackReaper>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            var rm = this.GetComponentInParent<ReaperMeleeAttack>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();

            if (creature.Tired.Value < 0.25f)
            {
                thisReaperBody.AddForce(rm.mouth.transform.forward * 20f, ForceMode.VelocityChange);
            }
            else if (creature.Tired.Value >= 0.25f)
            {
                thisReaperBody.AddForce(rm.mouth.transform.forward * 10f, ForceMode.VelocityChange);
            }            
            
            Logger.Log(Logger.Level.Debug, $"LUNGING AT {thisReaperBody.velocity.magnitude} M/S");

        }
       

        public void Reel()
        {
            var fb = this.GetComponent<FightBehavior>();
            var ar = this.GetComponentInParent<AttackReaper>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();
            var rm = this.GetComponentInParent<ReaperMeleeAttack>();
            var tr = rm.mouth.transform;
            var og = rm.mouth.transform.forward;

            thisReaperBody.AddForce(og + (5 * tr.up) + -(5 * tr.forward), ForceMode.VelocityChange);
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
