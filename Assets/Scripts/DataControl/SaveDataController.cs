using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.Security.Cryptography;



//최종 버전에서는 데이터를 로드하여 DontDestroyGeneral로 복사하고 클라이언트를 종료하기 전까지 DontDestroyGeneral에서 데이터를 가져오는 방식을 사용해야 한다.
public class SaveDataController : MonoBehaviour {

    static public SaveDataController instance = null;

    XmlDocument xmlDoc = new XmlDocument();

    //public Dictionary<string, int[]> userEquipItem;
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



    public void SaveEncryptTest()
    {
        string filepath = Application.persistentDataPath + "/encTest.xml";
        //경로에 있는 파일을 xmlDoc로 불러오기
        xmlDoc.Load(filepath);
        XmlElement elmRoot = xmlDoc.DocumentElement;

        string data = DataEncryptDecrypt.encryData(elmRoot.InnerXml);
        elmRoot.RemoveAll();
        elmRoot.InnerText = data;
        //xmlDoc에 있는 데이터를 경로에 저장
        xmlDoc.Save(filepath);
    }

    public void LoadDecryptTest()
    {
        string filepath = Application.persistentDataPath + "/encTest.xml";
        xmlDoc.Load(filepath);
        XmlElement elmRoot = xmlDoc.DocumentElement;
        string data = DataEncryptDecrypt.Decrypt(elmRoot.InnerText);
        elmRoot.InnerXml = data;

        MemoryStream xmlStream = new MemoryStream();
        xmlDoc.Save(xmlStream);
        xmlStream.Flush();//Adjust this if you want read your data 
        xmlStream.Position = 0;

        List<ArrayList> temp = new List<ArrayList>();
        List<ArrayList> loadedData = new List<ArrayList>();
        var serializer = new XmlSerializer(typeof(List<ArrayList>));
        using (var stream = xmlStream)
        {
            var other = (List<ArrayList>)(serializer.Deserialize(stream));
            temp.Clear();
            temp.AddRange(other);
        }
        loadedData = temp;
    }



    public void SaveCharacterData(List<ArrayList> AllCharData)
    {
        
        var serializer = new XmlSerializer(typeof(List<ArrayList>));
        using (StreamWriter stream = new StreamWriter(new FileStream(Application.persistentDataPath + "/characterDataTemp.xml", FileMode.Create), Encoding.UTF8))
        {
            serializer.Serialize(stream, AllCharData);
        }

        string filepath = Application.persistentDataPath + "/characterDataTemp.xml";
        //경로에 있는 파일을 xmlDoc로 불러오기
        xmlDoc.Load(filepath);
        XmlElement elmRoot = xmlDoc.DocumentElement;

        string data = DataEncryptDecrypt.encryData(elmRoot.InnerXml);
        elmRoot.RemoveAll();
        elmRoot.InnerText = data;
        //xmlDoc에 있는 데이터를 경로에 저장
        string filepath2 = Application.persistentDataPath + "/characterData.xml";
        xmlDoc.Save(filepath2);
    }

    public List<ArrayList> LoadCharacterData(List<ArrayList> AllCharData)
    {
        string filepath = Application.persistentDataPath + "/characterData.xml";
        xmlDoc.Load(filepath);
        XmlElement elmRoot = xmlDoc.DocumentElement;
        string data = DataEncryptDecrypt.Decrypt(elmRoot.InnerText);
        elmRoot.InnerXml = data;

        MemoryStream xmlStream = new MemoryStream();
        xmlDoc.Save(xmlStream);
        xmlStream.Flush();//Adjust this if you want read your data 
        xmlStream.Position = 0;

        List<ArrayList> temp = new List<ArrayList>();
        List<ArrayList> loadedData = new List<ArrayList>();
        var serializer = new XmlSerializer(typeof(List<ArrayList>));
        using (var stream = xmlStream)
        {
            var other = (List<ArrayList>)(serializer.Deserialize(stream));
            temp.Clear();
            temp.AddRange(other);
        }
        loadedData = temp;
        return loadedData;
    }

    public List<ArrayList> LoadCharacterData()
    {
        string filepath = Application.persistentDataPath + "/characterData.xml";
        xmlDoc.Load(filepath);
        XmlElement elmRoot = xmlDoc.DocumentElement;
        string data = DataEncryptDecrypt.Decrypt(elmRoot.InnerText);
        elmRoot.InnerXml = data;

        MemoryStream xmlStream = new MemoryStream();
        xmlDoc.Save(xmlStream);
        xmlStream.Flush();//Adjust this if you want read your data 
        xmlStream.Position = 0;

        List<ArrayList> temp = new List<ArrayList>();
        List<ArrayList> loadedData = new List<ArrayList>();
        var serializer = new XmlSerializer(typeof(List<ArrayList>));
        using (var stream = xmlStream)
        {
            var other = (List<ArrayList>)(serializer.Deserialize(stream));
            temp.Clear();
            temp.AddRange(other);
        }
        loadedData = temp;

        return loadedData;
    }

    public List<ArrayList> ArrayToList(string[,] arr)
    {
        List<ArrayList> totalMapData = new List<ArrayList>();
        //arraylist와 list의 길이 제한
        int listLength = arr.GetLength(0);
        int arrayListLength = arr.GetLength(1);

        //Debug.Log(listLength);
        //Debug.Log(arrayListLength); 

        //arr의 i번째항 j번째 열을 ArrayList에 저장
        for (int i = 0; i < listLength; i++)
        {
            //새 arraylist 선언 및 arraylist에 데이터 저장
            ArrayList xArrayData = new ArrayList();
            for (int j = 0; j < arrayListLength; j++)
            {
                xArrayData.Add(arr[i, j]);
            }
            //list에 저장
            totalMapData.Add(xArrayData);
        }

        return totalMapData;
    }



    public void SaveMapData(List<ArrayList> mapData)
    {
        var serializer = new XmlSerializer(typeof(List<ArrayList>));
        using (StreamWriter stream = new StreamWriter(new FileStream(Application.persistentDataPath + "/dat/mapData.xml", FileMode.Create), Encoding.UTF8))
        {
            serializer.Serialize(stream, mapData);
        }
    }

    public List<ArrayList> LoadMapData()
    {
        List<ArrayList> temp = new List<ArrayList>();
        List<ArrayList> loadedData = new List<ArrayList>();
        var serializer = new XmlSerializer(typeof(List<ArrayList>));
        using (var stream = File.OpenRead(Application.persistentDataPath + "/dat/mapData.xml"))
        {
            var other = (List<ArrayList>)(serializer.Deserialize(stream));
            temp.Clear();
            temp.AddRange(other);
        }
        loadedData = temp;

        return loadedData;
    }
    
    public List<ArrayList> LoadCsvData(string filePath)
    {
        TextAsset csvFile = Resources.Load(filePath) as TextAsset;
        string originText = csvFile.text;

        string[] splitByLine = originText.Split('\n');
        string[] _splitByComma = splitByLine[0].Split(',');
        int splitLength = _splitByComma.Length;

        //Debug.Log(splitByLine.Length);
        //Debug.Log(splitLength);

        string[,] splitByComma = new string[splitByLine.Length, splitLength];
        int[,] parse2Int = new int[splitByLine.Length, splitLength];

        List<ArrayList> list = new List<ArrayList>();
        
        for (int i = 0; i < splitByLine.Length - 1; i++) //가로 구분
        {
            ArrayList arrlist = new ArrayList();
            for (int j = 0; j < splitLength; j++) //세로 구분
            {               
                splitByComma[i, j] = splitByLine[i].Split(',')[j];
                splitByComma[i, j] = splitByComma[i, j].Replace("@", System.Environment.NewLine);
                try
                {
                    arrlist.Add(splitByComma[i, j]);
                }
                catch (FormatException)
                {
                }               
            }
            list.Add(arrlist);
        }
        
        return list;
    }

    public void SaveArrayList(ArrayList data, string filename)
    {
        var serializer = new XmlSerializer(typeof(ArrayList));
        using (StreamWriter stream = new StreamWriter(new FileStream(Application.persistentDataPath + "/dat/" + filename + "Temp.xml", FileMode.Create), Encoding.UTF8))
        {
            serializer.Serialize(stream, data);
        }

        string filepath = Application.persistentDataPath + "/dat/" + filename + "Temp.xml";
        //경로에 있는 파일을 xmlDoc로 불러오기
        xmlDoc.Load(filepath);
        XmlElement elmRoot = xmlDoc.DocumentElement;

        string _data = DataEncryptDecrypt.encryData(elmRoot.InnerXml);
        elmRoot.RemoveAll();
        elmRoot.InnerText = _data;
        //xmlDoc에 있는 데이터를 경로에 저장
        string filepath2 = Application.persistentDataPath + "/dat/" + filename + ".xml";
        xmlDoc.Save(filepath2);


    }

    public ArrayList LoadFromResources(string filename)
    {
        ArrayList temp = new ArrayList();
        ArrayList loadedData = new ArrayList();
        var serializer = new XmlSerializer(typeof(ArrayList));

        TextAsset firstData = Resources.Load("characterStartData/" + filename) as TextAsset;
        using (var sr = new StreamReader(new MemoryStream(firstData.bytes)))
        {
            var other = (ArrayList)(serializer.Deserialize(sr));
            temp.Clear();
            temp.AddRange(other);
        }

        loadedData = temp;
        return loadedData;
    }

    //public ArrayList LoadFromResourcesProgress

    public ArrayList LoadArrayList(string filename)
    {
        //#if (UNITY_ANDROID && !UNITY_EDITOR)

        //#else
        string filepath = Application.persistentDataPath + "/dat/" + filename + ".xml";
        xmlDoc.Load(filepath);
        XmlElement elmRoot = xmlDoc.DocumentElement;
        string data = DataEncryptDecrypt.Decrypt(elmRoot.InnerText);
        elmRoot.InnerXml = data;

        MemoryStream xmlStream = new MemoryStream();
        xmlDoc.Save(xmlStream);
        xmlStream.Flush();//Adjust this if you want read your data 
        xmlStream.Position = 0;

        ArrayList temp = new ArrayList();
        ArrayList loadedData = new ArrayList();
        var serializer = new XmlSerializer(typeof(ArrayList));
        using (var stream = xmlStream)
        {
            var other = (ArrayList)(serializer.Deserialize(stream));
            temp.Clear();
            temp.AddRange(other);
        }
        loadedData = temp;
        return loadedData;


        //File.OpenRead(Application.persistentDataPath + "/" + filename + ".xml") <- 수정하기 전의 경로

        //#endif
        /*
        using (var stream = File.OpenRead(Application.persistentDataPath + "/progress.xml"))
        {
            var other = (ArrayList)(serializer.Deserialize(stream));
            temp.Clear();
            temp.AddRange(other);
        }
        */


    }

    public ArrayList LoadArrayListTemp(string filename)
    {


        //#if (UNITY_ANDROID && !UNITY_EDITOR)



        //#else
 

        ArrayList temp = new ArrayList();
        ArrayList loadedData = new ArrayList();
        var serializer = new XmlSerializer(typeof(ArrayList));
        using (var stream = File.OpenRead(Application.persistentDataPath + "/" + filename + ".xml"))
        {
            var other = (ArrayList)(serializer.Deserialize(stream));
            temp.Clear();
            temp.AddRange(other);
        }
        loadedData = temp;
        return loadedData;


        //File.OpenRead(Application.persistentDataPath + "/" + filename + ".xml") <- 수정하기 전의 경로

        //#endif
        /*
        using (var stream = File.OpenRead(Application.persistentDataPath + "/progress.xml"))
        {
            var other = (ArrayList)(serializer.Deserialize(stream));
            temp.Clear();
            temp.AddRange(other);
        }
        */


    }


    public void Save(List<string> myItem, string currentItem)
    {
        var serializer = new XmlSerializer(typeof(List<string>));
        using (StreamWriter stream = new StreamWriter (new FileStream (Application.persistentDataPath + "/" + currentItem + ".xml", FileMode.Create), Encoding.UTF8))
        {
            serializer.Serialize(stream, myItem);
        }

        //Debug.Log(Application.persistentDataPath);
    }
    /*
        public void Load(List<string> myItem, string currentItem)
        {
            var serializer = new XmlSerializer(typeof(List<string>));
            using (var stream = File.OpenRead(Application.persistentDataPath + "/" + currentItem + ".xml"))
            {
                var other = (List<string>)(serializer.Deserialize(stream));
                myItem.Clear();
                myItem.AddRange(other);
            }

            playerData = new PlayerData();
            if(currentItem == "equip")
            {
                playerData.userEquipItem = myItem;
            }
            else if(currentItem == "clothes")
            {
                playerData.userClothesItem = myItem;
            }
            else if (currentItem == "expendable")
            {
                playerData.userExpendableItem = myItem;
            }

            //Debug.Log("playerData = " + playerData.userClothesItem[0]);
        }

        public void SaveGold(int gold)
        {
            PlayerPrefs.SetInt("gold", gold);
            PlayerPrefs.Save();
        }

        public int LoadGold()
        {
            int gold;
            gold = PlayerPrefs.GetInt("gold", 0);

            return gold; 
        }

        public void LoadAndAddStat()
        {
            //default data 로드
            List<ArrayList> character = LoadCharacterData();
            int charNum = DontDestoryGeneral.instance.nowCharacter;

            DontDestoryGeneral.instance.defaultStat = new ArrayList();

            for (int i = 0; i < 4; i++)
            {
                DontDestoryGeneral.instance.defaultStat.Add(character[charNum][i+2]);
            }

            int defStr = Convert.ToInt32(DontDestoryGeneral.instance.defaultStat[0]);
            int defInt = Convert.ToInt32(DontDestoryGeneral.instance.defaultStat[1]);
            int defHp = Convert.ToInt32(DontDestoryGeneral.instance.defaultStat[2]);
            int defMana = Convert.ToInt32(DontDestoryGeneral.instance.defaultStat[3]);

            //add data 로드
            //장비 추가 스탯
            //장비에서 데이터 로드
            //무기 : str, int, melee, magic
            List<ArrayList> equipAdd = LoadEquipDataXml();
            int equipStr = Convert.ToInt32(equipAdd[1][3]);
            int equipInt = Convert.ToInt32(equipAdd[1][4]);
            int equipMelee = Convert.ToInt32(equipAdd[1][9]);
            int equipMagic = Convert.ToInt32(equipAdd[1][10]);

            //장비 : hp, mana, healthpoint, manapoint, defense, dodge
            int equipHp = Convert.ToInt32(equipAdd[2][5]);
            int equipMana = Convert.ToInt32(equipAdd[2][6]);
            int equipHealthPoint = Convert.ToInt32(equipAdd[2][7]);
            int equipManaPoint = Convert.ToInt32(equipAdd[2][8]);
            int equipDefense = Convert.ToInt32(equipAdd[2][11]);
            int equipDodge = Convert.ToInt32(equipAdd[2][12]);

            DontDestoryGeneral.instance.addStat = new ArrayList();

            DontDestoryGeneral.instance.addStat.Add(equipStr);
            DontDestoryGeneral.instance.addStat.Add(equipInt);
            DontDestoryGeneral.instance.addStat.Add(equipHp);
            DontDestoryGeneral.instance.addStat.Add(equipMana);
            DontDestoryGeneral.instance.addStat.Add(equipHealthPoint);
            DontDestoryGeneral.instance.addStat.Add(equipManaPoint);
            DontDestoryGeneral.instance.addStat.Add(equipMelee);
            DontDestoryGeneral.instance.addStat.Add(equipMagic);
            DontDestoryGeneral.instance.addStat.Add(equipDefense);
            DontDestoryGeneral.instance.addStat.Add(equipDodge);

            //스킬 추가 스탯
            //합산 스탯 저장
            DontDestoryGeneral.instance.totalStat = new ArrayList();

            for (int i = 0; i < 4; i++)
            {
                DontDestoryGeneral.instance.totalStat.Add(Convert.ToInt32(DontDestoryGeneral.instance.defaultStat[i]) + Convert.ToInt32(DontDestoryGeneral.instance.addStat[i]));
            }

            int totalStr = Convert.ToInt32(DontDestoryGeneral.instance.totalStat[0]);
            int totalInt = Convert.ToInt32(DontDestoryGeneral.instance.totalStat[1]);
            int totalHp = Convert.ToInt32(DontDestoryGeneral.instance.totalStat[2]);
            int totalMana = Convert.ToInt32(DontDestoryGeneral.instance.totalStat[3]);
            //HP, MP 계산
            int defHealthPoint = DontDestoryGeneral.instance.HPMPPointCalc(totalStr, totalHp);
            int defManaPoint = DontDestoryGeneral.instance.HPMPPointCalc(totalInt, totalMana);

            //공격력 계산
            int defMelee = DontDestoryGeneral.instance.DmgCalc(totalStr, totalHp);
            int defMagic = DontDestoryGeneral.instance.DmgCalc(totalInt, totalMana);
            //방어, 회피율 계산 (실제로 사용할 때는 /1000)
            float defDefense = DontDestoryGeneral.instance.ProtectionCalc(totalHp, totalMana);
            float defDodge = 0;

            DontDestoryGeneral.instance.defaultStat.Add(defHealthPoint);
            DontDestoryGeneral.instance.defaultStat.Add(defManaPoint);
            DontDestoryGeneral.instance.defaultStat.Add(defMelee);
            DontDestoryGeneral.instance.defaultStat.Add(defMagic);
            DontDestoryGeneral.instance.defaultStat.Add(defDefense);
            DontDestoryGeneral.instance.defaultStat.Add(defDodge);

            for (int i = 4; i < 10; i++)
            {
                DontDestoryGeneral.instance.totalStat.Add(Convert.ToSingle(DontDestoryGeneral.instance.defaultStat[i]) + Convert.ToSingle(DontDestoryGeneral.instance.addStat[i]));
            }
        }
    */
}

public class DataEncryptDecrypt
{
    public static string encryData(string toEncrypt)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12345678901234567890123456789012");

        byte[] toEmcryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
        RijndaelManaged rDel = new RijndaelManaged();

        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;

        rDel.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = rDel.CreateEncryptor();

        byte[] resultArray = cTransform.TransformFinalBlock(toEmcryptArray, 0, toEmcryptArray.Length);

        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    public static string Decrypt(string toDecrypt)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12345678901234567890123456789012");

        byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;

        rDel.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = rDel.CreateDecryptor();

        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return UTF8Encoding.UTF8.GetString(resultArray);
    }
}

