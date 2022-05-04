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
        private int optionalTasksStart = 1;

        [SerializeField]
        private List<Task> tasks = new List<Task>();

        private int index = 0;

        public bool IsRequired()
        {
            return index < optionalTasksStart;
        }

        public bool RequiredTasksFinished()
        {
            return index >= optionalTasksStart - 1;
        }

        public bool IsLastRequiredTask()
        {
            return index == optionalTasksStart- 1;
        }

        public string GetName()
        {
            if (index >= tasks.Count) return "";
            return tasks[index].Name;
        }

        public string GetDescription()
        {
            if (index >= tasks.Count) return "";
            return tasks[index].Description;
        }

        public string GetIdentifier()
        {
            if (index >= tasks.Count) return "";
            return tasks[index].Identifier;
        }

        public float GetPercentage()
        {
            int completed = IsRequired() ? index : index - optionalTasksStart;
            int total = IsRequired() ? optionalTasksStart - 1 : tasks.Count - optionalTasksStart - 1;
            if (total == 0) return 1f;
            return completed / (float)total;
        }

        /// <summary>
        /// Complete a tutorial task
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns> Whether identifier was found </returns>
        public bool CompleteTask(string identifier)
        {
            if (index >= tasks.Count - 1) return false;
            if (GetIdentifier() != identifier) return false;
            index++;
            return true;
        }

        /// <summary>
        /// Skip a tutorial task
        /// </summary>
        /// <returns> Whether a task was skipped </returns>
        public bool SkipTask()
        {
            return CompleteTask(GetIdentifier());
        }
    }
}

