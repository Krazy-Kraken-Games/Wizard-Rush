using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public interface IStoring
{
    //This will be applied on stations to determine storing of items

    void AddItem(FixedString128Bytes item);
}
