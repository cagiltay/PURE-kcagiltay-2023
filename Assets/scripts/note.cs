#if UNITY_EDITOR
using UnityEngine;

public class note : MonoBehaviour{
    [TextArea(5, 1000)]
    public string comment = "Leave your notes here";
}
#endif
