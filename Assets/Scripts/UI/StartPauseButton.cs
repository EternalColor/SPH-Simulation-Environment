using UnityEngine;
using UnityEngine.UIElements;

namespace SPHSimulator.UI
{
    [UxmlElement]
    public partial class StartPauseButton : Button
    {
        [UxmlAttribute]
        public Texture2D StartIcon { get; set; }

        [UxmlAttribute]
        public Texture2D PauseIcon { get; set; }

        [UxmlAttribute]
        public bool IsRunning 
        {
            get
            {
                return isRunning;
            }
            set
            {
                isRunning = value;
                iconImage = isRunning ? PauseIcon : StartIcon;
            }
        }

        private bool isRunning;

        public StartPauseButton()
        {
            RegisterCallback<ClickEvent>(ToggleState);
        }

        private void ToggleState(ClickEvent clickEvent)
        {
            IsRunning = !IsRunning;
        }
    }
}