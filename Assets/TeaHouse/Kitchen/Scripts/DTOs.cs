using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum EvaluationResult
{
    Excellent,
    Good,
    Bad
}

[JsonConverter(typeof(StringEnumConverter))]
public enum TeaName
{
    Unknown,
    BlackTea,
    GreenTea,
    OolongTea,
    WhiteTea,
    HerbalTea
}

public record MakedTea
{
    public TeaName TeaName;
    public EvaluationResult Evaluation;
}