using System.Collections;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Animations;

namespace XESuitPod
{
    public class RECGun : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<RECGunMono>();
        }
    }

    public class RECGunMono : MonoBehaviour
    {
        private Item item;
        private Handle handle;
        private Transform head;
        private AudioSource audio;
        private bool cooldown;

        public void Start()
        {
            item = GetComponent<Item>();
            cooldown = false;
            handle = item.GetCustomReference<Handle>("MainHandle");
            head = item.GetCustomReference("Head"); 
            audio = item.GetCustomReference<AudioSource>("Audio");
            handle.OnHeldActionEvent += HandleOnOnHeldActionEvent;
        }

        private void HandleOnOnHeldActionEvent(RagdollHand ragdollhand, Interactable.Action action)
        {
            if (action == Interactable.Action.UseStart && !cooldown)
            {
                Catalog.GetData<ItemData>("RECProj").SpawnAsync(Proj =>
                {
                    Proj.transform.position = item.flyDirRef.position;
                    Proj.transform.rotation = item.flyDirRef.rotation;
                    Proj.physicBody.useGravity = false;
                    Proj.physicBody.AddForce(item.flyDirRef.forward * 50f, ForceMode.Impulse);
                    Proj.Throw();
                    Proj.IgnoreObjectCollision(item);
                    Proj.gameObject.AddComponent<RECProj>();
                });
                StartCoroutine(Reload());
                audio.Play();
            }
        }

        private IEnumerator Reload()
        {
            head.transform.localScale = Vector3.zero;
            cooldown = true;
            yield return Yielders.ForSeconds(1f);
            head.transform.localScale = Vector3.one;
            cooldown = false;
        }


        public class RECProj : MonoBehaviour
        {
            private Item item;
            private EffectData effect;

            public void Start()
            {
                item = GetComponent<Item>();
                /*var e =Catalog.GetData<EffectData>("SpellOrbLightning").Spawn(item.transform);
                e.SetIntensity(10f);
                e.Play();
                */
                effect = Catalog.GetData<EffectData>("ImbueLightningRagdoll");
                item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandlerOnOnCollisionStartEvent;
            }

            private void MainCollisionHandlerOnOnCollisionStartEvent(CollisionInstance collisioninstance)
            {
                if (collisioninstance.targetCollider.GetComponentInParent<Creature>() is Creature creature)
                {
                    if (creature.isPlayer) return;
                    if (!creature.isKilled) creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                    foreach (RagdollPart part in creature.ragdoll.parts)
                    {
                        part.physicBody.AddForce(collisioninstance.impactVelocity.normalized * 15, ForceMode.Impulse);
                    }

                    creature.TryElectrocute(50, 10, true, true, effect);
                }

                item.Despawn();
            }
        }
    }
}
