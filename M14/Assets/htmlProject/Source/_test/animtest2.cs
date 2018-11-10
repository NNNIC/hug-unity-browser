using UnityEngine;
using System.Collections;

public class animtest2 : MonoBehaviour {

    void Start() {
        gameObject.AddComponent<Animation>();
        gameObject.AddComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(-1, 5, 0), new Vector3(1, 5, 0),
            new Vector3(-1, 8, 0), new Vector3(1, 8, 0), new Vector3(-1, 13, 0), new Vector3(1, 13, 0)
        };
        mesh.uv = new Vector2[] {
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)
        };
        mesh.triangles = new int[] {
            0, 1, 2, 1, 3, 2,
            4, 5, 6, 5, 7, 6
        };
        mesh.RecalculateNormals();
        renderer.material = new Material(Shader.Find("Diffuse"));
        BoneWeight[] weights = new BoneWeight[8]{
            new BoneWeight(){ boneIndex0=1, weight0=1},
            new BoneWeight(){ boneIndex0=1, weight0=1},
            new BoneWeight(){ boneIndex0=1, weight0=1},
            new BoneWeight(){ boneIndex0=1, weight0=1},

            new BoneWeight(){ boneIndex0=2, weight0=1},
            new BoneWeight(){ boneIndex0=2, weight0=1},
            new BoneWeight(){ boneIndex0=2, weight0=1},
            new BoneWeight(){ boneIndex0=2, weight0=1},

        };

        mesh.boneWeights = weights;
        Transform[] bones = new Transform[3];
        Matrix4x4[] bindPoses = new Matrix4x4[3];

        bones[0] = new GameObject("Parent").transform;
        bones[0].parent = transform;
        bones[0].localRotation = Quaternion.identity;
        bones[0].localPosition = Vector3.zero;
        bindPoses[0] = bones[0].worldToLocalMatrix * transform.localToWorldMatrix;

        bones[1] = new GameObject("Lower").transform;
        bones[1].parent = bones[0];
        bones[1].localRotation = Quaternion.identity;
        bones[1].localPosition = Vector3.zero;
        bindPoses[1] = bones[1].worldToLocalMatrix * bones[0].transform.localToWorldMatrix;

        bones[2] = new GameObject("Upper").transform;
        bones[2].parent = bones[0];
        bones[2].localRotation = Quaternion.identity;
        bones[2].localPosition = new Vector3(0, 5, 0);
        bindPoses[2] = bones[2].worldToLocalMatrix * bones[0].transform.localToWorldMatrix;
        mesh.bindposes = bindPoses;
        renderer.bones = bones;
        renderer.sharedMesh = mesh;

        AnimationCurve curve = new AnimationCurve();
        curve.keys = new Keyframe[] {new Keyframe(0, 0, 0, 0), new Keyframe(1, 3, 0, 0), new Keyframe(2, 0.0F, 0, 0)};
        AnimationClip clip = new AnimationClip();
        clip.SetCurve("Lower", typeof(Transform), "m_LocalPosition.z", curve);
        animation.AddClip(clip, "test");
        animation.Play("test");
    }

}
