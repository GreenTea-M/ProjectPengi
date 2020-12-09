using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Gameplay;
using GameSystem;
using GameSystem.Save;
using Others;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Dialog
{
    public class IconManager : MonoBehaviour, SaveClientCallback
    {
        public GameConfiguration gameConfiguration;
        public InformSpeakerReturn informSpeakerReturnValue = new InformSpeakerReturn();
        public UnifiedCharacterScript currentSpeaking;

        private SaveClient _saveClient;
        private UnifiedCharacterScript[] _characterList;
        private UnifiedCharacterScript _mainCharacter;
        private UnifiedCharacterScript _narratingCharacter;
        private readonly List<UnifiedCharacterScript> _activeCharacterList = new List<UnifiedCharacterScript>();

        private void OnEnable()
        {
            if (_saveClient == null)
            {
                _saveClient = gameConfiguration.RequestSaveAccess(this);
            }
        }

        private void OnDisable()
        {
            gameConfiguration.ReleaseSaveAccess(_saveClient);
            _saveClient = null;
        }

        private void Awake()
        {
            Debug.Assert(gameConfiguration != null);

            var characterPrefabList = gameConfiguration.characterPrefabList;
            _characterList = new UnifiedCharacterScript[characterPrefabList.Length];
            for (int i = 0; i < characterPrefabList.Length; i++)
            {
                _characterList[i] = InstantiateCharacter(characterPrefabList[i]);
            }

            _narratingCharacter = InstantiateCharacter(gameConfiguration.narratingCharacterPrefab);
            _mainCharacter = InstantiateCharacter(gameConfiguration.mainCharacterPrefab);

            if (_saveClient == null)
            {
                _saveClient = gameConfiguration.RequestSaveAccess(this);
            }
        }

        private UnifiedCharacterScript InstantiateCharacter(GameObject prefab)
        {
            var script = Object.Instantiate(prefab)
                .GetComponent<UnifiedCharacterScript>();
            Debug.Assert(script != null);
            script.SetData(this);
            return script;
        }

        public InformSpeakerReturn InformSpeaker(string candidateSpeaker)
        {
            // find out who is the active speaker
            // arrange accordingly
            bool shouldRearrange = false;

            // extract description
            var speakingParts = candidateSpeaker.Split(',');
            candidateSpeaker = speakingParts[0];
            var description = speakingParts.Length != 2 ? "" : speakingParts[1];

            if (_narratingCharacter.IsSimilar(candidateSpeaker))
            {
                currentSpeaking = _narratingCharacter;
            }
            else if (_mainCharacter.IsSimilar(candidateSpeaker))
            {
                currentSpeaking = _mainCharacter;
            }
            else
            {
                foreach (var character in _activeCharacterList)
                {
                    if (character.IsSimilar(candidateSpeaker))
                    {
                        currentSpeaking = character;
                        shouldRearrange = true;
                        break;
                    }
                }

                if (!shouldRearrange)
                {
                    Debug.LogWarning($"Speaker not found: {candidateSpeaker}");
                }
            }

            if (shouldRearrange)
            {
                _activeCharacterList.Remove(currentSpeaking);
                _activeCharacterList.Insert(0, currentSpeaking);
            }

            _narratingCharacter.UpdateStatus(description);
            _mainCharacter.UpdateStatus(description);

            foreach (var character in _activeCharacterList)
            {
                character.UpdateStatus(description);
            }

            informSpeakerReturnValue.character = currentSpeaking;
            informSpeakerReturnValue.realName = currentSpeaking.RealName;

            return informSpeakerReturnValue;
        }

        public void ShowElements(bool shouldShow)
        {
            if (!shouldShow)
            {
                foreach (var characterScript in _characterList)
                {
                    characterScript.Leave();
                }

                _mainCharacter.Leave();
                _narratingCharacter.Leave();
            }
        }

        public void WriteAutoSave()
        {
            _saveClient.autoSave.SetActiveSpeakerList(_activeCharacterList);
        }

        public void EnterStage(string characterName)
        {
            if (DoesActiveCharacterListContain(characterName))
            {
                return;
            }

            foreach (var characterScript in _characterList)
            {
                if (characterScript.IsSimilar(characterName))
                {
                    _activeCharacterList.Add(characterScript);
                    return;
                }
            }

            Debug.LogWarning($"EnterStage: Character {characterName} not in character list");
        }

        private bool DoesActiveCharacterListContain(string characterName)
        {
            foreach (var characterScript in _activeCharacterList)
            {
                if (characterScript.IsSimilar(characterName))
                {
                    return true;
                }
            }

            return false;
        }

        public void ExitStage(string characterName)
        {
            for (int i = 0; i < _activeCharacterList.Count; i++)
            {
                if (_activeCharacterList[i].IsSimilar(characterName))
                {
                    _activeCharacterList[i].Leave();
                    _activeCharacterList.RemoveAt(i);
                    return;
                }
            }

            Debug.LogWarning($"ExitStage: Character {characterName} not in character list");
        }

        public void CreateButtons(int optionsLength)
        {
            _mainCharacter.CreateButtons(optionsLength);
        }

        public void ActivateButtons(int i, UnityAction action)
        {
            _mainCharacter.ActivateButtons(i, action);
        }

        public void SetButtonText(int i, string optionText)
        {
            _mainCharacter.SetButtonText(i, optionText);
        }

        public void HideAllButtons()
        {
            _mainCharacter.HideAllButtons();
            _narratingCharacter.ResetTextLocation();
        }

        public int GetSideCharacterIndex(UnifiedCharacterScript unifiedCharacterScript)
        {
            return _activeCharacterList.IndexOf(unifiedCharacterScript);
        }

        public void InformShowingOptions()
        {
            _narratingCharacter.SetTextAlternativeLocation();
        }

        public void UpdateAlternativeTextLocation(TextAlternativeLocationState alternativeLocationState)
        {
            _narratingCharacter.SetAlternativeLocationState(alternativeLocationState);
            _mainCharacter.SetAlternativeLocationState(alternativeLocationState);
            foreach (var characterScript in _characterList)
            {
                characterScript.SetAlternativeLocationState(alternativeLocationState);
            }
        }
    }

    public class InformSpeakerReturn
    {
        public UnifiedCharacterScript character;
        public readonly DialogueBlocker dialogueBlocker = new DialogueBlocker();
        public string realName = "";
    }

    public class DialogueBlocker
    {
        private bool _isBlocking = false;
        public bool IsBlocking => _isBlocking;

        public void Unblock()
        {
            _isBlocking = false;
        }

        public void Block()
        {
            _isBlocking = true;
        }
    }
}