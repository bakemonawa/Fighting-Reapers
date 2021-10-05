using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWE;
using UnityEngine;
using Logger = QModManager.Utility.Logger;

namespace FightingReapers
{
    
    class MouthTriggerController : MonoBehaviour
    {
        public GameObject mouth; //this trigger controller is for the mouth

        public void Awake()
        {
            mouth = this.gameObject;
            Logger.Log(Logger.Level.Debug, "This mandible established");

        }

        public void OnTriggerEnter(Collider other)
        {
            
            var fb = mouth.GetComponentInParent<FightBehavior>();            
            
                fb.biteObjects.Add(other);                        

        }

    }
}
