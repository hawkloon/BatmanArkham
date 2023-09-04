using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Manikin;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace XESuitPod
{
    class BatgirlHair : ItemModuleApparel
    {
        public IEnumerator HairChange(Creature creature, ItemModuleWardrobe.CreatureWardrobe wardrobeData)
        {
            yield return Yielders.ForSeconds(1.5f);
            if (creature.data.gender == CreatureData.Gender.Female)
            {
                var s = wardrobeData.manikinWardrobeData.channels;
                List<ManikinPart> parts = creature.manikinLocations.GetPartsAtChannel(s[0]);
                Renderer[] helmetRenderers = null;
                for (int i = 0; i < parts.Count; i++)
                {
                    if (parts[i].name.Contains("BatgirlCowl"))
                    {
                        helmetRenderers = parts[i].GetRenderers();
                    }
                }
                if (helmetRenderers != null)
                {
                    foreach (Material material in helmetRenderers[0].materials)
                    {
                        if (material.name.Contains("BatgirlHair"))
                        {
                            material.SetColor("_BaseColor", Player.characterData.hairColor);
                            material.SetColor("_SpecColor", Player.characterData.hairSpecularColor);

                        }
                    }
                }
            }
            yield return Yielders.ForSeconds(1.5f);
        }
        public override void OnEquip(Creature creature, ApparelModuleType equippedOn, ItemModuleWardrobe.CreatureWardrobe wardrobeData)
        {
            base.OnEquip(creature, equippedOn, wardrobeData);

            GameManager.local.StartCoroutine(HairChange(creature, wardrobeData));
        }
    }
}
