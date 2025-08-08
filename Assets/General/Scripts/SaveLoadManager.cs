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

    public void Save<T>(T dataClass)
    {
        string directory = Application.persistentDataPath + "/" + date;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        string json = JsonConvert.SerializeObject(dataClass, Formatting.Indented);
        File.WriteAllText(directory + "/" + typeof(T).Name, json);
    }

    public T Load<T>()
    {
        string json = File.ReadAllText($"{Application.persistentDataPath}/{date}/{typeof(T).Name}");
        return JsonConvert.DeserializeObject<T>(json);
    }

    public void SaveAllByDate(int date)
    {
        this.date = date;
        onSave?.Invoke();
        Debug.Log($"데이터 저장됨: {Application.persistentDataPath}/{date}");
    }

    public void LoadAllByDate(int date)
    {
        this.date = date;
        onLoad?.Invoke();
        Debug.Log($"데이터 로드됨: {Application.persistentDataPath}/{date}");
    }
}
