using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Feif.UIFramework;

namespace Feif.UI
{
    public class UserItemData_Demo2 : UIData
    {
        public StarData_Demo2 User;
    }

    public class UserItem_Demo2 : UIComponent<UserItemData_Demo2>
    {
        [SerializeField] private Image imgUser;
        [SerializeField] private Text txtName;

        private Sprite sprite;

        protected override async Task OnCreate()
        {
            if (string.IsNullOrEmpty(Data.User.name))
            {
                txtName.text = Data.User.login;
            }
            else
            {
                txtName.text = Data.User.name;
            }
            // 下载头像
            Debug.Log("下载头像中：" + Data.User.avatar_url);
            sprite = await NetworkResources.LoadSpriteAsync(Data.User.avatar_url);
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

        [UGUIButtonEvent]
        protected void OnBtnDelete()
        {
            // 注意：
            // 创建UI元素使用 UIFrame.Instantiate
            // 销毁UI元素使用 UIFrame.Destroy或UIFrame.DestroyImmediate
            // 不要使用GameObject.Instantiate，GameObject.Destroy，GameObject.DestroyImmediate

            // 销毁UI元素
            UIFrame.Destroy(gameObject);
        }

        protected override void OnDied()
        {
            // OnCreate中加载的资源在OnDied中销毁或释放
            GameObject.Destroy(sprite);
        }

    }
}