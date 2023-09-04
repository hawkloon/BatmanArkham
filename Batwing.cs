using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    public class Batwing : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<BatwingMono>();
        }
    }

    public class BatwingMono : MonoBehaviour
    {
        private Item item;
        private Animation _animation;

        public void Start()
        {
            item = GetComponent<Item>();
            _animation = GetComponent<Animation>();
            item.OnHeldActionEvent += ItemOnOnHeldActionEvent;
        }

        private void ItemOnOnHeldActionEvent(RagdollHand ragdollhand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                _animation.Play("Enter");
            }
        }
    }
}