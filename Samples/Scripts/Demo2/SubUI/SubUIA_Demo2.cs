using Feif.UIFramework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Feif.UI
{
    public class SubUIA_Demo2 : UIBase
    {
        [SerializeField] private Text txtContent;

        private int refreshCount = 0;

        protected override async Task OnRefresh()
        {
            Debug.Log("Refresh SubUIA");
            using var request = UnityWebRequest.Get("https://cube.meituan.com/ipromotion/cube/toc/component/base/getServerCurrentTime");
            using var response = await request.SendWebRequest();
            if (response.result != UnityWebRequest.Result.Success)
            {
                txtContent.text = "网络请求失败";
                return;
            }
            var json = response.downloadHandler.text;
            txtContent.text = $"刷新次数 = {++refreshCount}\n{json}";
        }

        [UGUIButtonEvent]
        protected void OnBtnHide()
        {
            UIFrame.Hide(this);
        }

        [UGUIButtonEvent]
        protected void OnBtnRefresh()
        {
            // 刷新当前UI
            UIFrame.Refresh(this);
        }
    }
}