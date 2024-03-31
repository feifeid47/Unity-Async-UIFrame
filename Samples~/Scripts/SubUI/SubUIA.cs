using Feif.Extensions;
using Feif.UIFramework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
#if USING_UNITASK
using Task = Cysharp.Threading.Tasks.UniTask;
using Cysharp.Threading.Tasks;
#else
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
#endif

namespace Feif.UI
{
    public class SubUIA : UIBase
    {
        [SerializeField] private Text txtContent;

        private int refreshCount = 0;

        protected override async Task OnRefresh()
        {
            Debug.Log("Refresh SubUIA");
            using (var request = UnityWebRequest.Get("https://cube.meituan.com/ipromotion/cube/toc/component/base/getServerCurrentTime"))
            {
                using (var response = await request.SendWebRequest())
                {
                    if (response.responseCode != 200)
                    {
                        txtContent.text = "网络请求失败";
                        return;
                    }
                    var json = response.downloadHandler.text;
                    txtContent.text = $"刷新次数 = {++refreshCount}\n{json}";
                }
            }
        }

        [UGUIButtonEvent("@BtnHide")]
        protected void OnClickBtnHide()
        {
            UIFrame.Hide(this);
        }

        [UGUIButtonEvent("@BtnRefresh")]
        protected void OnClickBtnRefresh()
        {
            // 刷新当前UI
            UIFrame.Refresh(this);
        }
    }
}