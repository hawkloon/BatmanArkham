using System;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace XESuitPod
{
    public class PodData : CustomData
    {
        public string[] itemIDs;
        public bool isSuit;
        public string HologramID;
    }
    public class PodCallerRewrite : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<PodCallerRewriteMono>();
        }
    }

    public class PodCallerRewriteMono : MonoBehaviour
    {
        private Item item;
        private bool hologramsOpen;
        private GameObject gadgetHolograms;
        private GameObject suitHolograms;
        private bool displayingGadgets;
        private string selectedHologram;
        bool firstTap;
        bool timer;
        static float tapTimerMax = 0.14f;
        float tapTimer = tapTimerMax;
        public void Start()
        {
            item = GetComponent<Item>();
            
            gadgetHolograms = item.GetCustomReference("GadgetHolograms").gameObject;
            suitHolograms = item.GetCustomReference("SuitHolograms").gameObject;
            
            HologramClose();
            
            
            item.OnGrabEvent += ItemOnOnGrabEvent;
            item.OnUngrabEvent += ItemOnOnUngrabEvent;
            item.OnHeldActionEvent += ItemOnOnHeldActionEvent;
        }

        private void ItemOnOnUngrabEvent(Handle handle, RagdollHand ragdollhand, bool throwing)
        {
            if(hologramsOpen) HologramClose();
        }

        public void HologramClose()
        {
            gadgetHolograms.SetActive(false);
            suitHolograms.SetActive(false);

            hologramsOpen = false;
            displayingGadgets = true;
        }

        private void ItemOnOnHeldActionEvent(RagdollHand ragdollhand, Handle handle, Interactable.Action action)
        {
            if(action == Interactable.Action.AlternateUseStart)
            {
                firstTap = true;
                timer = true;
            }
        }

        private void ItemOnOnGrabEvent(Handle handle, RagdollHand ragdollhand)
        {
            if (!hologramsOpen || handle == item.mainHandleLeft || handle == item.mainHandleRight) return;


            selectedHologram = handle.name.Split('.')[0];
            Debug.Log(selectedHologram);
            HologramClose();
            ragdollhand.TryRelease();

        }

        private void Update()
        {
            if (item.mainHandler != null)
            {
                if (timer)
                {
                    if (tapTimer >= 0)
                    {
                        tapTimer -= Time.deltaTime;
                    }
                    else
                    {
                        timer = false;
                        tapTimer = tapTimerMax;

                        if (firstTap && item.mainHandler.playerHand.controlHand.alternateUsePressed)
                        {
                            if (!hologramsOpen)
                            {
                                gadgetHolograms.SetActive(true);

                                hologramsOpen = true;
                            }
                            else
                            {
                                HologramClose();
                            }
                        }
                        else
                        {
                            if (hologramsOpen)
                            {
                                if (displayingGadgets)
                                {
                                    gadgetHolograms.SetActive(false);
                                    suitHolograms.SetActive(true);
                                    displayingGadgets = false;
                                }
                                else
                                {
                                    suitHolograms.SetActive(false);
                                    gadgetHolograms.SetActive(true);
                                    displayingGadgets = true;
                                }
                            }
                            else
                            {
                                if (selectedHologram.Contains("Suit"))
                                {
                                    Catalog.GetData<ItemData>(selectedHologram + "Pod").SpawnAsync(Pod =>
                                    {
                                        Pod.transform.position = Player.currentCreature.transform.position + (Player.currentCreature.ragdoll.headPart.transform.forward) + new Vector3(0f, 30f, 0f);
                                        Pod.transform.rotation = Quaternion.identity;
                                        Pod.physicBody.isKinematic = false;
                                        Pod.isThrowed = true;
                                    });
                                }
                                else
                                {
                                    Catalog.GetData<ItemData>("ACWeaponPod").SpawnAsync(Pod =>
                                    {
                                        Pod.transform.position = Player.currentCreature.transform.position + (Player.currentCreature.ragdoll.headPart.transform.forward) + new Vector3(0f, 30f, 0f);
                                        Pod.transform.rotation = Quaternion.identity;
                                        Pod.physicBody.isKinematic = false;
                                        Pod.isThrowed = true;
                                        var holderData = Pod.GetCustomReference("Holder").GetComponent<Holder>().data;
                                        holderData.spawnItemID = selectedHologram;
                                        holderData.maxQuantity = 1;
                                        holderData.spawnQuantity = 1;
                                        holderData.highlightDefaultTitle = selectedHologram;
                                        Pod.GetCustomReference("Holder").GetComponent<Holder>().Load(holderData);
                                    });
                                }
                                Catalog.GetData<EffectData>("PodCalled").Spawn(item.transform).Play();
                            }

                            firstTap = false;
                        }
                    }
                }
            }
        }
    }
}