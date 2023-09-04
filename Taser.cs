using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    public class Taser : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<TaserMono>();
        }
    }

    public class TaserMono : MonoBehaviour
    {
        private Item item;
        private Creature tasedCreature;
        private EffectData effect;
        private LineRenderer line;

        public void Start()
        {
            item = GetComponent<Item>();
            effect = Catalog.GetData<EffectData>("ImbueLightningRagdoll");
            LineSetUp();
            item.OnHeldActionEvent += ItemOnOnHeldActionEvent;
        }
        private void LineSetUp()
        {
            var game = new GameObject();
            game.transform.parent = item.transform;
            game.transform.localPosition = Vector3.zero;
            game.transform.localEulerAngles = Vector3.zero;
            line = game.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            line.startWidth = 0.01f;
            line.endWidth = 0.01f;
            line.material.color = Color.black;
            line.gameObject.SetActive(false);
        }
        private void ItemOnOnHeldActionEvent(RagdollHand ragdollhand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.UseStart)
            {
                if(Physics.SphereCast(item.flyDirRef.transform.position, 0.3f, item.flyDirRef.transform.forward, out RaycastHit hit, 150f))
                {
                    if (hit.collider.GetComponentInParent<Creature>() is Creature creature)
                    {
                        line.gameObject.SetActive(true);
                        tasedCreature = creature;
                        line.SetPosition(0, item.flyDirRef.position);
                        line.SetPosition(1, creature.ragdoll.targetPart.transform.position);
                    }
                }
            }

            if (action == Interactable.Action.UseStop && tasedCreature != null)
            {
                StopTase();
            }
        }

        void StopTase()
        {
            tasedCreature?.StopShock();
            line.gameObject.SetActive(false);
        }
        void Update()
        {
            if (tasedCreature != null && line != null)
            {
                line.SetPosition(0, item.flyDirRef.position);
                line.SetPosition(1, tasedCreature.ragdoll.targetPart.transform.position);
                tasedCreature.TryElectrocute(60, 15, true, true, effect);
            }
        }
    }
}