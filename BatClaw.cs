
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    public class BatClaw : ItemModule
    {
        public bool killOnHang;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<BatClawMono>();
        }
    }

    public class BatClawMono : MonoBehaviour
    {
        private Item item;
        private BatClaw module;

        bool firstTap;
        bool secondTap;
        bool Timer;
        static float tapTimerMax = 0.3f;
        float tapTimer = tapTimerMax;
        private GameObject clawMesh;
        private Animation clawAnim;

        private LineRenderer line;
        private Joint joint;
        private Transform firePos;
        private Creature grabbedCreature;
        private GameObject Pos;

        public void Start()
        {
            item = GetComponent<Item>();
            firstTap = false;
            secondTap = false;
            Timer = false;
            module = item.data.GetModule<BatClaw>();
            firePos = item.GetCustomReference("FirePos", true);
            clawMesh = item.GetCustomReference("Claw").gameObject;
            clawAnim = clawMesh.GetComponent<Animation>();
            LineSetUp();
            item.OnUngrabEvent += ItemOnOnUngrabEvent;
            item.OnHeldActionEvent += ItemOnOnHeldActionEvent;
        }

        private void ItemOnOnUngrabEvent(Handle handle, RagdollHand ragdollhand, bool throwing)
        {
            LineDestroy();
        }
        private void LineSetUp()
        {
            var game = new GameObject();
            game.transform.parent = item.transform;
            game.transform.localPosition = Vector3.zero;
            game.transform.localEulerAngles = Vector3.zero;
            line = game.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("ThunderRoad/Lit"));
            line.startWidth = 0.01f;
            line.endWidth = 0.01f;
            line.material.SetColor("_BaseColor", Color.black);
            line.gameObject.SetActive(false);
        }

        public IEnumerator Claw(Creature creature)
        {
            clawAnim.Play("ClawOpen");
            yield return new WaitForSeconds(0.14f);
            clawMesh.SetActive(false);
            line.gameObject.SetActive(true);
            if (creature.isPlayer) yield break;
            grabbedCreature = creature;
        }
        private void ItemOnOnHeldActionEvent(RagdollHand ragdollhand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.UseStart)
            {
                if (Physics.Raycast(firePos.position, firePos.forward, out RaycastHit hit, 250f))
                {
                    Catalog.GetData<EffectData>("GrappleStart").Spawn(item.transform).Play();

                    if (hit.transform.GetComponent<Rigidbody>() is Rigidbody rb)
                    {
                        if (rb.transform.GetComponentInParent<Creature>() is Creature creature)
                        {
                            StartCoroutine(Claw(creature));
                        }

                    }
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



        public void LineDestroy()
        {
            if (grabbedCreature != null)
            {
                clawAnim.Play("ClawIdle");
                Catalog.GetData<EffectData>("GrappleStop").Spawn(item.transform).Play();
            }
            grabbedCreature = null;
            line.gameObject.SetActive(false);
            clawMesh.gameObject.SetActive(true);
        }

        private GameObject FloatPointSetUp(Side side, RaycastHit hit)
        {
            var floatPoint = new GameObject();
            var rb = floatPoint.AddComponent<Rigidbody>();
            rb.useGravity = false;
            grabbedCreature.ragdoll.SetState(Ragdoll.State.Destabilized);
            floatPoint.transform.position = grabbedCreature.GetFoot(side).transform.position;
            var joint = grabbedCreature.GetFoot(side).gameObject.AddComponent<SpringJoint>();
            joint.connectedBody = rb;
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = new Vector3(0, 0, 0f);
            joint.spring = 2500f;
            joint.damper = 150f;
            joint.maxDistance = Vector3.Distance(hit.point, grabbedCreature.GetFoot(side).transform.position) / 8f;
            floatPoint.transform.position =
                Vector3.Lerp(hit.point, grabbedCreature.GetFoot(side).transform.position, 0.15f);
            floatPoint.AddComponent<FixedJoint>();
            return floatPoint;
        }
        public void CreatureHang(RaycastHit hit)
        {
            var point1 = FloatPointSetUp(Side.Right, hit);
            var point2 = FloatPointSetUp(Side.Left, hit);
            if(module.killOnHang) grabbedCreature.Kill();
            grabbedCreature.gameObject.AddComponent<HungCreature>().SetUp(point1, point2, hit);
            LineDestroy();
        }

        public IEnumerator CreaturePull(Creature creature)
        {
            creature.ragdoll.SetState(Ragdoll.State.Destabilized);
            while (Vector3.Distance(creature.ragdoll.rootPart.transform.position, item.transform.position) > 0.5f)
            {
                foreach (RagdollPart part in creature.ragdoll.parts)
                {
                    var dir = ((item.transform.position + new Vector3(0, 0.5f, 0)) - part.transform.position).normalized;
                    part.physicBody.AddForce(dir * 7.5f, ForceMode.Force);
                    
                }
                yield return Yielders.EndOfFrame;
            }
            LineDestroy();
        }
        public void Update()
        {
            if (grabbedCreature != null)
            {
                line.SetPosition(0, firePos.position);
                line.SetPosition(1, grabbedCreature.ragdoll.targetPart.transform.position);
            }

            if (item.mainHandler != null)
            {
                if (grabbedCreature != null)
                {
                    
                    if ((item.physicBody.velocity - Player.currentCreature.currentLocomotion.rb.velocity).sqrMagnitude >= 40)
                    {
                        grabbedCreature.ragdoll.SetState(Ragdoll.State.Destabilized);
                        foreach (RagdollPart part in grabbedCreature.ragdoll.parts)
                        {
                            part.physicBody.AddForce((item.transform.position - part.transform.position).normalized * (5 * item.physicBody.velocity.magnitude), ForceMode.Impulse);
                            LineDestroy();
                        }
                    }
                }
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
                            LineDestroy();
                        }

                        else if (firstTap && item.mainHandler.playerHand.controlHand.alternateUsePressed)
                        {
                            if (grabbedCreature != null && grabbedCreature.gameObject.GetComponent<HungCreature>() == null)
                            {
                                if (Physics.Raycast(firePos.position, firePos.forward, out RaycastHit hit, 5f,
                                        LayerMask.GetMask("Default", "MovingItem", "DroppedItem", "ItemAndRagdollOnly",
                                            "NPC", "Ragdoll", "Item", "PlayerLocomotionObject"),
                                        QueryTriggerInteraction.Ignore))
                                {
                                    Debug.Log("penis");
                                    CreatureHang(hit);
                                }
                            }
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
        }
    }

    public class HungCreature : MonoBehaviour
    {
        private Creature creature;
        private LineRenderer line;
        private Vector3 point;
        private GameObject[] points = new GameObject[2];

        public void SetUp(GameObject floatPoint, GameObject floatPoint2, RaycastHit hit)
        {
            points[0] = floatPoint;
            points[1] = floatPoint2;
            point = hit.point;
        }
        public void Start()
        {
            creature = GetComponent<Creature>();
            line = gameObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("ThunderRoad/Lit"));
            line.startWidth = 0.01f;
            line.endWidth = 0.01f;
            line.material.SetColor("_BaseColor", Color.black);
            line.SetPosition(0, creature.footLeft.transform.position);
            line.SetPosition(1, point);
            creature.OnDespawnEvent += CreatureOnOnDespawnEvent;
        }

        private void CreatureOnOnDespawnEvent(EventTime eventtime)
        {
            if (eventtime == EventTime.OnEnd)
            {
                if (points != null && points.Length == 2)
                {
                    Destroy(points[0]);
                    Destroy(points[1]);
                }
                if (creature.GetFoot(Side.Left).gameObject.GetComponent<SpringJoint>() == null ||
                    creature.GetFoot(Side.Right).gameObject.GetComponent<SpringJoint>() == null) return;
                Destroy(creature.GetFoot(Side.Left).gameObject.GetComponent<SpringJoint>());
                Destroy(creature.GetFoot(Side.Right).gameObject.GetComponent<SpringJoint>());
                Destroy(line);
                Destroy(this);
            }
        }

        private void Update()
        {
            if (line != null && points != null)
            {
                line.SetPosition(0, creature.footLeft.transform.position);
                line.SetPosition(1, point);
            }

            if (creature != null && !creature.isKilled && !(creature.ragdoll.state == Ragdoll.State.Destabilized))
            {
                creature.ragdoll.state = Ragdoll.State.Destabilized;
            }
        }
    }
}