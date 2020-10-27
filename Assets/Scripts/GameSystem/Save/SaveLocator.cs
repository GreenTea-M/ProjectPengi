namespace Tomato.Core.GameSystem.Save
{
    /// <summary>
    /// This class locates the needed SaveData
    /// </summary>
    public class SaveLocator
    {
        private SaveData _nullSave;
        private SaveData _currentSave;

        public SaveLocator()
        {
            _nullSave = SaveData.AsNull();
            _currentSave = _nullSave;
        }

        public void Provide(SaveData saveData)
        {
            if (saveData != null)
            {
                _currentSave = saveData;
            }
            else
            {
                _currentSave = _nullSave;
            }
        }

        public SaveData GetSaveData()
        {
            return _currentSave;
        }
    }
}
