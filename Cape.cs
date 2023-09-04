using System;
using System.Collections.Generic;
using HawkUtils;
using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    public class Cape : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<CapeMono>();
        }
    }

    public class CapeMono : MonoBehaviour
    {
        private Item item;
        private Creature creature;
        public static CapeMono local;
        private EffectData effect;
        public GameObject physicsCape;
        public GameObject glidingCape;
        bool isGliding;

        public void Start()
        {
            item = GetComponent<Item>();
            creature = Player.currentCreature;
            if(local != null) item.Despawn();
            local = this;
            isGliding = false;
            physicsCape = item.GetCustomReference("physicsCape").gameObject;
            var cloth = physicsCape.GetComponentInChildren<Cloth>();
            ColliderPopulate(cloth);
            glidingCape = item.GetCustomReference("glidingCape").gameObject;
            effect = Catalog.GetData<EffectData>("CapeSound");
            item.disallowDespawn = true;
            item.isCulled = false;
            item.OnCullEvent += Item_OnCullEvent;
            item.physicBody.isKinematic = true;
            item.transform.SetParent(Player.currentCreature.animator.GetBoneTransform(HumanBodyBones.Spine));
            item.transform.localPosition = new Vector3(BatmanArkhamLoader.CapeHeight, 0f, -0.16842455f);
            item.transform.localEulerAngles = new Vector3(6.18985905e-06f, 6.78921747f, 90.0000076f);
            glidingCape.SetActive(false);
        }

        private void Item_OnCullEvent(bool culled)
        {
            item.isCulled = false;
        }

        public List<string> WhitelistedParts = new List<string>()
        {
            "LeftLeg",
            "RightLeg",
            "LeftUpLeg",
            "RightUpLeg",
            "Hips",
            "Spine",
            "Head"
        };

        private void OnDestroy()
        {
            if (local == this)
            {
                local = null;
            }
        }

        public void ColliderPopulate(Cloth cloth)
        {
            var i = 0;
            Collider[] colliders = new Collider[52];
            foreach (RagdollPart part in creature.ragdoll.parts)
            {
                if (!WhitelistedParts.Contains(part.name)) continue;
                foreach (Collider collider in part.colliderGroup.colliders)
                {
                    colliders[i] = collider;
                    i++;
                }
            }

            cloth.capsuleColliders = colliders as CapsuleCollider[];
            cloth.collisionMassScale = 0.3f;
        }

        void Update()
        {
            if (item.transform.localPosition != new Vector3(BatmanArkhamLoader.CapeHeight, 0f, -0.16842455f))
            {
                item.transform.localPosition = new Vector3(BatmanArkhamLoader.CapeHeight, 0f, -0.16842455f);
                Debug.Log("CORRECTING CAPE POSITION");
            }
            if (!isGliding)
            {
                if (Vector3.Dot(creature.GetHand(Side.Left).PointDir, -creature.transform.right) >= 0.75 &&
                    Vector3.Dot(creature.GetHand(Side.Left).PointDir, creature.GetHand(Side.Right).PointDir) <
                    -0.7)
                {
                    isGliding = true;
                    physicsCape.SetActive(false);
                    glidingCape.SetActive(true);
                    effect.Spawn(item.transform).Play();
                    Player.local.locomotion.SetPhysicModifier(this, 0.5f, -1, 1.3f);
                }
            }
            else
            {
               if (!Player.local.locomotion.isGrounded && isGliding)
                {
                    creature.currentLocomotion.rb.velocity *= 0.96f;

                    if (Player.local.creature && Player.local.locomotion)
                    {
                        Player.local.locomotion.rb.AddForce(Player.local.creature.transform.forward * BatmanArkhamLoader.GlideSpeed, ForceMode.Acceleration);
                    }
                }
                if (Vector3.Dot(creature.GetHand(Side.Left).PointDir, -creature.transform.right) <= 0.6 &&
                    Vector3.Dot(creature.GetHand(Side.Left).PointDir, creature.GetHand(Side.Right).PointDir) > -0.3f)
                {
                    isGliding = false;
                    physicsCape.SetActive(true);
                    glidingCape.SetActive(false);
                    Player.local.locomotion.RemovePhysicModifier(this);
                    effect.Spawn(item.transform).Play();
                    /*creature.currentLocomotion.rb.drag = initDrag;
                    creature.currentLocomotion.rb.angularDrag = initAngular;*/
                }
            }
        }
        
    }
}