namespace mardevmil.Core
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectPooler
    {
        private struct ObjectPoolElement
        {
            public GameObject element;
            public Vector3 position;
            public Vector3 scale;
            public Transform parent;
            public string name;
        }

        private int DEFAULT_AMOUNT = 10;
        private Vector3 _defaultPosition = new Vector3(2000f, 2000f, 2000f);

        private GameObject element;
        private List<GameObject> pool;
        private List<ObjectPoolElement> poolElements;
        
        public ObjectPooler(GameObject gameObject, int amount, Transform parent)
        {
            if(gameObject == null)
            {
                Debug.LogError("Given GameObject for creating pool is null!");
                return;
            }

            GameObject root = null;
            if(parent == null)
            {
                root = new GameObject();
                root.name = gameObject.name + "_Pool";
                root.transform.position = _defaultPosition;
                root.transform.localScale = Vector3.one;
            }

            var rootTransform = parent != null ? parent : root.transform;
            var total = amount > 0 ? amount : DEFAULT_AMOUNT;
            
            pool = new List<GameObject>(total);
            poolElements = new List<ObjectPoolElement>(total);
            for (int i = 0; i < total; i++)
            {
                var elm = Object.Instantiate(gameObject, rootTransform.position, Quaternion.identity, rootTransform);
                elm.name = gameObject.name + "#" + i;
                pool.Add(elm);

                var poolElement = new ObjectPoolElement()
                {
                    element = elm,
                    position = elm.transform.position,
                    scale = elm.transform.localScale,
                    parent = rootTransform,
                    name = elm.name
                };

                poolElements.Add(poolElement);
                elm.SetActive(false);                
            }
        }

        public ObjectPooler(GameObject gameObject) : this(gameObject, 0, null) { }
        public ObjectPooler(GameObject gameObject, int amount) : this(gameObject, amount, null) { }

        public GameObject Get()
        {
            if (pool == null || pool.Count == 0)
                return null;

            var elm = pool[0];
            pool.Remove(elm);
            elm.transform.SetParent(null);
            elm.SetActive(true);
            return elm;
        }

        public void Release(GameObject element)
        {
            if (poolElements == null || poolElements.Count == 0 || pool == null)
                return;
            
            for (int i = 0; i < poolElements.Count; i++)
            {
                if(element == poolElements[i].element)
                {
                    pool.Add(element);
                    element.transform.SetParent(poolElements[i].parent);
                    element.transform.localScale = poolElements[i].scale;
                    element.transform.position = poolElements[i].position;
                    element.SetActive(false);
                }
            }
        }
    }
}

