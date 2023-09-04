using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    class PodCaller : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<PodCallerMono>();
        }
    }

    public class PodCallerMono : MonoBehaviour
    {
        Item item;
        public string[] Pods = new string[10]
        {
            "AKSuit",
            "XESuit",
            "AKGrapnelGun",
            "LineLauncher",
            "RemoteClaw",
            "ExplosiveGel",
            "743Suit",
            "AASuit",
            "AOSuit",
            "ACSuit"
        };
        public List<Item> Holos = new List<Item>();
        public GameObject[] HoloPos;
        string PodSelected;
        bool suitIsPod;
        bool SuitCalled;
        bool firstTap;
        bool secondTap;
        bool Timer;
        static float tapTimerMax = 0.14f;
        float tapTimer = tapTimerMax;
        public void Start()
        {
            item = GetComponent<Item>();
            SuitCalled = false;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            suitIsPod = false;
            HoloPos = new GameObject[10]
            {
                item.GetCustomReference("HoloPos1").gameObject,
                item.GetCustomReference("HoloPos2").gameObject,
                item.GetCustomReference("HoloPos3").gameObject,
                item.GetCustomReference("HoloPos4").gameObject,
                item.GetCustomReference("HoloPos5").gameObject,
                item.GetCustomReference("HoloPos6").gameObject,
                item.GetCustomReference("HoloPos7").gameObject,
                item.GetCustomReference("HoloPos8").gameObject,
                item.GetCustomReference("HoloPos9").gameObject,
                item.GetCustomReference("HoloPos10").gameObject
            };
            firstTap = false;
            secondTap = false;
            Timer = false;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if(action == Interactable.Action.AlternateUseStart)
            {
                firstTap = true;
                Timer = true;
            }
        }

        private void Pod_OnDespawnEvent(EventTime eventTime)
        {
            if(eventTime == EventTime.OnEnd)
            {
                SuitCalled = false;
            }
        }
        public void Hologram(string Selection, bool isSuit)
        {
            PodSelected = Selection;
            suitIsPod = isSuit;
            if(Holos != null)
            {
                foreach(Item item in Holos)
                {
                    item.Despawn();
                }
                Holos.Clear();
            }
        }
        void Update()
        {
            if(item.mainHandler != null)
            {
                if (Timer)
                {
                    if(tapTimer >= 0)
                    {
                        tapTimer -= Time.deltaTime;
                    }
                    else
                    {
                        Timer = false;
                        tapTimer = tapTimerMax;

                        if (firstTap && item.mainHandler.playerHand.controlHand.alternateUsePressed)
                        {
                            var HoloParent = new GameObject("HoloParent");
                            HoloParent.transform.SetPositionAndRotation(item.transform.position, item.transform.rotation);
                            HoloParent.transform.SetParent(item.transform);
                            for (int i = 0; i < Pods    .Length; i++)
                            { 
                                int index = i;
                                Catalog.GetData<ItemData>(Pods[i] + "Holo").SpawnAsync(Holo =>
                                {
                                    Holo.transform.parent = item.transform;
                                    Holo.transform.localPosition = HoloPos[index].transform.localPosition;
                                    Holo.transform.rotation = item.transform.rotation;
                                    Holo.physicBody.isKinematic = true;
                                    var data = Holo.data.GetModule<Hologram>();
                                    data.Caller = item;
                                    data.placement = HoloPos[index].transform;
                                    Holo.data.GetModule<Hologram>().Position();
                                    Holos.Add(Holo);
                                });
                            }
                            /*if (Holos != null) return;
                            else
                            {
                                int e = 0;
                                foreach (string s in Pods)
                                {
                                    Catalog.GetData<ItemData>($"{s}Holo").SpawnAsync(Holo =>
                                    {
                                        int a = e;
                                        Holo.transform.position = item.transform.position -= new Vector3(a/10, 0, 0);
                                        Holo.transform.rotation = Quaternion.identity;
                                        Holo.transform.SetParent(item.transform);
                                        Holo.rb.isKinematic = true;
                                        Holo.data.GetModule<Hologram>().Caller = item;
                                        Holos.Add(Holo);
                                    });
                                    e++;
                                }
                            }*/
                        }
                        else
                        {
                            if (suitIsPod)
                            {
                                Catalog.GetData<ItemData>(PodSelected + "Pod").SpawnAsync(Pod =>
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
                                    holderData.spawnItemID = PodSelected;
                                    holderData.maxQuantity = 1;
                                    holderData.spawnQuantity = 1;
                                    holderData.highlightDefaultTitle = PodSelected;
                                    Pod.GetCustomReference("Holder").GetComponent<Holder>().Load(holderData);
                                });
                            }
                            {
                                Catalog.GetData<EffectData>("PodCalled").Spawn(item.transform.position, Quaternion.identity).Play();
                            }
                            firstTap = false;
                        }
                    }
                }
            }
        }
    }
}
