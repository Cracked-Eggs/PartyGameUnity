using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luakite
{
    public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
    {
        [SerializeField] private AudioSource hoverSound;

        public void OnPointerEnter(PointerEventData ped)
        {
            hoverSound.Play();
            // SoundManager.Play("MouseOverButton");
        }

        public void OnPointerDown(PointerEventData ped)
        {
            // SoundManager.Play("MouseClickButton");
        }
    }
}
