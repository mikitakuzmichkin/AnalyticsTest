using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DefaultNamespace
{
    public class Test : MonoBehaviour
    {
        private async void Awake()
        {
            var analytics = new AnalyticService();
            analytics.Init("test_err", 3);
            analytics.TrackEvent("startTest", "start good");
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            analytics.TrackEvent("secondEvent", "start good");
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            analytics.TrackEvent("additional", "add additional");
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            analytics.TrackEvent("error", "send error");
            analytics.TrackEvent("reload", "reload");
            analytics.Init("test", 3);
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            analytics.TrackEvent("additional", "add additional_2");
            analytics.TrackEvent("send", "good");
            analytics.Dispose();
        }
    }
}