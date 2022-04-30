using System.Collections.Generic;
using System;
using UnityEngine;

namespace _Project.UI.Scripts.Tutorial
{
    /// <summary>
    /// Class that manages the tutorial tasks
    /// </summary>
    [Serializable]
    public class TutorialTasks
    {
        [SerializeField]
        private List<TutorialTask> tasks;
        private List<TutorialTask> completedTasks = new List<TutorialTask>();

        private int index;

        public bool IsFinished()
        {
            return tasks.Count == 0;
        }

        public string GetName()
        {
            return tasks[index].Name;
        }

        public string GetDescription()
        {
            return tasks[index].Description;
        }

        public float GetPercentage()
        {
            if (tasks.Count == 0)
                return 1;

            if (completedTasks.Count == 0)
                return 0;

            return (completedTasks.Count) / (float)(tasks.Count + completedTasks.Count);
        }

        /// <summary>
        /// Complete a tutorial task
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns> Whether identifier was found </returns>
        public bool CompleteTask(string identifier)
        {
            if (index >= tasks.Count)
                return false;

            if (tasks[index].Identifier != identifier)
                return false;

            completedTasks.Add(tasks[index]);
            tasks.RemoveAt(index);

            if (tasks.Count > 0)
                index = index % tasks.Count;

            return true;
        }
    }
}

