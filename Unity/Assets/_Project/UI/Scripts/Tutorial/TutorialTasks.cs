using System.Collections.Generic;
using System;
using UnityEngine;

namespace _Project.UI.Scripts.Tutorial
{
    /// <summary>
    /// Simple class that describes a tutorial task.
    /// </summary>
    [Serializable]
    public class Task
    {
        /// <summary>
        /// Identifier of this task.
        /// </summary>
        [SerializeField]
        private string identifier;
        public string Identifier
        {
            get { return identifier; }
            set { identifier = value; }
        }

        /// <summary>
        /// Name of this task.
        /// </summary>
        [SerializeField]
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Description of this task that has to be completed by the user.
        /// </summary>
        [SerializeField]
        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

    }


    /// <summary>
    /// Class that manages the tutorial tasks
    /// </summary>
    [Serializable]
    public class Tasks
    {
        [SerializeField]
        private int optionalTasksStart;

        [SerializeField]
        private List<Task> tasks;
        private List<Task> completedTasks = new List<Task>();

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

