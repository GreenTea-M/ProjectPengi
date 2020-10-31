using GameSystem.Save;
using Tomato.Core.GameSystem.Save;
using UnityEngine;
using Yarn.Unity;

/// <summary>
/// <c>GameInstance</c> primarily serves to contain several Subsystems or ServiceLocators
/// and it should only be used to access those.
/// </summary>
/// <remarks>
/// It may house some other variables for prototyping but those should be enclosed in either of the
/// two types of components that this Instance should hold.
/// </remarks>
/// <example>
/// To access GameInstance, call <c>GameConfiguration.GetGameInstance</c>.
/// </example>
/// <see cref="GameConfiguration"/>
public class GameInstance : MonoBehaviour
{
    private static GameInstance _instance = null;

    [Header("Variables")] [Tooltip("Used to access configurations that may be relevant for gameplay and debugging")]
    public GameConfiguration gameConfiguration;

    [Tooltip("Required component for YarnSpinner")]
    public DialogueRunner dialogueRunner;

    [Header("Debug GameSave")] [Tooltip("Toggle whether to use a supplied SaveGame")]
    public bool useDebugGameSave = false;

    [Tooltip("Save data for a specific debugging case. If null, SaveLocator uses null Save Data.")]
    public DebugSaveData debugSaveData;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // endregion singleton pattern

        // init
        SaveLocator = new SaveLocator();

        // debug
        if (useDebugGameSave && debugSaveData != null)
        {
            SaveLocator.Provide(debugSaveData.saveData);
        }
    }

    public static GameInstance Instance => _instance;

    public SaveLocator SaveLocator { get; private set; }
    public SaveIO SaveIO { get; private set; }
}