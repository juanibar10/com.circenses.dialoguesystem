using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "New character", menuName = "Character", order = 0)]
    public class Character : ScriptableObject
    {
        public string Name;
        public Sprite image;
    }
}