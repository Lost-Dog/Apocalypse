using System.Collections.Generic;

namespace JU.SaveLoad
{
    /// <summary>
    /// Save the progress of all <see cref="JUSaveLoadComponent"/> into the save file.
    /// </summary>
    public static class JUSaveLoadManager
    {
        private static List<ISave> _objectsToSave;

        /// <summary>
        /// Save the game progression on the save file.
        /// </summary>
        public static void SaveOnFile()
        {
            if (_objectsToSave != null)
            {
                // Ensure all components are synchronized before write on save.
                foreach (var item in _objectsToSave)
                {
                    if (item != null)
                        item.Save();
                }
            }

            JUSaveLoad.Save();
        }

        /// <summary>
        /// Add component to save when <see cref="SaveOnFile"/> is called.
        /// </summary>
        /// <param name="save"></param>
        public static void AddObjectToSave(ISave save)
        {
            if (save == null)
                return;

            if (_objectsToSave == null)
                _objectsToSave = new List<ISave>();

            _objectsToSave.Add(save);
        }

        /// <summary>
        /// Remove save component.
        /// </summary>
        /// <param name="save"></param>
        public static void RemoveObjectToSave(ISave save)
        {
            if (_objectsToSave == null || _objectsToSave.Count == 0 || save == null)
                return;

            _objectsToSave.Remove(save);
        }
    }
}