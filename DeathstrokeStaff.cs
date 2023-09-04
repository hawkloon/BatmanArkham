using System;
using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    public class DeathstrokeStaff : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            item.gameObject.AddComponent<DeathstrokeStaffMono>();
        }
    }

    public class DeathstrokeStaffMono : MonoBehaviour
    {
        private Item item;
        private bool extended;
        private Animation anim;

        public void Start()
        {
            item = GetComponent<Item>();
            extended = false;
            anim = GetComponent<Animation>();
            item.OnHeldActionEvent += ItemOnOnHeldActionEvent;
        }

        private void ItemOnOnHeldActionEvent(RagdollHand ragdollhand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                if (extended)
                {
                    anim.Play("StaffRetract");
                    extended = false;
                }
                else
                {
                    anim.Play("StaffExtend");
                    extended = true;
                }
            }
        }
    }
}