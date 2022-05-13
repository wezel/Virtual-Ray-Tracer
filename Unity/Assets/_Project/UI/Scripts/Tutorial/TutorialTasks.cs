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
        [TextArea(10, 20)]
        [SerializeField]
        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Whether this task is skippable (by pressing a skip button)
        /// Used for when there is no task involved, just an explanation text
        /// </summary>
        [SerializeField]
        public bool skippable;
        public bool Skippable
        {
            get { return skippable; }
            set { skippable = value; }
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

        public bool IsSkippable()
        {
            if (index >= tasks.Count) return false;
            return tasks[index].Skippable;
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

        public float RequiredPercentage()
        {
            int total = optionalTasksStart - 1;
            if (total == 0) return 1f;
            return Math.Min(index / (float)total, 1f);
        }

        public float OptionalPercentage()
        {
            if (index < optionalTasksStart) return 0f;
            int total = tasks.Count - optionalTasksStart - 1;
            if (total == 0) return 1f;
            return Math.Max(0f, (index - optionalTasksStart) / (float)total);
        }

        public float GetTotalPercentage()
        {
            if (tasks.Count == 0) return 0f;
            return index / (float)tasks.Count;
        }

        public int CurrentTaskIndex()
        {
            return index;
        }

        public int TotalTasksCount()
        {
            return tasks.Count;
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

