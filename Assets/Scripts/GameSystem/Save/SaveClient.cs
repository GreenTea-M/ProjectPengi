using System.Collections.Generic;
using UI;
using Yarn.Unity;

namespace GameSystem.Save
{
    public class SaveClient
    {
        public SaveData currentSave;
        public SaveData autoSave;
        public SaveClientCallback saveClientCallback;

        public void TryAutoSaveWrite()
        {
            saveClientCallback.WriteAutoSave();
        }
    }

    public interface SaveClientCallback
    {
        void WriteAutoSave();
    }
}