using System.Collections.Generic;
using UnityEngine;

namespace HauntedNight.Util
{
    /// <summary>Minimal pool for VFX/audio props. Avoids GC spikes on mobile.</summary>
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int prewarm = 8;

        private readonly Queue<GameObject> _free = new();

        private void Awake()
        {
            for (int i = 0; i < prewarm; i++) _free.Enqueue(Create());
        }

        private GameObject Create()
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            return go;
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var go = _free.Count > 0 ? _free.Dequeue() : Create();
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            return go;
        }

        public void Return(GameObject go, float delay = 0f)
        {
            if (delay > 0f) StartCoroutine(ReturnLater(go, delay));
            else ReturnNow(go);
        }

        private System.Collections.IEnumerator ReturnLater(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnNow(go);
        }

        private void ReturnNow(GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            _free.Enqueue(go);
        }
    }
}
