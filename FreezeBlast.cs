using System;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    public class FreezeBlast : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<FreezeBlastMono>();
        }
    }

    public class FreezeBlastMono : MonoBehaviour
    {
        private Item item;

        public void Start()
        {
            item = GetComponent<Item>();
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandlerOnOnCollisionStartEvent;
        }

        private void MainCollisionHandlerOnOnCollisionStartEvent(CollisionInstance collisioninstance)
        {
            if (collisioninstance.intensity >= 0.75f)
            {
                if (collisioninstance.targetCollider.GetComponentInParent<Creature>() is Creature creature)
                {
                    if (creature.isPlayer || creature.isKilled) return;
                    creature.gameObject.AddComponent<FreezeBlastCreature>();
                    item.Despawn();
                }
            }
        }
    }


    public class FreezeBlastCreature : MonoBehaviour
    {
        private Creature creature;
        private bool frozen;
        private Color BaseColor;
        private GameObject freezeVFX;
        public void Start()
        {
            creature = GetComponent<Creature>();
            frozen = false;
            BaseColor = creature.GetColor(Creature.ColorModifier.Skin);
            Catalog.InstantiateAsync("FreezeBlastVFX", Vector3.zero, Quaternion.identity, 
                creature.animator.GetBoneTransform(HumanBodyBones.Spine), VFX =>
                {
                    VFX.transform.localPosition = new Vector3(-0.1305129322f, 0, 0.01177100898f);
                    VFX.transform.localEulerAngles = new Vector3(3.41509462e-06f,276.789215f,90f);
                    freezeVFX = VFX;
                }, "FreezeBlastVFX");
            Catalog.GetData<EffectData>("FreezeBlastExplode").Spawn(creature.transform).Play();
            StartCoroutine(ColorFreeze(0.3f, BaseColor, Color.cyan));
            ConstrainTarget();
            
        }


        public IEnumerator ColorFreeze(float maxT, Color StartingColor, Color EndColor)
        {
            float t = 0;    
            Debug.Log("Starting freeze");
            while (t < maxT)
            {
                var currentColor = Color.Lerp(StartingColor, EndColor, t);
                creature.SetColor(currentColor, Creature.ColorModifier.Skin, true);
                t += 0.01f;
                yield return 0;
            }
        }
        void ConstrainTarget()
        {
            creature.ragdoll.SetState(Ragdoll.State.Frozen);
            creature.brain.Stop();
            frozen = true;
            StartCoroutine(FreezeStop());
        }


        public IEnumerator FreezeStop()
        {
            yield return Yielders.ForSeconds(7.5f);
            frozen = false;
            Destroy(freezeVFX);
            StartCoroutine(ColorFreeze(1f, creature.GetColor(Creature.ColorModifier.Skin), BaseColor));
            if (!creature.isKilled)
            {
                creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                creature.brain.Load(creature.brain.instance.id);
                
            }
            else creature.ragdoll.SetState(Ragdoll.State.Inert);
            
        }


        void RagdollCheck(Ragdoll ragdoll)
        {
            if (ragdoll.state != Ragdoll.State.Frozen)
            {
                ragdoll.SetState(Ragdoll.State.Frozen);
            }
        }
        void Update()
        {
            if (frozen)
            {
                RagdollCheck(creature.ragdoll);
            }   
        }
    }
}