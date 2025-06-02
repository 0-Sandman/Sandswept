/*
using BepInEx;
using BepInEx.Bootstrap;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RoR2.UI.CharacterSelectController;

namespace Sandswept.Survivors.Ranger.SkillDefs.Passive
{
    public class PassiveFix
    {
        public static bool ModExists(params string[] mods)
            => mods.All(Chainloader.PluginInfos.ContainsKey);
        public static void Init()
        {
            if (ModExists("prodzpod.MinerSkillReturns") || ModExists("xyz.yekoc.PassiveAgression")) return;
            On.RoR2.UI.LoadoutPanelController.Row.FromSkillSlot += (orig, owner, bodyIndex, skillSlotIndex, skillSlot) =>
            {
                var ret = (LoadoutPanelController.Row)orig(owner, bodyIndex, skillSlotIndex, skillSlot);
                if (((ScriptableObject)skillSlot.skillFamily).name.Contains("Passive"))
                {
                    Transform label = ret.rowPanelTransform.Find("SlotLabel") ?? ret.rowPanelTransform.Find("LabelContainer").Find("SlotLabel");
                    if (label) label.GetComponent<LanguageTextMeshController>().token = "Passive";
                }
                return ret;
            };
            // specifically copied these part from pa, wtf is this function
            if (!ModExists("com.Borbo.ArtificerExtended"))
            {
                // if we keep making more and more fucked up structures they will eventually add it to r2api
                IL.RoR2.UI.CharacterSelectController.BuildSkillStripDisplayData += (il) =>
                {
                    ILCursor c = new(il);
                    int skillIndex = -1;
                    c.GotoNext(x => x.MatchLdloc(out skillIndex), x => x.MatchLdfld(typeof(GenericSkill).GetField("hideInCharacterSelect")), x => x.MatchBrtrue(out _));
                    c.GotoNext(x => x.MatchCallOrCallvirt(typeof(List<StripDisplayData>).GetMethod("Add")));
                    if (!Chainloader.PluginInfos.ContainsKey("com.Borbo.ArtificerExtended")) c.Remove();
                    c.Emit(OpCodes.Ldloc, skillIndex);
                    c.EmitDelegate<Action<List<StripDisplayData>, StripDisplayData, GenericSkill>>((list, disp, ski) =>
                    {
                        if ((ski.skillFamily as ScriptableObject).name.Contains("Passive")) list.Insert(0, disp);
                        else list.Add(disp);
                    });
                };
                // seriously this should be r2api.skills default
            }
            IL.RoR2.UI.LoadoutPanelController.Rebuild += (il) =>
            {
                ILCursor c = new(il);
                c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(LoadoutPanelController.Row).GetMethod(nameof(LoadoutPanelController.Row.FromSkillSlot))));
                c.EmitDelegate<Func<LoadoutPanelController.Row, LoadoutPanelController.Row>>((orig) =>
                {
                    var label = orig.rowPanelTransform.Find("SlotLabel") ?? orig.rowPanelTransform.Find("LabelContainer").Find("SlotLabel");
                    if (label && label.GetComponent<LanguageTextMeshController>().token == "Passive") orig.rowPanelTransform.SetSiblingIndex(0);
                    return orig;
                });
            };
        }
    }
}
*/