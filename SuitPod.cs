using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using System.Collections;


namespace XESuitPod
{
    public class XESSuitPod : ItemModule
    {
        public string suit;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<XESuitPodMono>();
        }
    }
    public class XESuitPodMono : MonoBehaviour
    {
        Item item;
        bool scannerRevealed;
        bool podOpened;
        Animation anim;
        public static ContainerData.Content[] BeyondArmor;
        XESSuitPod module;
        public void Start()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<XESSuitPod>();
            Debug.Log(module.suit);
            scannerRevealed = false;
            podOpened = false;
            anim = item.GetCustomReference("anim").GetComponent<Animation>();
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            item.mainCollisionHandler.OnTriggerEnterEvent += MainCollisionHandler_OnTriggerEnterEvent;
        }

        private void MainCollisionHandler_OnTriggerEnterEvent(Collider other)
        {
            if(scannerRevealed || module.suit != "XESuit" || module.suit != "AASuit" || module.suit != "AOSuit" || module.suit != "ACSuit")
            {
                if (other.GetComponentInParent<RagdollPart>() is RagdollPart part && !podOpened)
                {
                    if (part.type == RagdollPart.Type.LeftHand || part.type == RagdollPart.Type.RightHand)
                    {
                        if (module.suit == "XESuit")
                        {
                            StartCoroutine(SuitEquip(part.ragdoll.creature, XESuit));
                        }
                        else if(module.suit == "743Suit")
                        {
                            StartCoroutine(SuitEquip(part.ragdoll.creature, SevenFourThreeSuit));
                        }
                        else if (module.suit == "AKSuit")
                        {
                            StartCoroutine(SuitEquip(part.ragdoll.creature, AKSuit));
                        }
                        else if(module.suit == "AASuit")
                        {
                            StartCoroutine(SuitEquip(part.ragdoll.creature, AASuit));
                        }
                        else if(module.suit == "AOSuit")
                        {
                            StartCoroutine(SuitEquip(part.ragdoll.creature, AOSuit));
                        }
                        else if(module.suit == "ACSuit")
                        {
                            StartCoroutine(SuitEquip(part.ragdoll.creature, ACSuit));
                        }
                    }
                }
            }
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            item.physicBody.isKinematic = true;
        }

        public ContainerData.Content[] XESuit = new ContainerData.Content[5]
        {
            new ContainerData.Content(Catalog.GetData<ItemData>("XECowl")),
            new ContainerData.Content(Catalog.GetData<ItemData>("XETorso")),
            new ContainerData.Content(Catalog.GetData<ItemData>("XELegs")),
            new ContainerData.Content(Catalog.GetData<ItemData>("XEGloveLeft")),
            new ContainerData.Content(Catalog.GetData<ItemData>("XEGloveRight"))
        };
        public ContainerData.Content[] SevenFourThreeSuit = new ContainerData.Content[5]
{
            new ContainerData.Content(Catalog.GetData<ItemData>("743Cowl")),
            new ContainerData.Content(Catalog.GetData<ItemData>("743Torso")),
            new ContainerData.Content(Catalog.GetData<ItemData>("743Legs")),
            new ContainerData.Content(Catalog.GetData<ItemData>("743GloveLeft")),
            new ContainerData.Content(Catalog.GetData<ItemData>("743GloveRight"))
};
        public ContainerData.Content[] AKSuit = new ContainerData.Content[5]
{
            new ContainerData.Content(Catalog.GetData<ItemData>("AKCowl")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AKTorso")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AKLegs")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AKGloveLeft")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AKGloveRight"))
};
        public ContainerData.Content[] AASuit = new ContainerData.Content[5]
        {
            new ContainerData.Content(Catalog.GetData<ItemData>("AACowl")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AATorso")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AALegs")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AAGloveLeft")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AAGloveRight"))
        };
        public ContainerData.Content[] ACSuit = new ContainerData.Content[5]
        {
            new ContainerData.Content(Catalog.GetData<ItemData>("ACCowl")),
            new ContainerData.Content(Catalog.GetData<ItemData>("ACTorso")),
            new ContainerData.Content(Catalog.GetData<ItemData>("ACLegs")),
            new ContainerData.Content(Catalog.GetData<ItemData>("ACGloveLeft")),
            new ContainerData.Content(Catalog.GetData<ItemData>("ACGloveRight"))
        };
        public ContainerData.Content[] AOSuit = new ContainerData.Content[5]
{
            new ContainerData.Content(Catalog.GetData<ItemData>("AOCowl")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AOTorso")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AOLegs")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AOGloveLeft")),
            new ContainerData.Content(Catalog.GetData<ItemData>("AOGloveRight"))
}; 
        public IEnumerator SuitEquip(Creature creature, ContainerData.Content[] suit)
        {
            podOpened = true;
            item.mainCollisionHandler.OnTriggerEnterEvent -= MainCollisionHandler_OnTriggerEnterEvent;
            Catalog.GetData<EffectData>("PodScanner").Spawn(item.transform).Play();
            yield return Yielders.ForSeconds(1f);
            anim.Play("SuitReveal");
            foreach (ContainerData.Content content in suit)
            {
                creature.equipment.EquipWardrobe(content);
                Debug.Log(content.itemData.displayName);
            }

            if (CapeMono.local == null)
            {
                Catalog.GetData<ItemData>("BatmanCape").SpawnAsync(Cape =>
                {
                    Cape.physicBody.isKinematic = true;
                    Cape.transform.SetParent(Player.currentCreature.animator.GetBoneTransform(HumanBodyBones.Neck));
                    Cape.transform.localPosition = new Vector3(1.67089486f, 0, 0.406990916f);
                    Cape.transform.localEulerAngles = new Vector3(4.69575525e-06f, 348.064911f, 90);
                });
            }
            Player.currentCreature.equipment.armourEditModeEnabled = true;
            yield return Yielders.ForSeconds(10f);
            Player.currentCreature.equipment.armourEditModeEnabled = false;
            //Player.currentCreature.gameObject.AddComponent<ShockGloveMono>();
        }

        public RagdollPart GetPart(string name)
        {
            foreach (RagdollPart part in Player.currentCreature.ragdoll.parts)
            {
                if (part.name == name)
                {
                    return part;
                }
            }
            Debug.Log($"ERROR could not find part {name}");
            return null;
        }
        void Update()
        {
            if (scannerRevealed && !item.physicBody.isKinematic) item.physicBody.isKinematic = true;
            if (!scannerRevealed)
            {
                if(module.suit == "XESuit" || module.suit == "AASuit" || module.suit == "AOSuit" || module.suit == "ACSuit")
                { 
                    if (Vector3.Distance(Player.currentCreature.transform.position, item.transform.position) <= 3)
                    {
                        anim.Play("PodOpen");
                        scannerRevealed = true;
                    }
                }
            }
        }

        /* void MainCollisionHandler_OnTriggerEnterEvent(Collider other)
        {
            Debug.Log("Trigger touched");
            if (other.GetComponentInParent<RagdollPart>() is RagdollPart part && !podOpened)
            {
                if (part.type == RagdollPart.Type.LeftHand || part.type == RagdollPart.Type.RightHand)
                {
                    if (module.suit == "XESuit")
                    {
                        StartCoroutine(SuitEquip(part.ragdoll.creature, XESuit));
                    }
                    else if (module.suit == "AKSuit")
                    {
                        StartCoroutine(SuitEquip(part.ragdoll.creature, AKSuit));
                    }
                }
            }
        }*/
    }
}
