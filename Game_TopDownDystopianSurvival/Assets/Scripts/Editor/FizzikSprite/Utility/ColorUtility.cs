using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fizzik {
    public class ColorUtility {
        /*
         * Returns a color darker than col by a factor between [0.0, 1.0]
         * The smaller the factor, the darker the new color is.
         */
        public static Color darker(Color col, float factor = .75f) {
            float f = Mathf.Clamp(factor, 0f, 1f);

            return new Color(col.r * factor, col.g * factor, col.b * factor, col.a);
        }

        /*
         * Returns a color lighter than col by a factor between [0.0, 1.0]
         * The larger the factor, the lighter the new color is
         */
        public static Color lighter(Color col, float factor = .25f) {
            return new Color((1f - col.r) * factor, (1f - col.g) * factor, (1f - col.b) * factor, col.a);
        }
    }
}
