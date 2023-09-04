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
    class ExplosiveGel : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ExplosiveGelMono>();
        }
    }
    public class ExplosiveGelMono : MonoBehaviour
    {
        Item item;
        bool firstTap;
        bool secondTap;
        bool Timer;
        float radius = 100.0f;
        float power = 100.0f;
        static float tapTimerMax = 0.2f;
        float tapTimer = tapTimerMax;
        public List<Item> ExplosiveGelSprays = new List<Item>();
        public void Start()
        {
            item = GetComponent<Item>();
            firstTap = false;   
            secondTap = false;
            Timer = false;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.UseStart)
            {
                firstTap = true;
                Timer = true;
            }
            else if (action == Interactable.Action.AlternateUseStart && ExplosiveGelSprays != null)
            {
                Catalog.GetData<EffectData>("ExplosiveGelClick").Spawn(item.transform).Play();
                if(ExplosiveGelSprays.Count > 0 && ExplosiveGelSprays    != null)
                {
                    foreach (Item spray in ExplosiveGelSprays)
                    {
                        foreach (Collider collider in Physics.OverlapSphere(spray.transform.position, 3f))
                        {
                            Vector3 boomTrans = spray.transform.position;
                            if (collider.GetComponentInParent<Creature>() is Creature creature)
                            {
                                if (!creature.isPlayer)
                                {
                                    if (!creature.isKilled) creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                                    foreach (RagdollPart part in creature.ragdoll.parts)
                                    {
                                        part.physicBody.rigidBody.AddExplosionForce(power * part.physicBody.mass, boomTrans, radius);
                                    }
                                }
                            }
                            else
                            {
                                collider.attachedRigidbody?.AddExplosionForce(power * 20 * collider.attachedRigidbody.mass, boomTrans, radius);
                            }
                        }
                        var game = new GameObject()
                        {
                            transform =
                            {
                                position = spray.transform.position,
                                rotation = Quaternion.identity
                            }
                        };
                        Catalog.GetData<EffectData>("ExplosiveGelExplode").Spawn(game.transform).Play();
                        spray.Despawn();

                    }
                    ExplosiveGelSprays.Clear();
                }
            }
        }
        void Update()
        {
            if (item.mainHandler != null)
            {
                if (Timer)
                {
                    if (tapTimer >= 0)
                    {
                        tapTimer -= Time.deltaTime;
                    }
                    else
                    {
                        Timer = false;
                        tapTimer = tapTimerMax;

                        if (firstTap && item.mainHandler.playerHand.controlHand.usePressed)
                        {
                            Debug.Log($"Hold");
                            if (ExplosiveGelSprays.Count < 3)
                            {
                                if (Physics.Raycast(item.flyDirRef.transform.position, item.flyDirRef.transform.forward, out RaycastHit hit, 0.5f))
                                {
                                    Catalog.GetData<ItemData>("ExplosiveGelSpray").SpawnAsync(Spray =>
                                    {
                                        var pointObject = new GameObject("pointObject");
                                        pointObject.transform.SetPositionAndRotation(hit.point, hit.transform.rotation);
                                        Spray.transform.SetParent(pointObject.transform);
                                        Spray.transform.position = hit.point + (hit.normal/ 20);
                                        Spray.transform.rotation = Quaternion.LookRotation(-hit.normal);
                                        if (hit.rigidbody != null)
                                        {
                                            var Joint = Spray.gameObject.AddComponent<FixedJoint>();
                                            Joint.connectedBody = hit.rigidbody;
                                        }
                                        else
                                        {
                                            Spray.physicBody.isKinematic = true;
                                        }
                                        ExplosiveGelSprays.Add(Spray);
                                    });
                                    Catalog.GetData<EffectData>("ExplosiveGelSpraySound").Spawn(item.transform).Play();
                                }
                            }
                            item.mainHandler.playerHand.controlHand.HapticShort(5f);
                        }

                        else if (firstTap && !secondTap)
                        {

                        }

                        firstTap = false;
                        secondTap = false;
                    }
                }
            }
        }
    }
}
