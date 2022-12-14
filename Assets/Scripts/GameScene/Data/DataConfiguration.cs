using System;
using UnityEngine;

namespace HBStudio.Test.Data
{
    [CreateAssetMenu(menuName = "Data")]
    public class DataConfiguration : ScriptableObject
    {
        [Header("Game Configuration")]
        [SerializeField] private Configuration _configuration;
        public Configuration Configuration => _configuration;
    }

    [Serializable]
    public struct Configuration
    {
        public float JerkDistance;
        public float DurationInvincibilityMode;
        public int CountWinsToWin;
    }
}