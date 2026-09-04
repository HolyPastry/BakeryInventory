using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bakery
{
    public class InstancePool<T> : IEnumerable<T> where T : MonoBehaviour
    {
        private readonly List<T> _pool = new();
        private readonly T _prefab;
        private readonly Transform _parent;

        public InstancePool(T prefab, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
        }

        ~InstancePool()
        {
            while (_pool.Count > 0)
            {
                var instance = _pool[0];
                _pool.RemoveAt(0);
                GameObject.Destroy(instance);
            }
        }

        public T Add()
        {
            var obj = _pool.FirstOrDefault(item => !item.gameObject.activeSelf);
            if (obj == null)
            {
                obj = GameObject.Instantiate(_prefab, _parent);
                _pool.Add(obj);
            }
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Remove(T obj)
        {
            obj.gameObject.SetActive(false);
        }

        public void Clear()
        {
            foreach (var obj in _pool)
            {
                obj.gameObject.SetActive(false);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _pool.Where(item => item.gameObject.activeSelf).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
