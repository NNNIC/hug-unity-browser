using UnityEditor;
using UnityEngine;
using System.Collections;

public class CloneIt : MonoBehaviour {
    [MenuItem ("Assets/Clone")]
    static void AssetInstantiate()
    {
        GameObject obj = Selection.gameObjects[0];
        Instantiate(obj);
    }
}
