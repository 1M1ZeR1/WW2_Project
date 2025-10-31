using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INeedTime
{
    public Action<object> Completed { get; set; }
}
