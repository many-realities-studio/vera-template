using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CSVColumnDefinition", menuName = "ScriptableObjects/CSVColumnDefinition", order = 1)]
public class CSVColumnDefinition : ScriptableObject
{
    [Serializable]
    public class Column
    {
        public string name;
        public DataType type;
    }

    public enum DataType
    {
        Number,
        String,
        JSON
    }

    public List<Column> columns = new List<Column>();
}
