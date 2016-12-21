using UnityEngine;
using System.Collections;

//TODO TEMPORARY DEBUG CHARACTER
public class TestCharacter : Pawn {
    
	protected new void construct() {
        base.construct();
	}
	
	protected new void update() {
        base.update();
	}

    protected override void init() {
        generateRenderObject();

        SpriteRenderer rend = renderObject.AddComponent<SpriteRenderer>();

        rend.sortingLayerName = "OtherCharacter";

        rend.sprite = level.TestCharacterSprite;
    }

    /*
     * TODO - testing method, all pawn translation should happen using level logic, the setWorldPosition are just helped methods, so a level
     * should keep track of moving pawns, and what chunks or regions they are contained within.
     */
    public static void CreateTestCharacterAtWorldPosition(Level level, Vector3 pos) {
        TestCharacter pawn = new TestCharacter();
        pawn.construct();
        pawn.level = level;
        pawn.init();
        pawn.setWorldPosition(pos);
    }
}
