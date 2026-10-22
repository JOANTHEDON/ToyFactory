using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds the step-by-step recipe for any toy by reading the real recipe ScriptableObjects
/// that the counters in the scene use (cutting, melting, molding/shaping, painting).
/// Because it is data driven, the Recipe Details modal always matches what the machines actually do.
/// </summary>
public static class ToyRecipeDatabase {

    public class RecipeStep {
        public string stepName;
        public string description;
        public Sprite icon;
        public string station;
        public string keyHint;
        public float seconds;

        public RecipeStep(string stepName, string description, Sprite icon = null, string station = "", string keyHint = "E", float seconds = 0f) {
            this.stepName = stepName;
            this.description = description;
            this.icon = icon;
            this.station = station;
            this.keyHint = keyHint;
            this.seconds = seconds;
        }
    }

    public class ToyRecipe {
        public ToyObjectSO toyObjectSO;
        public string toyName;
        public string materialName;
        public List<RecipeStep> steps;

        public ToyRecipe(ToyObjectSO toyObjectSO, string toyName, List<RecipeStep> steps) {
            this.toyObjectSO = toyObjectSO;
            this.toyName = toyName;
            this.steps = steps;
        }

        public float GetMachineSeconds() {
            float total = 0f;
            foreach (RecipeStep step in steps) total += step.seconds;
            return total;
        }
    }

    private static readonly Dictionary<ToyObjectSO, ToyRecipe> cache = new Dictionary<ToyObjectSO, ToyRecipe>();
    private static bool scanned;
    private static readonly List<CuttingRecipeSO> cuttingRecipes = new List<CuttingRecipeSO>();
    private static readonly List<MeltingRecipeSO> meltingRecipes = new List<MeltingRecipeSO>();
    private static readonly List<MoldingRecipeSO> moldingRecipes = new List<MoldingRecipeSO>();
    private static readonly List<MoldingRecipeSO> shapingRecipes = new List<MoldingRecipeSO>();
    private static float coloringSeconds = 4f;
    private static bool hasColorCounter;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        Invalidate();
        SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
        SceneManager.sceneLoaded += SceneManager_SceneLoaded;
    }

    private static void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode) {
        Invalidate();
    }

    public static void Invalidate() {
        cache.Clear();
        scanned = false;
    }

    private static void EnsureScanned() {
        if (scanned) return;
        scanned = true;

        cuttingRecipes.Clear();
        meltingRecipes.Clear();
        moldingRecipes.Clear();
        shapingRecipes.Clear();
        hasColorCounter = false;

        foreach (WoodCounter counter in UnityEngine.Object.FindObjectsByType<WoodCounter>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            AddAll(cuttingRecipes, counter.GetCuttingRecipeSOArray());
        }
        foreach (MelterCounter counter in UnityEngine.Object.FindObjectsByType<MelterCounter>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            AddAll(meltingRecipes, counter.GetMeltingRecipeSOArray());
        }
        foreach (MoldMakerCounter counter in UnityEngine.Object.FindObjectsByType<MoldMakerCounter>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            AddAll(moldingRecipes, counter.GetMoldingRecipeSOArray());
        }
        foreach (WoodShaperCounter counter in UnityEngine.Object.FindObjectsByType<WoodShaperCounter>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            AddAll(shapingRecipes, counter.GetShapingRecipeSOArray());
        }
        foreach (ColorCounter counter in UnityEngine.Object.FindObjectsByType<ColorCounter>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            hasColorCounter = true;
            coloringSeconds = counter.GetColoringTimerMax();
        }
    }

    private static void AddAll<T>(List<T> list, T[] items) where T : UnityEngine.Object {
        if (items == null) return;
        foreach (T item in items) {
            if (item != null && !list.Contains(item)) list.Add(item);
        }
    }

    public static ToyRecipe GetRecipeForToy(ToyObjectSO toySO) {
        if (toySO == null) return null;
        EnsureScanned();

        if (cache.TryGetValue(toySO, out ToyRecipe cached)) return cached;

        ToyRecipe recipe = BuildFromRecipeAssets(toySO);
        if (recipe == null) recipe = BuildFallback(toySO);
        cache[toySO] = recipe;
        return recipe;
    }

    /// <summary>Kept for older callers; the sprites are no longer needed because icons come from the recipe assets.</summary>
    public static ToyRecipe GetRecipeForToy(ToyObjectSO toySO, Sprite plasticSprite, Sprite metalSprite, Sprite meltedPlasticSprite, Sprite meltedMetalSprite) {
        return GetRecipeForToy(toySO);
    }

    // ---------------------------------------------------------------------

    private static ToyRecipe BuildFromRecipeAssets(ToyObjectSO toySO) {
        string toyName = ToyObjectSO.GetDisplayName(toySO);

        MoldingRecipeSO mold = FindMoldingRecipeWithOutput(moldingRecipes, toySO);
        MoldingRecipeSO shape = FindMoldingRecipeWithOutput(shapingRecipes, toySO);
        MoldingRecipeSO finalStage = mold != null ? mold : shape;
        if (finalStage == null || finalStage.input == null) return null;

        List<RecipeStep> steps = new List<RecipeStep>();
        ToyObjectSO stageInput = finalStage.input;
        string stageInputName = ToyObjectSO.GetDisplayName(stageInput);
        ToyObjectSO raw = stageInput;

        MeltingRecipeSO melt = FindMeltingRecipeProducing(stageInput);
        CuttingRecipeSO cut = FindCuttingRecipeProducing(stageInput);

        if (melt != null && melt.input != null) {
            raw = melt.input;
            string rawName = ToyObjectSO.GetDisplayName(raw);
            string meltedName = ToyObjectSO.GetDisplayName(melt.output);

            steps.Add(new RecipeStep(
                "Grab " + rawName,
                "Walk to the " + rawName + " container and take one " + rawName + ".",
                raw.sprite, rawName + " Container", "E"));

            steps.Add(new RecipeStep(
                "Melt it",
                "Put the " + rawName + " in the Melter, then hit MELT " + melt.meltingProgressMax + " times until it turns into " + meltedName + ".",
                melt.output != null ? melt.output.sprite : raw.sprite, "Melter", "F x" + melt.meltingProgressMax, 0f));

            if (melt.holderOutput != null) {
                Sprite holderIcon = melt.holderOutput.sprite != null ? melt.holderOutput.sprite : (melt.output != null ? melt.output.sprite : null);
                steps.Add(new RecipeStep(
                    "Scoop it up",
                    "Pick it up from the Melter. You get a " + ToyObjectSO.GetDisplayName(melt.holderOutput) + " full of molten " + rawName + ".",
                    holderIcon, "Melter", "E"));
            }
        } else if (cut != null && cut.input != null) {
            raw = cut.input;
            string rawName = ToyObjectSO.GetDisplayName(raw);

            steps.Add(new RecipeStep(
                "Grab " + rawName,
                "Walk to the " + rawName + " container and take one " + rawName + ".",
                raw.sprite, rawName + " Container", "E"));

            steps.Add(new RecipeStep(
                "Cut it",
                "Place the " + rawName + " on the Wood Cutter and wait " + Seconds(cut.cuttingTimerMax) + ". Take the " + stageInputName + " when the saw stops.",
                stageInput.sprite != null ? stageInput.sprite : raw.sprite, "Wood Cutter", "E", cut.cuttingTimerMax));
        } else {
            steps.Add(new RecipeStep(
                "Grab " + stageInputName,
                "Take one " + stageInputName + " from its container.",
                stageInput.sprite, stageInputName + " Container", "E"));
        }

        if (mold != null) {
            steps.Add(new RecipeStep(
                "Mold the " + toyName,
                "Pour the " + stageInputName + " into the Mold Maker, choose " + toyName + " from the menu and wait " + Seconds(mold.moldingTimerMax) + ".",
                toySO.sprite, "Mold Maker", "E", mold.moldingTimerMax));
        } else {
            steps.Add(new RecipeStep(
                "Carve the " + toyName,
                "Load the " + stageInputName + " into the Wood Shaper, choose " + toyName + " from the menu and wait " + Seconds(shape.moldingTimerMax) + ".",
                toySO.sprite, "Wood Shaper", "E", shape.moldingTimerMax));
        }

        if (hasColorCounter) {
            steps.Add(new RecipeStep(
                "Paint it",
                "Put the " + toyName + " on the Paint Station and wait " + Seconds(coloringSeconds) + " until the colors are on. Unpainted toys are refused.",
                toySO.sprite, "Paint Station", "E", coloringSeconds));
        }

        steps.Add(new RecipeStep(
            "Deliver",
            "Carry the finished " + toyName + " to the Delivery Counter. The customer who ordered it takes it, no matter where they stand in the line.",
            UIResources.Delivery != null ? UIResources.Delivery : toySO.sprite, "Delivery Counter", "E"));

        ToyRecipe recipe = new ToyRecipe(toySO, toyName, steps);
        recipe.materialName = ToyObjectSO.GetDisplayName(raw);
        return recipe;
    }

    private static ToyRecipe BuildFallback(ToyObjectSO toySO) {
        string name = ToyObjectSO.GetDisplayName(toySO);
        ToyRecipe recipe = new ToyRecipe(toySO, name, new List<RecipeStep> {
            new RecipeStep("Raw Material", "Gather the raw material from its container.", toySO.sprite, "Container", "E"),
            new RecipeStep("Process it", "Prepare the material at the matching machine (Melter or Wood Cutter).", toySO.sprite, "Machine", "E"),
            new RecipeStep("Shape the " + name, "Make the " + name + " at the Mold Maker or Wood Shaper.", toySO.sprite, "Mold Maker", "E"),
            new RecipeStep("Paint it", "Finish it at the Paint Station.", toySO.sprite, "Paint Station", "E"),
            new RecipeStep("Deliver", "Hand it to the customer at the Delivery Counter.", UIResources.Delivery != null ? UIResources.Delivery : toySO.sprite, "Delivery Counter", "E"),
        });
        recipe.materialName = "Mystery";
        return recipe;
    }

    private static MoldingRecipeSO FindMoldingRecipeWithOutput(List<MoldingRecipeSO> list, ToyObjectSO output) {
        foreach (MoldingRecipeSO recipe in list) {
            if (recipe == null || recipe.availableOutputs == null) continue;
            foreach (ToyObjectSO candidate in recipe.availableOutputs) {
                if (candidate == output) return recipe;
            }
        }
        return null;
    }

    private static MeltingRecipeSO FindMeltingRecipeProducing(ToyObjectSO item) {
        foreach (MeltingRecipeSO recipe in meltingRecipes) {
            if (recipe == null) continue;
            if (recipe.holderOutput == item || recipe.output == item) return recipe;
        }
        return null;
    }

    private static CuttingRecipeSO FindCuttingRecipeProducing(ToyObjectSO item) {
        foreach (CuttingRecipeSO recipe in cuttingRecipes) {
            if (recipe != null && recipe.output == item) return recipe;
        }
        return null;
    }

    private static string Seconds(float seconds) {
        return Mathf.RoundToInt(seconds) + "s";
    }
}
