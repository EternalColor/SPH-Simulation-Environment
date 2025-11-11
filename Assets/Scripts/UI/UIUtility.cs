using System.Collections.Generic;
using UnityEngine.UIElements;

namespace SPHSimulator.UI
{
    public static class UIExtensions
    {
        public static void RemoveClassesByNameStartingWith(this VisualElement visualElement, string classNamePrefix)
        {
            IList<string> classNamesToDelete = new List<string>();

            foreach (string className in visualElement.GetClasses())
            {
                if (className.StartsWith(classNamePrefix))
                {
                    classNamesToDelete.Add(className);
                }
            }

            foreach(string classNameToDelete in classNamesToDelete)
            {
                visualElement.RemoveFromClassList(classNameToDelete);
            }
        }
    }
}