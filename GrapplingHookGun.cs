using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    public class GrapplingHookGun : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<GrappleGunComponent>();
        }
    }

    public class GrappleGunComponent : MonoBehaviour
    {
        Item item;
        Transform firePos;
        GameObject Pos;
        LineRenderer Line;
        SpringJoint Joint;

        bool awayMode;

        bool firstTap;
        bool secondTap;
        bool Timer;
        static float tapTimerMax = 0.3f;
        float tapTimer = tapTimerMax;

        public void Awake()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;
            firePos = item.GetCustomReference("FirePos", true);
        }

        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            if (Joint != null)
            {
                MonoBehaviour.Destroy(Pos);
                MonoBehaviour.Destroy(Joint);
                MonoBehaviour.Destroy(Line);
            }
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.UseStart)
            {
                if (Physics.Raycast(firePos.position, firePos.forward, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Default", "MovingItem", "DroppedItem", "ItemAndRagdollOnly", "NPC", "Ragdoll", "Item", "PlayerLocomotionObject"), QueryTriggerInteraction.Ignore))
                {
                    Catalog.GetData<EffectData>("GrappleStart").Spawn(item.transform).Play();
                    Joint = Player.local.locomotion.gameObject.AddComponent<SpringJoint>();
                    Line = firePos.gameObject.AddComponent<LineRenderer>();

                    Pos = new GameObject();
                    Pos.transform.position = hit.point;
                    Pos.transform.parent = hit.transform;

                    if (hit.transform.GetComponent<Rigidbody>() is Rigidbody rb)
                        CreateWeb(hit.distance, hit.point, rb);

                    if (!hit.transform.GetComponent<Rigidbody>())
                        CreateWeb(hit.distance, hit.point, null);
                }
            }

            if (action == Interactable.Action.UseStop)
            {
                if (Joint != null)
                {
                    MonoBehaviour.Destroy(Pos);
                    MonoBehaviour.Destroy(Joint);
                    MonoBehaviour.Destroy(Line);
                    Catalog.GetData<EffectData>("GrappleStop").Spawn(item.transform).Play();
                }
            }

            if (action == Interactable.Action.AlternateUseStart && !firstTap)
            {
                firstTap = true;
                Timer = true;
            }
            else if (action == Interactable.Action.AlternateUseStart && firstTap && Timer)
            {
                secondTap = true;
                tapTimer = 0;
            }
        }

        public void CreateWeb(float l, Vector3 p, Rigidbody connection)
        {
            Line.SetPosition(0, firePos.position);
            Line.SetPosition(1, p);
            Line.material = new Material(Shader.Find("ThunderRoad/Lit"));
            Line.startWidth = 0.01f;
            Line.endWidth = 0.01f;
            Line.material.SetColor("_BaseColor", Color.black);

            Joint.autoConfigureConnectedAnchor = false;
            if (connection != null)
            {
                Joint.connectedBody = connection;
                Joint.connectedAnchor = connection.transform.InverseTransformPoint(p);
                if (Joint.connectedBody != null && Joint.GetComponentInParent<Creature>() != null)
                {
                    Creature creature = Joint.connectedBody.GetComponentInParent<Creature>();
                    if (creature.isKilled || creature.ragdoll.state == Ragdoll.State.Destabilized) return;
                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                }
            }
            else
            {
                Joint.connectedAnchor = p;
            }
            Joint.spring = 5000f;
            Joint.damper = 500f;
            Joint.connectedMassScale = 10f;
            Joint.minDistance = 0.01f;
            Joint.maxDistance = l;
        }

        public void Update()
        {
            if (item.mainHandler != null && Timer)
            {
                if (tapTimer >= 0)
                {
                    tapTimer -= Time.deltaTime;
                }
                else
                {
                    Timer = false;
                    tapTimer = tapTimerMax;


                    if (firstTap && secondTap)
                    {
                        awayMode = !awayMode;
                        item.mainHandler.playerHand.controlHand.HapticShort(5f);
                    }

                    firstTap = false;
                    secondTap = false;
                }
            }

            if (Joint != null)
            {
                Line.SetPosition(0, firePos.position);
                Line.SetPosition(1, Pos.transform.position);

                if (item.IsHanded(Side.Right) && PlayerControl.handRight.alternateUsePressed || item.IsHanded(Side.Left) && PlayerControl.handLeft.alternateUsePressed)
                {
                    if ((Joint.maxDistance - Time.deltaTime * 8) > Joint.minDistance && !awayMode)
                    {
                        Joint.maxDistance -= Time.deltaTime * 35f;
                    }

                    if (awayMode)
                    {
                        Joint.maxDistance += Time.deltaTime * 35f;
                    }
                }
            }
        }
    }
}
