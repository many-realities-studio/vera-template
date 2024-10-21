using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VERAColumnDefinition", menuName = "ScriptableObjects/VERAColumnDefinition", order = 1)]
public class VERAColumnDefinition : ScriptableObject
{
    [Serializable]
    public class Column
    {
        public string name;
        public DataType type;
    }

    public enum DataType
    {
        String,
        Number,
        Transform,
        JSON,
        Date
    }

    public List<Column> columns = new List<Column>();
}
