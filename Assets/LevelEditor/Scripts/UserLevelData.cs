using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PropertyData
{
    public string componentType;  // 脚本全名
    public string fieldName;      // 字段名
    public string value;          // 字符串形式的值
}

[Serializable]
public class PlacedItemData
{
    public int      id;
    public string   mechanismTypeAddress;
    public Vector3  position;
    public Vector3  rotation;
    public Vector3  scale;
    public int[]    linkedIds;

    public List<PropertyData> properties;  // 新增：保存所有可序列化字段
}

[Serializable]
public class UserLevelData
{
    public List<PlacedItemData> items;
}
