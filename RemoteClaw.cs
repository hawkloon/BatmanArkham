using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using System.Collections;
using HawkUtils;

namespace XESuitPod
{
    class RCHitInfo
    {
        public Creature[] hitCreature {get; set;}
        public Item[] hitItem { get; set; }
        public Vector3 worldPos { get; set; }
    }
    class RemoteClaw : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<RemoteClawMono>();
        }
    }
    public class RemoteClawMono : MonoBehaviour
    {
        Item item;
        private bool cooldown;
        public bool targetSelected;
        public Creature selectedCreature;
        public Item selectedItem;
        public void Start()
        {
            item = GetComponent<Item>();
            targetSelected = false;
            cooldown = false;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }
        private IEnumerator ShootDelay()
        {
            cooldown = true;
            yield return new WaitForSeconds(0.5f);
            cooldown = false;
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                if (Physics.Raycast(item.flyDirRef.transform.position, item.flyDirRef.transform.forward, out RaycastHit hit, 50f))
                {
                    Rigidbody rb = hit.rigidbody;

                    if (rb == null) return;

                    selectedCreature = null;
                    selectedItem = null;

                    targetSelected = rb.TryGetComponent<Item>(out selectedItem) || rb.TryGetComponentInParent<Creature>(out selectedCreature);
                    if (targetSelected)
                    {
                        var debugString = selectedItem ? "Item Targeted" : "Creature Targeted";
                        Debug.Log($"{debugString}");
                    }
                }
            }
            if (action == Interactable.Action.UseStart)
            {
                Catalog.GetData<ItemData>("RCHook").SpawnAsync(Hook =>
                {
                    Hook.transform.position = item.flyDirRef.transform.position;
                    Hook.transform.rotation = item.flyDirRef.transform.rotation;
                    Hook.IgnoreObjectCollision(item);
                    Hook.physicBody.AddForce(item.flyDirRef.transform.forward * 20, ForceMode.Impulse);
                    Hook.gameObject.AddComponent<RCHookMono>().SetUp(item);
                    StartCoroutine(ShootDelay());
                });
            }
        }
        public class RCHookMono : MonoBehaviour
        {
            public Item item;
            public RemoteClawMono gunMono;
            bool targeting;
            bool used;
            Creature targetCreature;
            public GameObject[] linepositions = new GameObject[2];
            Item targetItem;
            LineRenderer line;
            public void SetUp(Item gun)
            {
                gunMono = gun.gameObject.GetComponent<RemoteClawMono>();
                if (gunMono.targetSelected)
                {
                    if (gunMono.selectedCreature)
                    {
                        targetCreature = gunMono.selectedCreature;
                        targeting = true;
                    }
                    else if (gunMono.selectedItem)
                    {
                        targetItem = gunMono.selectedItem;
                        targeting = true;

                    }
                    else Debug.Log("target is selected, but either an Item nor Creature!");
                }

            }
            public void Start()
            {
                item = GetComponent<Item>();
                used = false;
                line = item.gameObject.AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                line.startWidth = 0.02f;
                line.endWidth = 0.02f;
                line.material.color = Color.black;
                item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            }
            public void CreaturePull(Creature creature, Vector3 pullPoint)
            {
                if (creature.isKilled || creature.isPlayer) return;
                Vector3 distance = (creature.transform.position - pullPoint).normalized;
                creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                foreach (RagdollPart part in creature.ragdoll.parts)
                {
                    part.physicBody.AddForce(-distance * 20 * part.physicBody.mass, ForceMode.Impulse);
                }
            }
            public void ItemPull(Item i, Vector3 pullPoint)
            {
                Vector3 distance = (item.transform.position - pullPoint).normalized;
                i.physicBody.AddForce(-distance * 20 * i.physicBody.mass, ForceMode.Impulse);
            }
            public Creature RetreiveClosestCreature()
            {
                return null;
            }
            private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
            {
                if (!used)
                {
                    if (collisionInstance.targetCollider.attachedRigidbody != null)
                    {
                        Rigidbody rb = collisionInstance.targetCollider.attachedRigidbody;
                        var joint = item.gameObject.AddComponent<FixedJoint>();
                        joint.connectedBody = rb;
                        if (targeting)
                        {
                            if (targetCreature)
                            {
                                if (rb.GetComponentInParent<Creature>() is Creature creature)
                                {
                                    Debug.Log("Creature");
                                    CreaturePull(creature, targetCreature.ragdoll.targetPart.transform.position);
                                    CreaturePull(targetCreature, creature.ragdoll.targetPart.transform.position);
                                    line.SetPosition(0, creature.ragdoll.targetPart.transform.position);
                                    line.SetPosition(1, targetCreature.ragdoll.targetPart.transform.position);
                                    linepositions[0] = creature.ragdoll.targetPart.gameObject;
                                    linepositions[1] = targetCreature.ragdoll.targetPart.gameObject;
                                }
                                else if (rb.GetComponent<Item>() is Item i)
                                {
                                    Debug.Log("Item");
                                    CreaturePull(targetCreature, i.transform.position);
                                    ItemPull(i, targetCreature.ragdoll.targetPart.transform.position);
                                    line.SetPosition(0, i.transform.position);
                                    line.SetPosition(1, targetCreature.ragdoll.targetPart.transform.position);
                                    linepositions[0] = i.gameObject;
                                    linepositions[1] = targetCreature.ragdoll.targetPart.gameObject;
                                }
                                targetCreature = null;
                            }
                            else if (targetItem)
                            {
                                if (rb.GetComponentInParent<Creature>() is Creature creature)
                                {
                                    Debug.Log("Creature");
                                    CreaturePull(creature, targetItem.transform.position);
                                    ItemPull(targetItem, creature.ragdoll.headPart.transform.position);
                                    line.SetPosition(0, creature.transform.position);
                                    line.SetPosition(1, targetItem.transform.position);
                                    linepositions[0] = creature.ragdoll.targetPart.gameObject;
                                    linepositions[1] = targetItem.gameObject;
                                }
                                else if (rb.GetComponent<Item>() is Item i)
                                {
                                    Debug.Log("Item");
                                    ItemPull(i, targetItem.transform.position);
                                    ItemPull(targetItem, i.transform.position);
                                    line.SetPosition(0, i.transform.position);
                                    line.SetPosition(1, targetItem.transform.position);
                                    linepositions[0] = i.gameObject;
                                    linepositions[1] = targetItem.gameObject;
                                }
                                targetItem = null;
                            }
                            targeting = false;
                            used = true;
                        }
                    }
                }
            }
            void Update()
            {
                if (used && linepositions[0] != null && line != null)
                {
                    line.SetPosition(0, linepositions[0].transform.position);
                    line.SetPosition(1, linepositions[1].transform.position);
                } 
            }
        }
    }
}
