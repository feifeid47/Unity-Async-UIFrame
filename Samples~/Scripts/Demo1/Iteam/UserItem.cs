using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Feif.UIFramework;

namespace Feif.UI
{
    public class UserItemData : UIData
    {
        public StarData User;
    }

    public class UserItem : UIComponent<UserItemData>
    {
        [SerializeField] private Image imgUser;
        [SerializeField] private Text txtName;
        [SerializeField] private Button btnDelete;

        private Sprite sprite;

        protected override async Task OnCreate()
        {
            if (string.IsNullOrEmpty(this.Data.User.name))
            {
                txtName.text = this.Data.User.login;
            }
            else
            {
                txtName.text = this.Data.User.name;
            }
            // 下载头像
            Debug.Log("下载头像中：" + this.Data.User.avatar_url);
            sprite = await NetworkResources.LoadSpriteAsync(this.Data.User.avatar_url);
            if (sprite == null)
            {
                imgUser.sprite = null;
                txtName.text = "错误";
            }
            else
            {
                imgUser.sprite = sprite;
            }
        }

        protected override void OnBind()
        {
            btnDelete.onClick.AddListener(OnBtnDelete);
        }

        protected override void OnUnbind()
        {
            btnDelete.onClick.RemoveListener(OnBtnDelete);
        }

        private void OnBtnDelete()
        {
            // 注意：
            // 创建UI元素使用 UIFrame.Instantiate
            // 销毁UI元素使用 UIFrame.Destroy或UIFrame.DestroyImmediate
            // 不要使用GameObject.Instantiate，GameObject.Destroy，GameObject.DestroyImmediate

            // 销毁UI元素
            UIFrame.Destroy(this.gameObject);
        }

        protected override void OnDied()
        {
            // OnCreate中加载的资源在OnDied中销毁或释放
            GameObject.Destroy(sprite);
        }

    }
}