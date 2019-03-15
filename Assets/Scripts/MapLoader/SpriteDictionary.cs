using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class SpriteDic : SerializableDictionaryBase<string, Sprite> { }

[System.Serializable]
public class LargeSpriteDicComponent : SerializableDictionaryBase<string, Sprite> { }

[System.Serializable]
public class LargeSpriteDicVariatoin : SerializableDictionaryBase<string, LargeSpriteDicComponent> { }

[System.Serializable]
public class LargeSpriteDic : SerializableDictionaryBase<string, LargeSpriteDicVariatoin> { }

public class SpriteDictionary : MonoBehaviour
{
    static public SpriteDictionary instance = null;
    //public SpriteDic roadSprDic;
    //public SpriteDic houseSprDic;
    public SpriteDic emptySprDic;
    //public SpriteDic industryDic;
    //public SpriteDic commercialDic;
    //public SpriteDic jobDic;
    //public SpriteDic parkDic;

    public LargeSpriteDic smallSprDic;
    public LargeSpriteDic largeSprDic;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
