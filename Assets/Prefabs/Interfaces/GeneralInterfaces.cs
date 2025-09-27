using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IName
{
    string Name { get; set; }
}
public interface IType<TEnum> where TEnum : Enum
{
    TEnum Type { get; set;}
}
public interface ISide
{
   SideEnum Side { get; set; }
}
