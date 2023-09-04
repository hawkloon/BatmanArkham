using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    class Hologram : ItemModule 
    {
        public string selection;
        public bool isSuit;
        public Item Caller;
        public Transform placement;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.OnGrabEvent += Item_OnGrabEvent;
        }

        public void Position()
        {
            item.transform.SetParent(Caller.transform);
            item.transform.localPosition = placement.localPosition;
        }

        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            Caller.gameObject.GetComponent<PodCallerMono>().Hologram(selection, isSuit);
        }
    }
}
