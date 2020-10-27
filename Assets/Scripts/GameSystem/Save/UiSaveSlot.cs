using Tomato.Core.GameSystem.Save;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// Script attached to object that loads a save file.
/// </summary>
/// <remarks>
/// <see cref="SetGameInstance"/> should be the first thing to call. To make full use of this script, call
/// <see cref="SetSlotIndex"/>. This script also handles scene loading if given a SceneAsset
/// through <see cref="SetDestination"/>.
/// </remarks>
public class UiSaveSlot : MonoBehaviour
{
    private int _index = 0;
    private GameConfiguration _gameConfiguration;
    // private SceneAsset _destinationScene;
    private SaveIO _saveIo;
    private SaveLocator _saveLocator;
    private Button _button;

    public void SetGameInstance(GameInstance gameInstance)
    {
        _saveIo = gameInstance.SaveIO;
        _saveLocator = gameInstance.SaveLocator;
        _gameConfiguration = gameInstance.gameConfiguration;
    }

    public void SetSlotIndex(int index)
    {
        Assert.IsTrue(_gameConfiguration != null);
        Assert.IsTrue(index < _gameConfiguration.maxSaveSlots);
        Assert.IsTrue(_saveIo != null);
        _index = index;

        /*
         * Note to self:
         * I had issues about understanding when Start is called. I assumed that Start is always
         * called first such that when the object is instantiated, it calls Start. Apparently,
         * I'm calling this method before Start here is called. This causes a button null
         * exception error
         */
        _button = GetComponent<Button>();
        _button.interactable = _saveIo.RequestExecutor()
            .AtSlotIndex(_index)
            .DoesExist();
    }

    // public void SetDestination(SceneAsset destinationScene)
    // {
    //     // _destinationScene = destinationScene;
    // }

    public void OnClick()
    {
        // todo: load save slot and go to destination
        Assert.IsTrue(_saveIo != null && _saveLocator != null);
        SaveData saveData = _saveIo.RequestExecutor()
            .AtSlotIndex(_index)
            .LoadSlot();
        _saveLocator.Provide(saveData);

        // if (_destinationScene != null)
        // {
        //     SceneManager.LoadScene(_destinationScene.name);
        // }
    }
}