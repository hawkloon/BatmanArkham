using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace XESuitPod
{
    class Batarang : ItemModule 
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<BatarangMono>();
        }
    }
    public class BatarangMono : MonoBehaviour
    {
        Item item;
        public void Start()
        {
            item = GetComponent<Item>();
            item.OnUngrabEvent += Item_OnUngrabEvent;
        }
 
        private Creature BatarangTarget()
        {
            float bestDist = 0;
            Creature bestCreature = null;
            for(int i = 0; i < Creature.allActive.Count; i++)
            {
                if (Creature.allActive[i].isKilled || Creature.allActive[i].isPlayer) continue;
                if(bestDist == 0 || Vector3.Distance(Creature.allActive[i].ragdoll.targetPart.transform.position, item.transform.position) < bestDist)
                {
                    bestDist = Vector3.Distance(Creature.allActive[i].ragdoll.targetPart.transform.position, item.transform.position);
                    bestCreature = Creature.allActive[i];
                }
            }
            if (bestCreature != null) return bestCreature;
            else
            {
                return null;
            }
        }
        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            if (item.isFlying)
            {
                Creature c = BatarangTarget();
                if(c != null)
                {
                    Vector3 targetPos = (c.ragdoll.targetPart.transform.position - item.transform.position).normalized;
                    item.physicBody.AddForce(targetPos * 5, ForceMode.Impulse);
                }
            }
        }
    }
}
