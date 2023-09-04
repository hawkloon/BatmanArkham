using System;
using System.Collections;
using System.ComponentModel;
using System.Resources;
using System.Security.Cryptography;
using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    public class GrapnelGun : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<GrapnelMono>();
        }
    }

    public class GrapnelMono : MonoBehaviour
    {
        private Item item;
        private LineRenderer line;
        private Transform firePos;
        private static float GrapnelForceMin = 15f;
        public Renderer mesh;
        private float GrapnelForce = GrapnelForceMin;
        private bool GrapnelBoost;
        public float matCharge;
        private SpringJoint joint;
        private GameObject clawMesh;
        private Animation clawAnim;
        private Coroutine activeco;
        public Color[] grappleColor = new Color[2];
        public void Start()
        {
            item = GetComponent<Item>();
            firePos = item.GetCustomReference("FirePos", true);
            matCharge = 0;
            mesh = item.GetCustomReference<SkinnedMeshRenderer>("Mesh");
            clawMesh = item.GetCustomReference("Claw").gameObject;
            clawAnim = clawMesh.GetComponent<Animation>();
            LineSetUp();
            grappleColor[0] = mesh.materials[2].GetColor("_EColor");
            grappleColor[1] = new Color(4f, 4f, 4f, 1f);

            item.OnUngrabEvent += ItemOnOnUngrabEvent;
            item.OnHeldActionEvent += ItemOnOnHeldActionEvent;
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
        private void ItemOnOnUngrabEvent(Handle handle, RagdollHand ragdollhand, bool throwing)
        {
            LineDestroy();
        }

        public IEnumerator ChargeCoroutine(bool Cool)
        {
            var mat = mesh.materials[2];
            if (Cool)
            {
                while (matCharge > 0)
                {
                    matCharge -= 0.001f;
                    var e = mat.GetColor(("_EmissionColor"));
                }
            }
            while (matCharge < 3)
            {
                matCharge += 0.001f;
                var e =mat.GetColor("_EmissionColor");
                mat.SetColor("_EmissionColor", e * matCharge);
                yield return Yielders.EndOfFrame;
            }
        }

        public void Charge()
        {
            return;
        }
        public IEnumerator Grapple(RaycastHit hit)
        {
            clawAnim.Play("ClawOpen");
            Catalog.GetData<EffectData>("GrappleStart").Spawn(item.transform).Play();
            yield return new WaitForSeconds(0.14f);
            clawMesh.SetActive(false);
            line.gameObject.SetActive(true);
            joint = Player.local.locomotion.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = hit.point;
            joint.spring = 3000f;
            joint.damper = 500f;
            joint.connectedMassScale = 10f;
            joint.maxDistance = Vector3.Distance(item.transform.position, hit.point);
            var mat = mesh.materials[2];
            while (Vector3.Distance(Player.currentCreature.ragdoll.targetPart.transform.position, hit.point) > 1.5f)
            {
                line.SetPosition(0, firePos.position);
                line.SetPosition(1, hit.point);
                matCharge += 0.01f;
                mat.SetColor("_EColor", Color.Lerp(grappleColor[0], grappleColor[1], matCharge));
                mesh.materials[2] = mat;
                joint.maxDistance -= Time.deltaTime * GrapnelForce;
                if (Vector3.Distance(Player.currentCreature.ragdoll.targetPart.transform.position, hit.point) < 10)
                {
                    Player.local.locomotion.rb.AddForce(Vector3.up * 25, ForceMode.Force);
                }

                yield return Yielders.EndOfFrame;
            }
            LineDestroy();
        }

        public IEnumerator Cooldown()
        {
            var mat = mesh.materials[2];
            while (matCharge > 0)
            {
                matCharge -= 0.01f;
                mat.SetColor("_EColor", Color.Lerp(grappleColor[0], grappleColor[1], matCharge));
                mesh.materials[2] = mat;
                yield return Yielders.EndOfFrame;
            }
        }
        private void LocomotionOnOnGroundEvent(Vector3 groundpoint, Vector3 velocity, Collider groundcollider)
        {
            Player.fallDamage = true;
            Player.local.locomotion.OnGroundEvent -= LocomotionOnOnGroundEvent;
        }

        private void ItemOnOnHeldActionEvent(RagdollHand ragdollhand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.UseStart)
            {
                if (Physics.SphereCast(firePos.position, 0.3f, firePos.forward, out RaycastHit hit, 250f))
                {
                    if(hit.collider.GetComponentInParent<Item>() != null || hit.collider.GetComponentInParent<Creature>() != null) return;
                    activeco = StartCoroutine(Grapple(hit));
                }
            }

            if (action == Interactable.Action.UseStop && activeco != null)
            {
                StopCoroutine(activeco);
                activeco = null;
                LineDestroy();
                StartCoroutine(Cooldown());
            }

            if (action == Interactable.Action.AlternateUseStart && activeco != null)
            {
                GrapnelBoost = true;
            }
        }

        private void LineDestroy()
        {
            if (GrapnelBoost)
            {
                
                Player.local.locomotion.rb.AddForce(Vector3.up * 40, ForceMode.Impulse);
                Player.fallDamage = false;
                Player.local.locomotion.OnGroundEvent += LocomotionOnOnGroundEvent;
            }
            GrapnelBoost = false;
            clawMesh.gameObject.SetActive(true);
            if (joint != null)
            {
                clawAnim.Play("ClawIdle");
                Catalog.GetData<EffectData>("GrappleStop").Spawn(item.transform).Play();
            }
            if(joint != null) Destroy(joint);
            line.gameObject.SetActive(false);
        }
    }
}