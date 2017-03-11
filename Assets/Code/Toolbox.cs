using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbox : Singleton<Toolbox>
{
    protected Toolbox() { } // guarantee this will be always a singleton only - can't use the constructor!

    public Language language = new Language();

    void Awake()
    {
        // Your initialization code here
    }

    // (optional) allow runtime registration of global objects
    static public T RegisterComponent<T>() where T : Component
    {
        return Instance.GetOrAddComponent<T>();
    }
}

[System.Serializable]
public class Language
{
    public string current;
    public string lastLang;
}