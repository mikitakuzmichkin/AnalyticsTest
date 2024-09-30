using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AnalyticService : IDisposable
{
    private const string _EVENT_PACKAGE_KEY = "eventPackageKey";
    private const string _ADDINIONAL_EVENT_PACKAGE_KEY = "additionalEventPackageKey";

    private string _serverUrl;
    private float _cooldownBeforeSend;
    private JArray _eventPackage = new JArray();
    private JArray _additionalEventPackage = new JArray();
    private bool _isMainPackageBusy;

    public void Init(string serverUrl, float cooldownBeforeSend = 3f)
    {
        _serverUrl = serverUrl;
        _cooldownBeforeSend = cooldownBeforeSend;
        Load();
    }

    public void TrackEvent(string type, string data)
    {
        var newEvent = new JObject();
        newEvent["type"] = type;
        newEvent["data"] = data;

        if (_isMainPackageBusy == false)
        {
            _eventPackage.Add(newEvent);
            if (_eventPackage.Count == 1)
            {
                StartSendTimeOut().Forget();
            }
        }
        else
        {
            _additionalEventPackage.Add(newEvent);
        }
        Save();
    }

    public void ForceSend()
    {
        Send().Forget();
    }

    private async UniTaskVoid StartSendTimeOut()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(_cooldownBeforeSend));
        
        if (_eventPackage != null && _eventPackage.Count > 0)
        {
            Send().Forget();
        }
    }

    private async UniTaskVoid Send()
    {
        if (_eventPackage == null || _eventPackage.Count < 1 || _isMainPackageBusy)
        {
            return;
        }
        
        var body = new JObject();
        body["events"] = _eventPackage;

        try
        {
            _isMainPackageBusy = true;
            if (_serverUrl.Contains("test"))
            {
                await UniTask.Delay(new TimeSpan(0,0, 3));
                Debug.Log(body);
                UpdateResult(_serverUrl.Contains("err") == false);
            }
            else
            {
                var request = (await UnityWebRequest.Post(_serverUrl, body.ToString()).SendWebRequest());
               UpdateResult(request.result == UnityWebRequest.Result.Success);
            }
        }
        finally
        {
            _isMainPackageBusy = false;
        }

        if (_eventPackage != null && _eventPackage.Count > 0)
        {
            StartSendTimeOut().Forget();
        }
    }

    private void UpdateResult(bool isSuccessResult)
    {
        if (isSuccessResult)
        {
            _eventPackage = _additionalEventPackage;
        }
        else
        {
            if (_additionalEventPackage != null && _additionalEventPackage.Count > 0)
            {
                foreach (var token in _additionalEventPackage)
                {
                    _eventPackage.Add(token);
                }
            }
        }
        _additionalEventPackage = new JArray();
        Save();
    }

    private void Save()
    {
        if (_eventPackage != null && _eventPackage.Count > 0)
        {
            var eventPackageString = _eventPackage.ToString();
            Debug.Log("player prefs event package save: \n" + eventPackageString.ToString());
            PlayerPrefs.SetString(_EVENT_PACKAGE_KEY, eventPackageString);
        }
        else
        {
            if (PlayerPrefs.HasKey(_EVENT_PACKAGE_KEY))
            {
                PlayerPrefs.DeleteKey(_EVENT_PACKAGE_KEY);
            }
        }
        
        if (_additionalEventPackage != null && _additionalEventPackage.Count > 0)
        {
            var additionalEventPackageString = _additionalEventPackage.ToString();
            Debug.Log("player prefs additional event package save: \n" + additionalEventPackageString.ToString());
            PlayerPrefs.SetString(_ADDINIONAL_EVENT_PACKAGE_KEY, additionalEventPackageString);
        }
        else
        {
            if (PlayerPrefs.HasKey(_ADDINIONAL_EVENT_PACKAGE_KEY))
            {
                PlayerPrefs.DeleteKey(_ADDINIONAL_EVENT_PACKAGE_KEY);
            }
        }
        
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(_EVENT_PACKAGE_KEY))
        {
            var eventPackageString = PlayerPrefs.GetString(_EVENT_PACKAGE_KEY);
            _eventPackage = JArray.Parse(eventPackageString);
        }
        
        if (PlayerPrefs.HasKey(_ADDINIONAL_EVENT_PACKAGE_KEY))
        {
            var additionalEventPackageString = PlayerPrefs.GetString(_ADDINIONAL_EVENT_PACKAGE_KEY);
            foreach (var token in JArray.Parse(additionalEventPackageString))
            {
                _eventPackage.Add(token);
            }
        }
        
        if (_eventPackage != null && _eventPackage.Count > 0)
        {
            StartSendTimeOut().Forget();
        }
    }

    public void Dispose()
    {
        
    }
}
