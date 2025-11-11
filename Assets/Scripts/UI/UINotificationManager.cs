using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

namespace SPHSimulator.UI
{
    public class UINotificationManager : MonoBehaviour
    {
        private VisualElement notificationRoot;
        private Label notificationLabel;
        private readonly Dictionary<ActionNotificationType, Color> colorMap = new Dictionary<ActionNotificationType, Color>
        {
            { ActionNotificationType.Error, Color.softRed },
            { ActionNotificationType.Success, Color.softGreen},
            { ActionNotificationType.Warning, Color.orange},
        };

        public void Initialize(VisualElement root)
        {
            notificationRoot = root.Q<VisualElement>("NotificationContainer");
            notificationLabel = root.Q<Label>("NotificationLabel");

            notificationRoot.style.display = DisplayStyle.None;
        }

        public void ShowNotification(ActionNotificationType notificationType, string message, float duration = 3f)
        {
            StopAllCoroutines();

            notificationLabel.text = message;
            notificationRoot.style.display = DisplayStyle.Flex;
            notificationRoot.AddToClassList("show");

            notificationRoot.style.borderTopWidth = 4;
            notificationRoot.style.borderRightWidth = 4;
            notificationRoot.style.borderBottomWidth = 4;
            notificationRoot.style.borderLeftWidth = 4;

            notificationRoot.style.borderTopColor = colorMap[notificationType];
            notificationRoot.style.borderRightColor = colorMap[notificationType];
            notificationRoot.style.borderBottomColor = colorMap[notificationType];
            notificationRoot.style.borderLeftColor = colorMap[notificationType];

            StartCoroutine(HideAfterDelay(duration));

        }
        public void ShowNotification(string message, float duration = 3f)
        {
            StopAllCoroutines();

            notificationLabel.text = message;
            notificationRoot.style.display = DisplayStyle.Flex;
            notificationRoot.AddToClassList("show");

            StartCoroutine(HideAfterDelay(duration));
        }

        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            notificationRoot.RemoveFromClassList("show");
            yield return new WaitForSeconds(0.5f);
            notificationRoot.style.display = DisplayStyle.None;
        }
    }
}