using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

public class SaveLoadManager : SceneSingleton<SaveLoadManager>
{
    public Action onSave;
    public Action onLoad;
    int date;
    string saveDirectory;

    public void Save<T>(T dataClass)
    {
        string json = JsonConvert.SerializeObject(dataClass, Formatting.Indented);
        File.WriteAllText($"{saveDirectory}/{typeof(T).Name}", json);
    }

    public T Load<T>()
    {
        string json = File.ReadAllText($"{saveDirectory}/{typeof(T).Name}");
        return JsonConvert.DeserializeObject<T>(json);
    }
    public void SaveAllByDate(int date)
    {
        this.date = date;
        saveDirectory = GetSaveDirectory();
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        onSave?.Invoke();
        Debug.Log($"데이터 저장됨: {saveDirectory}");
    }

    public void LoadAllByDate(int date)
    {
        this.date = date;
        saveDirectory = GetSaveDirectory();

        onLoad?.Invoke();
        Debug.Log($"데이터 로드됨: {saveDirectory}");
    }

    private string GetSaveDirectory()
    {
#if UNITY_EDITOR
        // 프로젝트 폴더 밑에 저장 (Assets 폴더와 같은 레벨)
        return Path.Combine(Application.dataPath, "SaveData", date.ToString());
#else
        // 빌드 환경에서는 persistentDataPath
        return Path.Combine(Application.persistentDataPath, date.ToString());
#endif
    }
}