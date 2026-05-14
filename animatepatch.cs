using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

[HarmonyDebug]
public static class AnimatePatch{
    public static bool MyAnimatorSpray(GameObject frankenObject, GameObject Actor=null, GameObject Using=null){
        if (frankenObject.HasPart("MentalShield")){

            frankenObject.PlayWorldSound("Sounds/Interact/sfx_interact_sentience_imbue");
            Popup.Show("You grant " + frankenObject.t() + " the gift of Sapience.");
            CombatJuice.playPrefabAnimation(frankenObject, "Abilities/AbilityVFXAnimated", frankenObject.ID, frankenObject.Render.Tile + ";" + frankenObject.Render.GetTileForegroundColor() + ";" + frankenObject.Render.getDetailColor());
            AnimateEvent.Send(Actor, frankenObject, Using);
            frankenObject.RemovePart("MentalShield");
            frankenObject.RequirePart<Sapient>();
            Using?.Destroy();

            return true;
        }

        Popup.ShowFail("You can't animate an object that already has a brain.");
        return false;
    }

    public static void MyNanoAnimator(GameObject frankenObject, InventoryActionEvent E=null, GameObject Using=null){
        if (frankenObject.HasPart("MentalShield")){
            E.Actor.UseEnergy(1000, "Item Animate");
            frankenObject.PlayWorldSound("Sounds/Interact/sfx_interact_sentience_imbue");
            if (E.Actor.IsPlayer())
            {
                Popup.Show("You grant " + frankenObject.t() + " the gift of Sapience.");
            }

            AnimateEvent.Send(E.Actor, frankenObject, Using);
            frankenObject.RemovePart("MentalShield");
            frankenObject.RequirePart<Sapient>();
            E.RequestInterfaceExit();
        } else{
            Popup.ShowFail("You can't animate an object that already has a brain.");
        }
    }

    /*[HarmonyPatch(typeof(GameObject),nameof(GameObject.HasTagOrProperty))]
    static void Postfix(ref bool __result, ref GameObject __instance, ref String Name){
        if (!__result){
            __result = (Name == "Animatable" && __instance.HasPart("MentalShield"));
        }
    }*/

    [HarmonyPatch(typeof(AnimatorSpray),nameof(AnimatorSpray.HandleEvent))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> SprayTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator){
        CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);

        int start = codeMatcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldstr, "You can't animate an object that already has a brain.")
        ).Pos;
        int end = codeMatcher.MatchStartForward(
            new CodeMatch(CodeInstruction.Call(typeof(Popup),nameof(Popup.ShowFail)))
        ).Pos;
        codeMatcher.RemoveInstructionsInRange(start, end + 1); // remove popup dialog and loading a false

        LocalBuilder gameObject = codeMatcher.MatchStartBackwards(
            new CodeMatch(OpCodes.Ldloc_S)
        ).Instruction.operand as LocalBuilder; // previous ldloc.s used the index for the target GameObject
        codeMatcher.Start().Advance(start); // return to previous location

        codeMatcher.Insert(
            new CodeInstruction(OpCodes.Ldloc_S, gameObject),
            new CodeInstruction(OpCodes.Ldarg_1), // Event argument
            CodeInstruction.LoadField(typeof(InventoryActionEvent), "Actor"), // should be E.Actor
            new CodeInstruction(OpCodes.Ldarg_0), // this
            CodeInstruction.LoadField(typeof(IPart), "_ParentObject"), // this._ParentObject
            CodeInstruction.Call(typeof(AnimatePatch), nameof(AnimatePatch.MyAnimatorSpray)) // AnimateObject.Animate(gameObject, E.Actor, ParentObject);
        );
        return codeMatcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(AnimateObject),nameof(AnimateObject.HandleEvent), typeof(InventoryActionEvent))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> NanoTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator){
        CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);

        int start = codeMatcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldstr, "You can't animate an object that already has a brain.")
        ).Pos;
        int end = codeMatcher.MatchStartForward(
            new CodeMatch(CodeInstruction.Call(typeof(Popup),nameof(Popup.Show)))
        ).Pos;
        codeMatcher.RemoveInstructionsInRange(start, end); // remove popup dialog

        LocalBuilder gameObject = codeMatcher.MatchStartBackwards(
            new CodeMatch(OpCodes.Ldloc_S)
        ).Instruction.operand as LocalBuilder; // previous ldloc.s used the index for the target GameObject
        codeMatcher.Start().Advance(start); // return to start location

        codeMatcher.Insert(
            new CodeInstruction(OpCodes.Ldloc_S, gameObject),
            new CodeInstruction(OpCodes.Ldarg_1), // Event argument
            new CodeInstruction(OpCodes.Ldarg_0), // this
            CodeInstruction.LoadField(typeof(IPart), "_ParentObject"), // this._ParentObject
            CodeInstruction.Call(typeof(AnimatePatch), nameof(AnimatePatch.MyNanoAnimator)) // AnimateObject.Animate(gameObject, E.Actor, ParentObject);
        );
        return codeMatcher.InstructionEnumeration();
    }
}
