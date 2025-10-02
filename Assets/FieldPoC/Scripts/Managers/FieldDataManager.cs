using System.Collections.Generic;
using UnityEngine;

public class FieldDataManager : SceneSingleton<FieldDataManager>
{
    public List<CropGroupData> cropGroups = new List<CropGroupData>();
    public float timeElapsedInField = 0f;

    void OnEnable()
    {
        SaveLoadManager.Instance.onSave += Save;
        SaveLoadManager.Instance.onLoad += Load;
    }

    void OnDisable()
    {
        SaveLoadManager.Instance.onSave -= Save;
        SaveLoadManager.Instance.onLoad -= Load;
    }

    public void Save()
    {
        SaveLoadManager.Instance.Save(cropGroups);
    }

    public void Load()
    {
        var loaded = SaveLoadManager.Instance.Load<List<CropGroupData>>();
        if (loaded != null)
            cropGroups = loaded;
    }

    public CropGroupData GetGroupData(string name)
    {
        var g = cropGroups.Find(x => x.cropName == name);
        if (g == null)
        {
            g = new CropGroupData { cropName = name };
            cropGroups.Add(g);
        }
        return g;
    }
}
