using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Feif.UIFramework;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
#if USING_UNITASK
using Task = Cysharp.Threading.Tasks.UniTask;
using Cysharp.Threading.Tasks;
#else
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
using Feif.Extensions;
#endif

namespace Feif.UI
{
    public class StarData
    {
        public string login;
        public int id;
        public string avatar_url;
        public string name;
    }

    // 这是一个Window，不需要UIData则继承UIBase，需要UIData则继承UIComponent
    [WindowLayer]
    public class UIStarred : UIBase
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private Button btnClose;
        [SerializeField] private Button btnStar;
        [SerializeField] private GameObject refItem;

        private UIWindowEffect effect;

        protected override Task OnCreate()
        {
            effect = GetComponent<UIWindowEffect>();
            return Task.CompletedTask;
        }

        protected override async Task OnRefresh()
        {
            var list = await GetStarList();

            if (list == null)
            {
                Debug.LogError("网络请求失败");
                return;
            }

            var tasks = new List<Task>();
            // 初始化清空列表
            while (content.childCount > 0)
            {
                // 不能使用UIFrame.Destroy，会死循环
                UIFrame.DestroyImmediate(content.GetChild(0).gameObject);
            }
            // 动态创建UI元素
            foreach (var item in list)
            {
                var data = new UserItemData()
                {
                    User = item
                };
                // 实例化UI元素
                tasks.Add(UIFrame.Instantiate(refItem, content, data));
            }
            await Task.WhenAll(tasks);
        }

        protected override void OnShow()
        {
            // 播放打开Window的动效
            effect.PlayOpen();
        }

        // 关闭按钮事件
        [UGUIButtonEvent("@BtnClose")]
        protected async void OnClickBtnClose()
        {
            // 播放关闭Window的动效
            await effect.PlayClose();
            // 动效播放完成后关闭当前Window
            await UIFrame.Hide(this);
        }

        // 前往关注按钮事件
        [UGUIButtonEvent("@BtnStar")]
        protected void OnClickBtnStar()
        {
            Application.OpenURL("https://github.com/feifeid47/Unity-Async-UIFrame");
        }

        // 获得关注列表
#if USING_UNITASK
        public static async UniTask<List<StarData>> GetStarList()
#else
        public static async Task<List<StarData>> GetStarList()
#endif
        {
            using (var request = UnityWebRequest.Get("https://api.github.com/repos/feifeid47/Unity-Async-UIFrame/stargazers?per_page=100"))
            {
                using (var response = await request.SendWebRequest())
                {
                    if (response.responseCode != 200) return null;

                    var json = response.downloadHandler.text;
                    var regex = new Regex("\"id\": *\\d*");
                    var matches = regex.Matches(json);
                    var result = new List<StarData>();
                    foreach (Match item in matches)
                    {
                        var id = int.Parse(item.Value.Replace("\"id\"", string.Empty).Trim(':').Trim());
                        using (var request2 = UnityWebRequest.Get($"https://api.github.com/user/{id}"))
                        {
                            using (var response2 = await request2.SendWebRequest())
                            {
                                Debug.Log($"获取用户信息：{id}");
                                result.Add(JsonUtility.FromJson<StarData>(response2.downloadHandler.text));
                            }
                        }
                    }
                    return result;
                }
            }
        }
    }
}