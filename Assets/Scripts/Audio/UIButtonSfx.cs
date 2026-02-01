using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Audio
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSfx : MonoBehaviour
    {

        private void Awake() => GetComponent<Button>().onClick.AddListener(() => AudioManager.Instance.PlaySfx("click"));
    }
}