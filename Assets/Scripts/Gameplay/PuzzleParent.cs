using System;
using System.Collections;
using System.Collections.Generic;
using Dialog;
using UnityEngine;

namespace Gameplay
{
    public class PuzzleParent : MonoBehaviour
    {
        public float delayedRate = 1f / 10f;
        
        private List<SpritePuzzle> _puzzleItems;
        private CustomCommands _customCommands;

        public void SetCustomCommand(CustomCommands customCommands)
        {
            _puzzleItems = new List<SpritePuzzle>(GetComponentsInChildren<SpritePuzzle>());
            _customCommands = customCommands;
            StartCoroutine(SlowUpdate());
        }
        
        private IEnumerator SlowUpdate()
        {
            while (true)
            {
                if (_puzzleItems.Count != 0)
                {
                    for (int i = _puzzleItems.Count - 1; i >= 0; i--)
                    {
                        if (_puzzleItems[i].IsFinish())
                        {
                            _puzzleItems.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    _customCommands.InformPuzzleDone();
                    break;
                }   
                
                yield return new WaitForSeconds(delayedRate);
            }
        }
    }
}