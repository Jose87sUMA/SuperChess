using UnityEngine;
using static UnityEngine.Mathf;

namespace UI
{
    [ExecuteAlways, RequireComponent(typeof(Renderer))]
    public class SpiralZoom : MonoBehaviour
    {
        [Tooltip("Revolutions per second (0.05 = 1 turn / 20 s)")]
        public float spinRate = 0.05f;

        [Tooltip("Distance in front of camera")]
        public float distance = 10f;

        static readonly int ST  = Shader.PropertyToID("_MainTex_ST");
        static readonly int Rot = Shader.PropertyToID("_Rot");

        Material mat; Camera cam; float phase;

        void Awake()
        {
            mat = GetComponent<Renderer>().sharedMaterial;
            cam = Camera.main;
        }

        void LateUpdate()
        {
            if (!cam) cam = Camera.main;
            if (!cam) return;

            var ct = cam.transform;
            transform.position = ct.position + ct.forward * distance;
            transform.rotation = Quaternion.LookRotation(ct.forward, ct.up);

            float h = 2f * distance * Tan(cam.fieldOfView * 0.5f * Deg2Rad);
            transform.localScale = new Vector3(h * cam.aspect, h, 1f);

            if (!Application.isPlaying) return;

            phase += spinRate * Time.deltaTime;
            float ang = phase * -2*PI;
            mat.SetVector(Rot, new Vector2(Cos(ang), Sin(ang)));

            mat.SetVector(ST, new Vector4(1, 1, 0, 0));
        }
    }
}