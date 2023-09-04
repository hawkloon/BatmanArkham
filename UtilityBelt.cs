using System.CodeDom.Compiler;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace XESuitPod
{
    public class UtilityBeltLoadout : CustomData
    {
        public string[] gadgets;
    }
    public class UtilityBelt : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<UtilityBeltMono>();
        }
    }

    public class UtilityBeltMono : MonoBehaviour
    {
        private Item item;
        private List<UtilityBeltLoadout> loadouts;
        private MeshCollider meshCollider;
        private Holder[] holders;
        private int activeLoadout;
        public bool worn;
        public void Start()
        {
            item = GetComponent<Item>();
            item.handles[0].data.allowTelekinesis = false;
            WheelMenuInventory.instance.OnOpen += InstanceOnOnOpen;
            meshCollider = item.GetCustomReference<MeshCollider>("Mesh");
            holders = item.GetCustomReference("Holders").GetComponentsInChildren<Holder>();
            Debug.Log(holders.Length);
            loadouts = Catalog.GetDataList<UtilityBeltLoadout>();
            activeLoadout = 0;
            for (var index = 0; index < holders.Length; index++)
            {
                var holder = holders[index];
                var hData = holder.data;
                hData.spawnItemID = loadouts[activeLoadout].gadgets[index];
                holder.Load(hData);
            }
            item.OnHeldActionEvent += ItemOnOnHeldActionEvent;
        }

        public void  TurnOffHipHolders()
        {
            foreach (Holder holder in Player.currentCreature.holders)
            {
                if(holder.interactableId == "HolderHips") holder.gameObject.SetActive(false);
            }
        }

        public void TurnOnHipHolders()
        {
            foreach (Holder holder in Player.currentCreature.holders)
            {
                if(holder.interactableId == "HolderHips") holder.gameObject.SetActive(true);
            }
        }
        
        

        private void ItemOnOnHeldActionEvent(RagdollHand ragdollhand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                if (worn)
                {
                    item.transform.parent = null;
                    item.physicBody.isKinematic = false;
                    TurnOnHipHolders();
                    meshCollider.enabled = true;
                }
                else
                {
                    ragdollhand.TryRelease();
                    item.transform.parent = Player.currentCreature.animator.GetBoneTransform(HumanBodyBones.Spine);
                    item.transform.localPosition = new Vector3(1.41046619f, 0, -0.2f);
                    item.transform.localEulerAngles = new Vector3(6.18985905e-06f, 6.78921747f, 90.0000076f);
                    item.physicBody.isKinematic = true;
                    TurnOffHipHolders();
                    meshCollider.enabled = false;
                }

                worn = !worn;
            }
        }

        public void HolderEmpty(Holder holder)
        {
            if (holder != null && holder.items != null && holder.items[0] != null)
            {
                var  snappedItem = holder.items[0];
                
                holder.UnSnap(snappedItem, true);
                snappedItem.Despawn();
            }
            
        }

        public void GadgetSwap()
        {
            activeLoadout++;
            if (activeLoadout > loadouts.Count - 1)
            {
                activeLoadout = 0;
            }

            for (var index = 0; index < holders.Length; index++)
            {
                var holder = holders[index];
                var hData = holder.data;
                hData.spawnItemID = loadouts[activeLoadout].gadgets[index];
                holder.Load(hData);
            }
        }
        private void InstanceOnOnOpen()
        {
            if(Vector3.Distance(Player.currentCreature.handLeft.transform.position, WheelMenuInventory.instance.transform.position) > Vector3.Distance(Player.currentCreature.handRight.transform.position, WheelMenuInventory.instance.transform.position))
            {
                if (PlayerControl.handRight.gripPressed)
                {
                    GadgetSwap();
                    WheelMenuInventory.instance.Hide();
                }
            }
            else
            {
                if (PlayerControl.handLeft.gripPressed)
                {
                    GadgetSwap();
                    WheelMenuInventory.instance.Hide();
                }
            }
        }
    }
}