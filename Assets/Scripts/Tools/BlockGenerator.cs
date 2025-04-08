using UnityEngine;
using UnityEditor;

public class BlockGenerator : EditorWindow
{
    GameObject prefab;
    int width = 10;
    int height = 1;
    int depth = 10;

    [MenuItem("Tools/Block Generator")]
    static void OpenWindow()
    {
        GetWindow<BlockGenerator>("Block Generator");
    }

    void OnGUI()
    {
        prefab = (GameObject)EditorGUILayout.ObjectField("Block Prefab", prefab, typeof(GameObject), false);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        depth = EditorGUILayout.IntField("Depth", depth);

        if (GUILayout.Button("Generate"))
        {
            GenerateBlocks();
        }
    }

    void GenerateBlocks()
    {
        if (prefab == null) return;

        GameObject parent = new GameObject("BlockGroup");

        Vector3 prefabScale = prefab.transform.localScale;

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        for (int z = 0; z < depth; z++)
        {
            GameObject block = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            block.transform.position = new Vector3(
                x * prefabScale.x,
                y * prefabScale.y,
                z * prefabScale.z
            );
            block.transform.SetParent(parent.transform);
        }
    }
}
