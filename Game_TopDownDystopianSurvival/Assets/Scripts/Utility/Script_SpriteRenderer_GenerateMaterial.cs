using UnityEngine;
using System.Collections;

/*
 * Utility that allows creating a material at runtime that will assign a given sprite, normal map with a given shader, etc.
 * Author - Maxim Tiourin
 */
public class Script_SpriteRenderer_GenerateMaterial : MonoBehaviour {
	public Sprite sprite;
	public Texture normalMap;
	public Shader shader;
	public Color mainColor = new Color(1f, 1f, 1f);

	public float shininess = .016f;
	public Color specularColor = new Color(112f / 255f, 112f / 255f, 112f / 255f);

	public bool isNormalMapped = false;
	public bool isShiny = false;
    
    private SpriteRenderer rend;
    private Material material;
    
    private bool matGenerated = false;
	
	void Start () {
		if (!matGenerated) {
			GenerateMaterial();
		}
	}

	/*
	 * "_MainTex" main diffuse texture
	 * "_BumpMap" normal map texture
	 * "_Cube" reflection cube map texture
	 */
	private void GenerateMaterial() {
        rend = GetComponent<SpriteRenderer>();
		
		rend.sprite = sprite;
		
		material = new Material(shader);

		material.SetColor("_Color", mainColor);

		if (isNormalMapped) {
			material.SetTexture("_BumpMap", normalMap);
		}

		if (isShiny) {
			material.SetColor("_SpecColor", specularColor);
			material.SetFloat("_Shininess", shininess);
		}
		
		material.name = "material_generated_" + this.gameObject.GetInstanceID();
		
		rend.sharedMaterial = material;
		
		matGenerated = true;
    }

    /*
     * Performs the material generation functionality of this component, but for use inside
     * of other scripts so that a reference to the material can be used multiple times easily.
     */
    public static Material generateMaterialReference(string uniqueID, Shader shader, Color? mainColor = null, bool isNormalMapped = false, Texture normalMap = null,
                                                        bool isShiny = false, float shininess = .016f, Color? specularColor = null) {
        Material material = new Material(shader);

        material.SetColor("_Color", mainColor ?? new Color(1f, 1f, 1f));

        if (isNormalMapped) {
            material.SetTexture("_BumpMap", normalMap);
        }

        if (isShiny) {
            material.SetColor("_SpecColor", specularColor ?? new Color(112f / 255f, 112f / 255f, 112f / 255f));
            material.SetFloat("_Shininess", shininess);
        }

        material.name = "material_generated_" + uniqueID;

        return material;
    }
}
