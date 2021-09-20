using Entities;
using UnityEngine;

public delegate void NoArgsHandler();
public delegate void BoolHandler(bool Value);
public delegate void BoolBoolHandler(bool Value1, bool Value2);
public delegate void IntHandler(int Value);
public delegate void Vector2IntHandler(Vector2Int Value);
public delegate void V2IntHandler(V2Int Value);
public delegate void V2IntV2IntHandler(V2Int Value1, V2Int Value2);
public delegate void Vector2IntVector2IntHandler(Vector2Int Value1, Vector2Int Value2);
public delegate void LongHandler(long Value);
public delegate void FloatHandler(float Value);
public delegate void DoubleHandler(double Value);
