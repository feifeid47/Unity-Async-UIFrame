using Feif.Extensions;
using Feif.UIFramework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Feif.UI
{
    public class SubUIA : UIBase
    {
        [SerializeField] private Text txtContent;
        [SerializeField] private Button btnRefresh;
        [SerializeField] private Button btnHide;

        private int refreshCount = 0;

        protected override void OnBind()
        {
            btnRefresh.onClick.AddListener(OnBtnRefresh);
            btnHide.onClick.AddListener(OnBtnHide);
        }

        protected override void OnUnbind()
        {
            btnRefresh.onClick.RemoveListener(OnBtnRefresh);
            btnHide.onClick.RemoveListener(OnBtnHide);
        }

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

        private void OnBtnHide()
        {
            UIFrame.Hide(this);
        }

        private void OnBtnRefresh()
        {
            // 刷新当前UI
            UIFrame.Refresh(this);
        }
    }
}