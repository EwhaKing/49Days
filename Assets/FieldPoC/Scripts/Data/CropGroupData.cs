using System;
using System.Collections.Generic;

[Serializable]
public class CropGroupData
{
    public string cropName;
    public List<CropSlotData> slots = new List<CropSlotData>();
}
