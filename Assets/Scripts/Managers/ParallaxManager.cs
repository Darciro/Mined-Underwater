using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    private Transform cam;
    private Vector3 camStartPos;
    private float distance;

    private GameObject[] backgrounds;
    private Material[] mat;
    private float[] backSpeed;

    private float farthestBg;

    [Range(0.01f, 0.05f)]
    public float parallaxSpeed = 0.02f;

    private void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        // Find all backgrounds once
        backgrounds = GameObject.FindGameObjectsWithTag("ParallaxBackground");
        int bgCount = backgrounds.Length;

        SpeedCalculate(bgCount);
    }

    private void SpeedCalculate(int bgCount)
    {
        // Sort backgrounds by Z position (closest to farthest from camera)
        System.Array.Sort(backgrounds, (a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

        // Initialize arrays after sorting
        mat = new Material[bgCount];
        backSpeed = new float[bgCount];

        // Get materials and assign speeds based on sorted order
        for (int i = 0; i < bgCount; i++)
        {
            mat[i] = backgrounds[i].GetComponent<MeshRenderer>().material;
            backSpeed[i] = 1f - ((float)i / Mathf.Max(bgCount - 1, 1));
        }
    }

    private void LateUpdate()
    {
        distance = cam.position.x - camStartPos.x;
        transform.position = new Vector3(cam.position.x - 2f, transform.position.y, 0);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
        }
    }
}