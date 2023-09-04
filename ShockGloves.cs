using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    class ShockGloveMono : MonoBehaviour
    {
        float charge;
        decimal sd;
        bool enabled;
        EffectData effectData;
        public void Start()
        {
            Player.currentCreature.ragdoll.OnContactStartEvent += Ragdoll_OnContactStartEvent;
            effectData = Catalog.GetData<EffectData>("ImbueLightningRagdoll"); 
        }

        private void Ragdoll_OnContactStartEvent(CollisionInstance collisionInstance, RagdollPart ragdollPart)
        {
            if(ragdollPart.type == RagdollPart.Type.LeftHand || ragdollPart.type == RagdollPart.Type.RightHand)
            {
                if(collisionInstance.intensity >= 0.5f && collisionInstance.targetCollider.GetComponentInParent<Creature>())
                {
                    if (charge <= 100f && !enabled)
                    {
                        charge += 5f;
                    }
                    else if(enabled)
                    {
                        if(collisionInstance.targetCollider.GetComponentInParent<Creature>() is Creature creature)
                        {
                            creature.TryElectrocute(20f, 2f, true, true, effectData);
                            charge -= 5f;
                        }
                    }
                }
                Debug.Log(charge);
                
            }
        }
        void Update()
        {
            if (charge >= 100f && !enabled) enabled = true;

            if (charge <= 0f && enabled) enabled = false;
        }
    }
}
