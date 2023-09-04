using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;

namespace XESuitPod
{
    public class SmokePellet : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<SmokePelletMono>();
        }
    }
    public class SmokePelletMono : MonoBehaviour
    {
        Item item;
        public void Start()
        {
            item = GetComponent<Item>();
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
        }

        public void SmokeStart(CollisionInstance collision)
        {
            var game = new GameObject();
            game.transform.position = collision.contactPoint;
            game.transform.eulerAngles = new Vector3(-90, 0f, 0f);
            Catalog.GetData<EffectData>("SmokePelletFX").Spawn(transform).Play();
            game.AddComponent<SmokeGameObject>();
            item.Despawn();
        }
        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if(collisionInstance.intensity > 0.7) SmokeStart(collisionInstance);
        }
    }
    public class SmokeGameObject : MonoBehaviour
    {
        public float initHorizontal;
        public float initVertical;
        List<Creature> affectedCreatures = new List<Creature>();
        public bool initRan;
        public void Start()
        {
            initRan = false;
            Catalog.GetData<EffectData>("SmokePelletFX").Spawn(transform).Play();
            Debug.Log("Booger aids");
            foreach(Creature creature in Creature.allActive)
            {
                if(Vector3.Distance(creature.transform.position, transform.position) < 7.5)
                {
                    if (creature.isPlayer || creature.isKilled) continue;
                    var detection = creature.brain.instance.GetModule<BrainModuleDetection>();
                    detection.canHear = false;
                    initHorizontal = detection.sightDetectionHorizontalFov;
                    initVertical = detection.sightDetectionVerticalFov;
                    detection.sightDetectionHorizontalFov = 0f;
                    detection.sightDetectionVerticalFov = 0f;
                    detection.alertednessLevel = 0f;
                    creature.brain.currentTarget = null;
                    affectedCreatures.Add(creature);
                    StartCoroutine(SightReset(creature));
                }
            }
            initRan = true;
        }
        void Update()
        {
            if(affectedCreatures != null && initRan)
            {
                foreach(Creature creature in affectedCreatures)
                {
                    var detection = creature.brain.instance.GetModule<BrainModuleDetection>();
                    detection.canHear = false;
                    detection.sightDetectionHorizontalFov = 0f;
                    detection.sightDetectionVerticalFov = 0f;
                    detection.alertednessLevel = 0f;
                    creature.brain.currentTarget = null;
                    creature.brain.isAttacking = false;
                    creature.brain.state = Brain.State.Idle;

                }
            }
            if (affectedCreatures.Count == 0 && initRan) Destroy(this);
        }
        public IEnumerator SightReset(Creature creature)
        {
            yield return new WaitForSeconds(10f);
            affectedCreatures.Remove(creature);
            var detection = creature.brain.instance.GetModule<BrainModuleDetection>();
            detection.canHear = true;
            detection.sightDetectionHorizontalFov = initHorizontal;
            detection.sightDetectionVerticalFov = initVertical;

        }
    }
}
