using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteScript : MonoBehaviour
{
    public void DeleteThisParent()
    {
        Destroy(gameObject.transform.parent.gameObject);
    }
}
