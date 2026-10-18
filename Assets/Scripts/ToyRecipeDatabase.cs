using System;
using System.Collections.Generic;
using UnityEngine;

public static class ToyRecipeDatabase {

    [Serializable]
    public class RecipeStep {
        public string stepName;
        public string description;
        public Sprite icon;

        public RecipeStep(string stepName, string description, Sprite icon = null) {
            this.stepName = stepName;
            this.description = description;
            this.icon = icon;
        }
    }

    public class ToyRecipe {
        public ToyObjectSO toyObjectSO;
        public string toyName;
        public List<RecipeStep> steps;

        public ToyRecipe(ToyObjectSO toyObjectSO, string toyName, List<RecipeStep> steps) {
            this.toyObjectSO = toyObjectSO;
            this.toyName = toyName;
            this.steps = steps;
        }
    }

    public static ToyRecipe GetRecipeForToy(ToyObjectSO toySO, Sprite plasticSprite, Sprite metalSprite, Sprite meltedPlasticSprite, Sprite meltedMetalSprite) {
        if (toySO == null) return null;

        string name = !string.IsNullOrEmpty(toySO.objectName) ? toySO.objectName : toySO.name;

        // Plastic toys: BoyDoll or Dinosaur
        if (name.IndexOf("Boy", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("Doll", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("Dino", StringComparison.OrdinalIgnoreCase) >= 0) {

            return new ToyRecipe(toySO, name, new List<RecipeStep> {
                new RecipeStep("1. Raw Material", "Get Plastic from Container", plasticSprite),
                new RecipeStep("2. Melt Plastic", "Melt at Melter & take Plastic Holder", meltedPlasticSprite),
                new RecipeStep("3. Mold Maker", "Pour into Mold Maker & select " + name, toySO.sprite),
                new RecipeStep("4. Color Counter", "Color at Color Counter & collect ready toy", toySO.sprite)
            });
        }

        // Metal toys: Car or Robot
        if (name.IndexOf("Car", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("Robot", StringComparison.OrdinalIgnoreCase) >= 0) {

            return new ToyRecipe(toySO, name, new List<RecipeStep> {
                new RecipeStep("1. Raw Material", "Get Metal from Container", metalSprite),
                new RecipeStep("2. Melt Metal", "Melt at Melter & take Metal Picker", meltedMetalSprite),
                new RecipeStep("3. Mold Maker", "Pour into Mold Maker & select " + name, toySO.sprite),
                new RecipeStep("4. Color Counter", "Color at Color Counter & collect ready toy", toySO.sprite)
            });
        }

        // Fallback generic recipe
        return new ToyRecipe(toySO, name, new List<RecipeStep> {
            new RecipeStep("1. Raw Material", "Gather raw material from Container", toySO.sprite),
            new RecipeStep("2. Melter", "Melt raw material at Melter Counter", toySO.sprite),
            new RecipeStep("3. Mold Maker", "Mold " + name + " at Mold Maker", toySO.sprite),
            new RecipeStep("4. Color Counter", "Color and finish at Color Counter", toySO.sprite)
        });
    }
}
