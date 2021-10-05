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
    
    class TriggerController : MonoBehaviour
    {
        public GameObject thisMandible; //this trigger controller is for mandibles

        public void Awake()
        {
            thisMandible = this.gameObject;
            Logger.Log(Logger.Level.Debug, "This mandible established");

        }

        public void OnTriggerEnter(Collider other)
        {
            var fb = thisMandible.GetComponentInParent<FightBehavior>();            
            
                fb.clawObjects.Add(thisMandible, other);                        

        }

    }
}
