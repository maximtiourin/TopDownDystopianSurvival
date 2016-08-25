/*
 * Author - Maxim Tiourin
 */

using UnityEngine;
using System;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Component_TileDataGenerator))]
public class EditorScript_Component_TileDataGenerator : Editor {
    private static int COUNT_MUTATE = 2;
    private static int MUTATE_DEFAULT = 0;
    private static int MUTATE_DELETE = 1;

    private bool foldTilesets;
    private bool[] foldTile;
    private bool[,] mutateButton; //Dangerous buttons like reset or delete, need to store confirmation status
    private bool[] zoomButton;
    
    private bool runOnce = true;

    private Editor editor;
    private Component_TileDataGenerator gen;

    public override void OnInspectorGUI() {
        editor = this;
        gen = (Component_TileDataGenerator) target;

        //Init
        stateInit();

        if (runOnce) {
            //Dont know why this needs to be ran once, but it does! Don't question the clusterfuck that is unity
            increaseMemory(gen.tileset.Length);

            runOnce = false;
        }

        //Textures
        Texture texHorSep = Resources.Load<Texture>("layout_horizontalseperator");
        Texture texVerSep = Resources.Load<Texture>("layout_verticalseperator");
        Texture texPlus = Resources.Load<Texture>("inspectorbutton_plus");
        Texture texMinus = Resources.Load<Texture>("inspectorbutton_minus");
        Texture texReverse = Resources.Load<Texture>("inspectorbutton_reverse");
        Texture texCopy = Resources.Load<Texture>("inspectorbutton_copy");
        Texture texMagnify = Resources.Load<Texture>("inspectorbutton_magnify");
        Texture texReduce = Resources.Load<Texture>("inspectorbutton_reduce");

        /* Styles */
        //Full Image Button
        GUIStyle styleFullImageBtn = new GUIStyle(GUI.skin.button);
        styleFullImageBtn.fixedWidth = 0;
        styleFullImageBtn.fixedHeight = 0;
        styleFullImageBtn.stretchWidth = true;
        styleFullImageBtn.stretchHeight = true;
        styleFullImageBtn.clipping = TextClipping.Overflow;
        styleFullImageBtn.padding = new RectOffset(0, 0, 0, 0);


        //Begin Inspector
        GUILayout.BeginVertical();

        GUILayout.Space(8);

        drawHorizontalSeperator(texHorSep, 4f);

        GUILayout.BeginHorizontal();

        //Create Add Element Button / Zoom All / Reduce All
        if (GUILayout.Button(texPlus, styleFullImageBtn, GUILayout.Width(32), GUILayout.Height(32))) {
            int oldLength = gen.tileset.Length;

            gen.addData(new Component_TileDataGenerator.Data());

            increaseMemory(oldLength);
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(texMagnify, styleFullImageBtn, GUILayout.Width(32), GUILayout.Height(32))) {
            for (int i = 0; i < gen.tileset.Length; i++) {
                zoomButton[i] = true;
            }
        }

        if (GUILayout.Button(texReduce, styleFullImageBtn, GUILayout.Width(32), GUILayout.Height(32))) {
            for (int i = 0; i < gen.tileset.Length; i++) {
                zoomButton[i] = false;
            }
        }

        GUILayout.EndHorizontal();

        drawHorizontalSeperator(texHorSep, 4f);

        GUILayout.Space(8);

        //Display Tilesets
        foldTilesets = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), foldTilesets, "Tilesets (Count: " + gen.tileset.Length + ")", true);
        if (foldTilesets) {
            GUILayout.BeginVertical();

            drawHorizontalSeperator(texHorSep, 3f);

            //Expand/Collapse buttons
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Expand All")) {
                for (int i = 0; i < foldTile.Length; i++) {
                    foldTile[i] = true;
                }
            }

            if (GUILayout.Button("Collapse All")) {
                for (int i = 0; i < foldTile.Length; i++) {
                    foldTile[i] = false;
                }
            }

            GUILayout.EndHorizontal();

            drawHorizontalSeperator(texHorSep, 3f);

            //List Tilesets
            for (int i = 0; i < gen.tileset.Length; i++) {
                Component_TileDataGenerator.Data data = gen.tileset[i];

                GUILayout.BeginHorizontal();

                GUILayout.Space(8);

                GUILayout.BeginVertical();

                foldTile[i] = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), foldTile[i], data.name, true);
                if (foldTile[i]) {
                    GUILayout.BeginVertical();

                    //Name / TextField / Copy Button / Reset to Default Button / Delete Button
                    GUILayout.BeginHorizontal();

                    GUILayout.Space(8);

                    GUILayout.Label("Name", GUILayout.ExpandWidth(false));

                    data.name = EditorGUILayout.DelayedTextField(data.name, GUILayout.Width(200));

                    GUILayout.FlexibleSpace();

                    //Display mutate buttons when none have been clicked
                    if (!mutateEnabled(i)) {
                        if (zoomButton[i]) {
                            if (GUILayout.Button(texReduce, styleFullImageBtn, GUILayout.Width(22), GUILayout.Height(22))) {
                                zoomButton[i] = false;
                            }
                        }
                        else {
                            if (GUILayout.Button(texMagnify, styleFullImageBtn, GUILayout.Width(22), GUILayout.Height(22))) {
                                zoomButton[i] = true;
                            }
                        }

                        if (GUILayout.Button(texCopy, styleFullImageBtn, GUILayout.Width(22), GUILayout.Height(22))) {
                            int oldLength = gen.tileset.Length;

                            gen.copyDataExceptName(data);

                            increaseMemory(oldLength);
                        }

                        if (GUILayout.Button(texReverse, styleFullImageBtn, GUILayout.Width(22), GUILayout.Height(22))) {
                            clearMutates();
                            mutateButton[MUTATE_DEFAULT, i] = true;
                        }

                        if (GUILayout.Button(texMinus, styleFullImageBtn, GUILayout.Width(22), GUILayout.Height(22))) {
                            clearMutates();
                            mutateButton[MUTATE_DELETE, i] = true;
                        }
                    }

                    //Display mutate buttons when one has been clicked
                    if (mutateButton[MUTATE_DEFAULT, i]) {
                        GUILayout.Label("Reset to Default?", GUILayout.ExpandWidth(false));

                        if (GUILayout.Button("Yes", GUILayout.Width(44), GUILayout.Height(22))) {
                            gen.tileset[i] = new Component_TileDataGenerator.Data();
                            mutateButton[MUTATE_DEFAULT, i] = false;
                        }

                        if (GUILayout.Button("No", GUILayout.Width(44), GUILayout.Height(22))) {
                            mutateButton[MUTATE_DEFAULT, i] = false;
                        }
                    }

                    if (mutateButton[MUTATE_DELETE, i]) {
                        GUILayout.Label("Delete?", GUILayout.ExpandWidth(false));

                        if (GUILayout.Button("Yes", GUILayout.Width(44), GUILayout.Height(22))) {
                            gen.deleteData(i);
                            mutateButton[MUTATE_DELETE, i] = false;
                        }

                        if (GUILayout.Button("No", GUILayout.Width(44), GUILayout.Height(22))) {
                            mutateButton[MUTATE_DELETE, i] = false;
                        }
                    }


                    GUILayout.EndHorizontal();
                    //-------------------------------------------

                    drawHorizontalSeperator(texHorSep, 1f);

                    //Sprite / Normal Map
                    GUILayout.BeginHorizontal();

                    int zoomwidth = (zoomButton[i]) ? (128) : (40);
                    int zoomheight = (zoomButton[i]) ? (128) : (40);

                    GUILayout.BeginVertical();

                    GUILayout.Label("Sprite");
                    data.sprite = (Sprite) EditorGUILayout.ObjectField(data.sprite, typeof(Sprite), false, GUILayout.Width(zoomwidth), GUILayout.Height(zoomheight));

                    GUILayout.EndVertical();

                    drawVerticalSeperator(texVerSep, 1f, zoomheight);

                    GUILayout.BeginVertical();

                    data.isNormalMapped = GUILayout.Toggle(data.isNormalMapped, "Normal Map");

                    if (data.isNormalMapped) {
                        data.normalMap = (Texture) EditorGUILayout.ObjectField(data.normalMap, typeof(Texture), false, GUILayout.Width(zoomwidth), GUILayout.Height(zoomheight));
                    }

                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();
                    //-------------------------------------------

                    GUILayout.Space(8);

                    drawHorizontalSeperator(texHorSep, 1f);

                    GUILayout.Space(8);

                    //Color / Shiny
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Sprite Color", GUILayout.ExpandWidth(false));
                    data.mainColor = EditorGUILayout.ColorField(data.mainColor);

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();

                    data.isShiny = GUILayout.Toggle(data.isShiny, "  Shiny    ", GUILayout.ExpandWidth(false));

                    if (data.isShiny) {
                        data.specularColor = EditorGUILayout.ColorField(data.specularColor);
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();

                    if (data.isShiny) {
                        GUILayout.Label("Shininess    ", GUILayout.ExpandWidth(false));
                        data.shininess = EditorGUILayout.DelayedFloatField(data.shininess);
                    }

                    GUILayout.EndHorizontal();
                    //-------------------------------------------

                    GUILayout.Space(8);

                    drawHorizontalSeperator(texHorSep, 1f);

                    GUILayout.Space(8);

                    //Shader
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Shader       ", GUILayout.ExpandWidth(false));
                    data.shader = (Shader) EditorGUILayout.ObjectField(data.shader, typeof(Shader), false);

                    GUILayout.EndHorizontal();
                    //-------------------------------------------

                    GUILayout.Space(8);

                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                drawHorizontalSeperator(texHorSep, 3f);
            }

            GUILayout.EndVertical();
        }

        GUILayout.Space(32);

        GUILayout.EndVertical();
    }

    private void drawHorizontalSeperator(Texture texture, float height) {
        Rect rect = GUILayoutUtility.GetRect(0f, 9999f, height, height);

        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill);
    }

    private void drawVerticalSeperator(Texture texture, float width, float height = 0f) {
        Rect rect = GUILayoutUtility.GetRect(width, width, (height == 0f) ? (0f) : (height), (height == 0f) ? (9999f) : (height));

        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill);
    }

    private void stateInit() {
        int length = gen.tileset.Length;

        if (mutateButton == null) mutateButton = new bool[COUNT_MUTATE, length];
    }

    private bool mutateEnabled(int j) {
        bool mutate = false;
        for (int i = 0; i < COUNT_MUTATE; i++) {
            mutate = mutate | mutateButton[i, j];
        }
        return mutate;
    }

    private void clearMutates() {
        for (int i = 0; i < COUNT_MUTATE; i++) {
            for (int j = 0; j < gen.tileset.Length; j++) {
                mutateButton[i, j] = false;
            }
        }
    }

    private void increaseMemory(int oldLength) {
        int length = gen.tileset.Length;

        if (oldLength == length) {
            foldTile = new bool[length];
            zoomButton = new bool[length];
        }
        else {
            //FoldTile
            bool[] newFoldTile = new bool[length];
            Array.Copy(foldTile, newFoldTile, oldLength);
            foldTile = newFoldTile;
            foldTile[length - 1] = true; //Fold open a newly created tile

            //Mutate
            mutateButton = new bool[COUNT_MUTATE, length];

            //Zoom
            bool[] newZoomButton = new bool[length];
            Array.Copy(zoomButton, newZoomButton, oldLength);
            zoomButton = newZoomButton;
        }
    }
}
