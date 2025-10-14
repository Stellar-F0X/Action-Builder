using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ActionBuilder.Runtime
{
    public static class Extensions
    {
        /// <summary> 현재 Transform을 포함한 모든 하위 자식에서 지정된 태그를 가진 첫 번째 Transform을 찾습니다. </summary>
        /// <param name="parent">검색을 시작할 Transform</param>
        /// <param name="tag">찾을 태그</param>
        /// <returns>해당 태그를 가진 첫 번째 Transform, 없으면 null</returns>
        public static Transform FindWithTag(this Transform parent, string tag)
        {
            // 현재 transform이 해당 태그인지 먼저 확인
            if (parent.CompareTag(tag))
            {
                return parent;
            }

            // 모든 하위 자식들을 재귀적으로 검색
            foreach (Transform child in parent)
            {
                Transform found = child.FindWithTag(tag);
                
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
        

        /// <summary> 현재 Transform을 포함한 모든 하위 자식에서 지정된 태그를 가진 모든 Transform을 찾습니다. </summary>
        /// <param name="parent">검색을 시작할 Transform</param>
        /// <param name="tag">찾을 태그</param>
        /// <returns>해당 태그를 가진 모든 Transform 배열</returns>
        public static Transform[] FindAllWithTag(this Transform parent, string tag)
        {
            List<Transform> transforms = ListPool<Transform>.Get();
            FindAllWithTagRecursive(parent, tag, transforms);
            Transform[] result = transforms.ToArray();
            ListPool<Transform>.Release(transforms);
            return result;
        }
        

        private static void FindAllWithTagRecursive(Transform parent, string tag, List<Transform> results)
        {
            // 현재 transform이 해당 태그인지 확인
            if (parent.CompareTag(tag))
            {
                results.Add(parent);
            }

            // 모든 하위 자식들을 재귀적으로 검색
            foreach (Transform child in parent)
            {
                FindAllWithTagRecursive(child, tag, results);
            }
        }
    }
}
