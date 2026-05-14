using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

[HarmonyDebug]
public static class AnimatePatch{
    public static bool MyAnimate(GameObject frankenObject, GameObject Actor=null, GameObject Using=null){
        if (frankenObject.HasPart("MentalShield")){
            frankenObject.RemovePart("MentalShield");

            frankenObject.PlayWorldSound("Sounds/Interact/sfx_interact_sentience_imbue");
            Popup.Show("You grant " + frankenObject.t() + " the gift of Sapience.");
            CombatJuice.playPrefabAnimation(frankenObject, "Abilities/AbilityVFXAnimated", frankenObject.ID, frankenObject.Render.Tile + ";" + frankenObject.Render.GetTileForegroundColor() + ";" + frankenObject.Render.getDetailColor());
            Using?.Destroy();

            return true;
        }
        return false;
    }



    [HarmonyPatch(typeof(GameObject),nameof(GameObject.HasTagOrProperty))]
    static void Postfix(ref bool __result, ref GameObject __instance, ref String Name){
        if (!__result){
            __result = (Name == "Animatable" && __instance.HasPart("MentalShield"));
        }
    }
    [HarmonyPatch(typeof(AnimatorSpray),nameof(AnimatorSpray.HandleEvent))]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator){
        CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);

        codeMatcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldstr, "You can't animate an object that already has a brain.")
        ).RemoveInstructions(6); //remove popup dialog and loading a false

        int pos = codeMatcher.Pos; //save our current position
        LocalBuilder gameObject = codeMatcher.MatchStartBackwards(
            new CodeMatch(OpCodes.Ldloc_S)
        ).Instruction.operand as LocalBuilder; //previous ldloc.s used the index for the target GameObject
        codeMatcher.Start().Advance(pos); //return to previous location

        codeMatcher.Insert(
            new CodeInstruction(OpCodes.Ldloc_S, gameObject),
            new CodeInstruction(OpCodes.Ldarg_1), // Event argument
            CodeInstruction.LoadField(typeof(InventoryActionEvent), "Actor"), //should be E.Actor
            new CodeInstruction(OpCodes.Ldarg_0), // this
            CodeInstruction.LoadField(typeof(IPart), "_ParentObject"), // this.get_ParentObject()
            CodeInstruction.Call(typeof(AnimatePatch), nameof(AnimatePatch.MyAnimate)/*, [typeof(GameObject), typeof(GameObject), typeof(GameObject)]*/) // AnimateObject.Animate(gameObject, E.Actor, ParentObject);
        );
        return codeMatcher.InstructionEnumeration();
    }
}
