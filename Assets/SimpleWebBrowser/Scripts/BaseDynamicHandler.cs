#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Collections;
using System.Collections.Generic;
using SimpleWebBrowser;
using UnityEngine;

public class BaseDynamicHandler : MonoBehaviour,IDynamicRequestHandler {
    public virtual string Request(string url, string query) {
        return null;
    }
}
#endif
