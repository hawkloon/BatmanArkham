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
    class WeaponPod : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<WeaponPodMono>();
        }
    }
    public class WeaponPodMono : MonoBehaviour
    {
        Item item;
        bool isMultiple;
        bool isOpened;
        Animation animation;
        public void Start()
        {
            item = GetComponent<Item>();
            animation = GetComponent<Animation>();
            item.mainCollisionHandler.OnTriggerEnterEvent += MainCollisionHandler_OnTriggerEnterEvent;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            isOpened = false;
        }
        private IEnumerator WeaponReveal()
        {
            isOpened = true;
            Catalog.GetData<EffectData>("PodScanner").Spawn(item.transform).Play();
            yield return Yielders.ForSeconds(1f);
            animation.Play("Open");
        }
        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            item.physicBody.isKinematic = true;
        }

        private void MainCollisionHandler_OnTriggerEnterEvent(Collider other)
        {
            if (other.GetComponentInParent<RagdollPart>() is RagdollPart part && !isOpened)
            {
                if (part.type == RagdollPart.Type.LeftHand || part.type == RagdollPart.Type.RightHand)
                {
                    StartCoroutine(WeaponReveal());
                }
            }
            
        }
    }
}
