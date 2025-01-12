using System;
using UnityEngine;

namespace CSSD 
{
    public class ScoreDealer : MonoBehaviour
    {
        [SerializeField] private int _score = 10;

        public static Action<int> scoreDel;

        private void OnDisable()
        {
            scoreDel?.Invoke(_score);            
        }
    }
}