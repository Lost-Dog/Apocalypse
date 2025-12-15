using JU.SaveLoad.Serialization;
using JU.SaveLoad.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace JU.SaveLoad
{
    /// <summary>
    /// Save and load files that contains data for each game scene or global data values.
    /// </summary>
    public static class JUSaveLoad
    {
        // Stores values that can be used on all program independent of the scene.
        private class GlobalData
        {
            public Dictionary<string, object> Data;

            public GlobalData()
            {
                Data = new Dictionary<string, object>();
            }
        }

        // Stores values for each scene.
        private class SceneData
        {
            public Dictionary<string, object> Data;

            public SceneData()
            {
                Data = new Dictionary<string, object>();
            }
        }

        // Contains all save file data.
        private class Data
        {
            public GlobalData GlobalData;
            public Dictionary<string, SceneData> Scenes;

            public Data()
            {
                Scenes = new Dictionary<string, SceneData>();
                GlobalData = new GlobalData();
            }
        }

        private static Data _data;
        private static JsonSerializerSettings _serializationSettings;

        /// <summary>
        /// Called on write the save file.
        /// </summary>
        public static event Action Saved;

        /// <summary>
        /// Called on load the save file.
        /// </summary>
        public static event Action Loaded;

        /// <summary>
        /// If true, it's will give log messages informing the save/load actions.
        /// </summary>
        public static bool ShowDebugLogs { get; set; }

        /// <summary>
        /// The final folder that will be saved the file with the data (without the file name). 
        /// </summary>
        public static string SaveFileFolder
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            get => Path.Combine(Application.dataPath, "..", "Saves");
#else
            get => Path.Combine(Application.persistentDataPath, "Saves");
#endif
        }

        /// <summary>
        /// The final full path that contains the file, including the file name.
        /// </summary>
        public static string SaveFilePath => Path.Combine(SaveFileFolder, "Save.bin");

        /// <summary>
        /// The final full path that contains the backup file, including the file name.
        /// </summary>
        public static string SavebackupFilePath => Path.Combine(SaveFileFolder, "Save.backup");

        private static void SetupSerializationSettings()
        {
            if (_serializationSettings == null)
                _serializationSettings = new JsonSerializerSettings();

            AddTypeConverter(new JUSerializeVector2());
            AddTypeConverter(new JUSerializeVector3());
            AddTypeConverter(new JUSerializeVector4());
            AddTypeConverter(new JUSerializeQuaternion());
        }

        private static void SaveData(Data data)
        {
            if (_serializationSettings == null)
                SetupSerializationSettings();

            if (data == null)
                data = new Data();

            if (!Directory.Exists(SaveFilePath))
                Directory.CreateDirectory(SaveFileFolder);

            using FileStream file = File.Create(SaveFilePath);

            if (ShowDebugLogs)
                Debug.Log($"Saving game data: {SavebackupFilePath}");

            try
            {
                byte[] serializedData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, Formatting.Indented, _serializationSettings));
                using (AesManaged aes = new AesManaged())
                {
                    GetCredentials(out byte[] key, out byte[] iv);

                    aes.Key = key;
                    aes.IV = iv;

                    using ICryptoTransform cryptoTransform = aes.CreateEncryptor(key, iv);
                    using (CryptoStream cryptoStream = new CryptoStream(file, cryptoTransform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(serializedData, 0, serializedData.Length);

                        if (ShowDebugLogs)
                            Debug.Log("Save updated.");

                        Saved?.Invoke();
                    }
                }

                CreateSaveBackup();
            }
            catch (IOException error)
            {
                Debug.LogError("IO error on save data" + error);
            }
            catch (CryptographicException error)
            {
                Debug.LogError("Cryptograph error on save data" + error);
            }
            catch (Exception error)
            {
                Debug.LogError("Error on save data" + error);
            }
            finally
            {
                file.Close();
            }
        }

        private static Data LoadData()
        {
            if (LoadData(SaveFilePath, out Data data))
            {
                if (ShowDebugLogs)
                    Debug.Log("Save loaded.");

                return data;
            }

            if (LoadData(SavebackupFilePath, out Data backupData))
            {
                if (ShowDebugLogs)
                    Debug.Log("Backup save loaded, can't load the original save file.");

                return backupData;
            }

            else
                return new Data();
        }

        private static bool LoadData(string path, out Data data)
        {
            if (_serializationSettings == null)
                SetupSerializationSettings();

            if (!File.Exists(path))
            {
                data = null;
                return false;
            }
            try
            {
                using (AesManaged aes = new AesManaged())
                {
                    byte[] fileBits = File.ReadAllBytes(path);
                    GetCredentials(out byte[] key, out byte[] iv);
                    aes.Key = key;
                    aes.IV = iv;

                    using ICryptoTransform cryptoTransform = aes.CreateDecryptor(
                        aes.Key,
                        aes.IV
                    );

                    using MemoryStream memoryStream = new MemoryStream(fileBits);
                    using CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        string result = streamReader.ReadToEnd();
                        data = JsonConvert.DeserializeObject<Data>(result, _serializationSettings);

                        Loaded?.Invoke();

                        return true;
                    }
                }
            }
            catch (IOException error)
            {
                Debug.Log("IO error: " + error);
                data = null;
                return false;
            }
            catch (CryptographicException error)
            {
                Debug.Log("Cryptograph error: " + error);
                data = null;
                return false;
            }
            catch (Exception error)
            {
                Debug.Log("Error: " + error);
                data = null;
                return false;
            }
        }

        private static void GetCredentials(out byte[] key, out byte[] iv)
        {
            byte[] keyRaw = Encoding.UTF8.GetBytes("Bla bla bla bla bla!");
            byte[] ivRaw = Encoding.UTF8.GetBytes("Du da da du da");

            key = new byte[32];
            iv = new byte[16];

            Array.Copy(keyRaw, key, Math.Min(keyRaw.Length, key.Length));
            Array.Copy(ivRaw, iv, Math.Min(ivRaw.Length, iv.Length));
        }

        private static void SetSceneData(string sceneName, SceneData sceneData)
        {
            Load();

            if (_data.Scenes.ContainsKey(sceneName))
                _data.Scenes[sceneName] = sceneData;
            else
                _data.Scenes.Add(sceneName, sceneData);
        }

        private static SceneData GetSceneData(string sceneName)
        {
            Load();

            if (_data.Scenes.ContainsKey(sceneName))
                return _data.Scenes[sceneName];

            var newScene = new SceneData();
            _data.Scenes.Add(sceneName, newScene);
            return newScene;
        }

        private static GlobalData GetGlobalData()
        {
            Load();
            return _data.GlobalData;
        }

        private static void SetGlobalData(GlobalData globalData)
        {
            Load();

            _data.GlobalData = globalData;
        }

        private static T ConvertTo<T>(object value)
        {
            if (value == null)
                return default;

            if (value is T)
                return (T)value;
            else if (JUSaveLoadUtilities.CanConvertTo<T>(value))
                return JUSaveLoadUtilities.ConvertTo<T>(value);

            Debug.LogError($"The value returns a {value.GetType()} but are you trying get a {typeof(T)}. \nInvalid casting error.");
            return default;
        }

        /// <summary>
        /// Add or replace a value from a scene of the save data. <para/>
        /// Don't fongot to call <see cref="Save"/> after set the values.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="sceneName">The scene that will contain this value.</param>
        /// <param name="key">The key used as ID of this value. Use the ID to load this value after, or replace this current value.<para/>
        ///  Can be like "playerLife" as example.</param>
        /// <param name="value">The value that will be assigned.</param>
        public static void SetSceneValue<T>(string sceneName, string key, T value)
        {
            SceneData sceneData = GetSceneData(sceneName);
            if (sceneData.Data.ContainsKey(key))
                sceneData.Data[key] = value;
            else
                sceneData.Data.Add(key, value);

            SetSceneData(sceneName, sceneData);
        }

        /// <summary>
        /// Try get a value from save data.
        /// </summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="sceneName">The scene that contains the value.</param>
        /// <param name="key">The ID of the value that you can load. Can be like "playerLife" as example.</param>
        /// <param name="value">The value that will be returned if exist on save data.</param>
        /// <param name="defaultValue">The default value that will be returned the key if not exist on the save data.</param>
        /// <returns>Return true if exist the data with the designed key but return false if the value does not exist.</returns>
        public static bool TryGetSceneValue<T>(string sceneName, string key, out T value, T defaultValue = default)
        {
            SceneData sceneData = GetSceneData(sceneName);
            if (sceneData.Data.TryGetValue(key, out var loadedValue))
            {
                value = ConvertTo<T>(loadedValue);
                return true;
            }

            value = defaultValue;
            return false;
        }

        /// <summary>
        /// Check if a value exist.
        /// </summary>
        /// <param name="sceneName">The scene that contains the value.</param>
        /// <param name="key">The ID of the value that you can load. Can be like "playerLife" as example.</param>
        /// <returns>Return true if the value with the key exist.</returns>
        public static bool HasSceneValue(string sceneName, string key)
        {
            SceneData sceneData = GetSceneData(sceneName);
            return sceneData.Data.ContainsKey(key);
        }

        /// <summary>
        /// Returns a value from the save file.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="sceneName">The scene that contain the value.</param>
        /// <param name="key">The ID of the value that you can load. Can be like "playerLife" as example.</param>
        /// <param name="defaultValue">The default value that will be returned the key if not exist on the save data.</param>
        /// <returns></returns>
        public static T GetSceneValue<T>(string sceneName, string key, T defaultValue = default)
        {
            SceneData sceneData = GetSceneData(sceneName);

            if (sceneData.Data.TryGetValue(key, out object value))
                return ConvertTo<T>(value);

            return defaultValue;
        }

        /// <summary>
        /// Delete a value from a scene if exist on the save.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="key"></param>
        public static void DeleteSceneValue(string sceneName, string key)
        {
            SceneData sceneData = GetSceneData(sceneName);
            if (sceneData.Data.ContainsKey(key))
                sceneData.Data.Remove(key);
        }

        /// <summary>
        /// Delete ALL save data from a scene.
        /// </summary>
        /// <param name="sceneName"></param>
        public static void DeleteSceneData(string sceneName)
        {
            Load();

            if (_data.Scenes.ContainsKey(sceneName))
                _data.Scenes.Remove(sceneName);
        }

        /// <summary>
        /// Get a value stored on the save data, global data can be accessed independent of the scene. Coins, as example.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The ID of the value that you can load. Can be like "playerLife" as example.</param>
        /// <param name="defaultValue">The default value that will be returned the key if not exist on the save data.</param>
        /// <returns></returns>
        public static T GetGlobalValue<T>(string key, T defaultValue = default)
        {
            GlobalData globalData = GetGlobalData();

            if (globalData.Data.TryGetValue(key, out object value))
                return ConvertTo<T>(value);

            return defaultValue;
        }

        /// <summary>
        /// Try get a value stored on the save data, global data can be accessed independent of the scene. Coins, as example.
        /// </summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="key">The ID of the value that you can load. Can be like "playerLife" as example.</param>
        /// <param name="value">The value that will be loaded if exist.</param>
        /// <param name="defaultValue">The default value that will be returned the key if not exist on the save data.</param>
        /// <returns>Return true if the value with the key exist.</returns>
        public static bool TryGetGlobalValue<T>(string key, out T value, T defaultValue = default)
        {
            GlobalData globalData = GetGlobalData();

            if (globalData.Data.TryGetValue(key, out object loadedValue))
            {
                value = ConvertTo<T>(loadedValue);
                return true;
            }

            value = defaultValue;
            return false;
        }

        /// <summary>
        /// Check if a value exist.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        /// <returns>Return true if exist a value with the key.</returns>
        public static bool HasGlobalValue(string key)
        {
            GlobalData globalData = GetGlobalData();
            return globalData.Data.ContainsKey(key);
        }

        /// <summary>
        /// Add or replace a value from the global data of the save data. <para/>
        /// Don't fongot to call <see cref="Save"/> after set the values.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key used as ID of this value. Use the ID to load this value after, or replace this current value.<para/>
        ///  Can be like "playerLife" as example.</param>
        /// <param name="value">The value that will be assigned.</param>
        public static void SetGlobalValue<T>(string key, T value)
        {
            GlobalData globalData = GetGlobalData();
            if (globalData.Data.ContainsKey(key))
                globalData.Data[key] = value;
            else
                globalData.Data.Add(key, value);

            SetGlobalData(globalData);
        }

        /// <summary>
        /// Remove a data from the save, if exist.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        public static void DeleteGlobalValue(string key)
        {
            GlobalData globalData = GetGlobalData();
            if (globalData.Data.ContainsKey(key))
                globalData.Data.Remove(key);
        }

        /// <summary>
        /// Load the data from the save file, if exists
        /// using the <see cref="SaveFilePath"/>.<para/>
        /// <param name="force">Force load value from save file. Usefull if the game must discart the unsaved game progress to load the save from the lasted save-point.</param>
        /// </summary>
        public static void Load(bool force = false)
        {
            if (force || _data == null)
                _data = LoadData();
        }

        /// <summary>
        /// Save all data on the save file using the <see cref="SaveFilePath"/>.
        /// </summary>
        public static void Save()
        {
            SaveData(_data);
        }

        /// <summary>
        /// Delete all save files using the <see cref="SaveFileFolder"/>.
        /// </summary>
        public static void DeleteAllSaves()
        {
            if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
            if (File.Exists(SavebackupFilePath)) File.Delete(SavebackupFilePath);

            if (ShowDebugLogs)
                Debug.Log("All saves deleted");
        }

        /// <summary>
        /// This can create a save backup file, if have a save file on the <see cref="SaveFilePath"/>.
        /// </summary>
        public static void CreateSaveBackup()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Copy(SaveFilePath, SavebackupFilePath, true);

                if (ShowDebugLogs)
                    Debug.Log("Backup save updated.");
            }
        }

        /// <summary>
        /// Pass a custom <see cref="JsonConverter"/> to deserialize custom value types.
        /// </summary>
        /// <param name="converter"></param>
        public static void AddTypeConverter(JsonConverter converter)
        {
            if (_serializationSettings == null)
                SetupSerializationSettings();

            if (!_serializationSettings.Converters.Contains(converter))
                _serializationSettings.Converters.Add(converter);
        }
    }
}