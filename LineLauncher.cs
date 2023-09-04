using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    class LineLauncher : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<LineLauncherMono>();       }
    }
    public class LineLauncherMono : MonoBehaviour
    {
        Item item;
        public Vector3 ForwardHit;
        public Vector3 BackwardHit;
        Vector3 forcePos;
        public bool LineCreated;
        public bool lookingForward;
        public bool triggerHeld;
        float lineTravelled;
        LineRenderer FrontLine;
        LineRenderer BackLine;
        GameObject FrontCanister;
        GameObject BackCanister;
        Animation anim;
        EffectData lineLoop;
        EffectInstance lineLoopIns;
        float distance;
        bool firstTap;
        bool secondTap;
        bool Timer;
        static float tapTimerMax = 0.2f;
        float tapTimer = tapTimerMax;
        bool forward;
        public void Start()
        {
            item = GetComponent<Item>();
            forward = true; 
            firstTap = false;
            secondTap = false;
            Timer = false;
            anim = item.GetCustomReference("anim").GetComponent<Animation>();
            FrontCanister = item.GetCustomReference("FrontCanister").gameObject;
            BackCanister = item.GetCustomReference("BackCanister").gameObject;
            var game = new GameObject();
            game.transform.position = item.transform.position;
            game.transform.rotation = item.transform.rotation;
            game.transform.parent = item.transform;
            var game2 = new GameObject();
            game2.transform.position = item.transform.position;
            game2.transform.rotation = item.transform.rotation;
            game2.transform.parent = item.transform;
            FrontLine = game2.AddComponent<LineRenderer>();
            BackLine = game.AddComponent<LineRenderer>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            LineCreated = false;
            triggerHeld = false;
            FrontLine.material = new Material(Shader.Find("ThunderRoad/Lit"));
            FrontLine.startWidth = 0.01f;
            FrontLine.endWidth = 0.01f;
            FrontLine.material.SetColor("_BaseColor", Color.black);
            BackLine.material = new Material(Shader.Find("ThunderRoad/Lit"));
            BackLine.startWidth = 0.01f;
            BackLine.endWidth = 0.01f;
            BackLine.material.SetColor("_BaseColor", Color.black);
            lineLoop = Catalog.GetData<EffectData>("LineLauncherLoop");
            lineLoopIns = lineLoop.Spawn(item.transform);

        }
        public Vector3 RaycastPoint(Vector3 position, Vector3 direction)
        {
            if(Physics.Raycast(position, direction, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Default", "MovingItem", "DroppedItem", "ItemAndRagdollOnly", "NPC", "Ragdoll", "Item", "PlayerLocomotionObject"), QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponentInParent<Item>() || hit.collider.GetComponentInParent<Creature>()) return Vector3.zero;
                else
                {
                    return hit.point;
                }
            }
            return Vector3.zero;
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if(action == Interactable.Action.AlternateUseStart)
            {
                if (!LineCreated)
                {
                    ForwardHit = RaycastPoint(FrontCanister.transform.position, FrontCanister.transform.forward);
                    BackwardHit = RaycastPoint(BackCanister.transform.position, BackCanister.transform.forward);
                    if(ForwardHit != Vector3.zero && BackwardHit != Vector3.zero)
                    {
                        forward = true;
                        FrontLine.gameObject.SetActive(true);
                        BackLine.gameObject.SetActive(true);
                        FrontLine.SetPosition(0, ForwardHit);
                        FrontLine.SetPosition(1, FrontCanister.transform.position);
                        BackLine.SetPosition(0, BackwardHit);
                        BackLine.SetPosition(1, BackCanister.transform.position);
                        Debug.Log($"{ForwardHit}");
                        item.physicBody.isKinematic = true;
                        LineCreated = true;
                        anim.Play("LineLauncherExtend");
                        Catalog.GetData<EffectData>("LineLauncherShoot").Spawn(item.transform).Play();
                    }
                }
                else
                {
                    FrontLine.gameObject.SetActive(false);
                    BackLine.gameObject.SetActive(false);
                    item.physicBody.isKinematic = false;
                    BackwardHit = Vector3.zero;
                    ForwardHit = Vector3.zero;
                    LineCreated = false;
                    anim.Play("LineLauncherFold");
                    triggerHeld = false;
                    lineLoopIns?.Stop();
                }
            }
            if(action == Interactable.Action.UseStart && !firstTap)
            {
                firstTap = true;
                Timer = true;
            }
            else if(action == Interactable.Action.UseStart && firstTap && Timer)
            {
                secondTap = true;
                tapTimer = 0f;
            }
            /*if(action == Interactable.Action.UseStart && !triggerHeld && LineCreated)
            {
                forcePos = (ForwardHit - item.transform.position).normalized;
                distance = Vector3.Distance(BackwardHit, item.transform.position) / Vector3.Distance(ForwardHit, BackwardHit);
                lineLoopIns.Play();
                triggerHeld = true;
            }*/
            if(action == Interactable.Action.UseStop && triggerHeld && LineCreated)
            {
                lineLoopIns.Stop();
                triggerHeld = false;
            }
            /*if(action == Interactable.Action.UseStart && LineCreated)
            {
                triggerHeld = true;
            }
            else if (action == Interactable.Action.UseStop && triggerHeld)
            {
                triggerHeld = false;
            }*/
        }
        public void CreateLine(LineRenderer Line)
        {
            if (triggerHeld)
            {
                item.physicBody.AddForce(forcePos * 30, ForceMode.Impulse);
                FrontLine.SetPosition(1, FrontCanister.transform.position);
                BackLine.SetPosition(1, BackCanister.transform.position);
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


                        if (firstTap && secondTap)
                        {
                            Debug.Log($"Double Tap");
                            forward = !forward;
                            item.mainHandler.playerHand.controlHand.HapticShort(5f);
                        }

                        else if (firstTap && item.mainHandler.playerHand.controlHand.usePressed)
                        {
                            forcePos = (ForwardHit - item.transform.position).normalized;
                            distance = Vector3.Distance(BackwardHit, item.transform.position) / Vector3.Distance(ForwardHit, BackwardHit);
                            lineLoopIns.Play();
                            triggerHeld = true;
                            item.mainHandler.playerHand.controlHand.HapticShort(5f);
                        }

                        else if (firstTap && !secondTap)
                        {
                            Debug.Log($"Tap");
                            item.mainHandler.playerHand.controlHand.HapticShort(5f);
                        }

                        firstTap = false;
                        secondTap = false;
                    }
                }
            }
            if (triggerHeld)
            {
                if (forward) item.transform.position = Vector3.Lerp(BackwardHit, ForwardHit, (distance += 0.003f));
                else item.transform.position = Vector3.Lerp(BackwardHit, ForwardHit, (distance -= 0.003f));
                FrontLine.SetPosition(1, FrontCanister.transform.position);
                BackLine.SetPosition(1, BackCanister.transform.position);
            }
        }
    }
}
