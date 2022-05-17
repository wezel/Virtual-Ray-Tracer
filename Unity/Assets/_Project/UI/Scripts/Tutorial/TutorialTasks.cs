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
        private int completedIndex = 0;

        /// <summary>
        /// Whether the current task is a required one.
        /// </summary>
        /// <returns>Whether the current task is a required one</returns>
        public bool IsRequiredTask()
        {
            return index < optionalTasksStart;
        }

        /// <summary>
        /// Whether the required tasks are finished.
        /// </summary>
        /// <returns>Whether the required tasks are finished</returns>
        public bool AreRequiredTasksFinished()
        {
            return completedIndex >= optionalTasksStart - 1;
        }

        /// <summary>
        /// Whether the current task is skippable.
        /// </summary>
        /// <returns>Whether the current task is skippable</returns>
        public bool IsSkippable()
        {
            if (index < completedIndex) return true;
            if (index >= tasks.Count) return false;
            return tasks[index].Skippable;
        }

        /// <summary>
        /// Get the current tasks' name.
        /// </summary>
        /// <returns>The current tasks' name</returns>
        public string GetName()
        {
            if (index >= tasks.Count) return "";
            return tasks[index].Name;
        }

        /// <summary>
        /// Get the current tasks' description.
        /// </summary>
        /// <returns>The current tasks' description</returns>
        public string GetDescription()
        {
            if (index >= tasks.Count) return "";
            return tasks[index].Description;
        }

        /// <summary>
        /// Get the current tasks' identifier.
        /// </summary>
        /// <returns>The current tasks' identifier</returns>
        public string GetIdentifier()
        {
            if (index >= tasks.Count) return "";
            return tasks[index].Identifier;
        }

        /// <summary>
        /// Get the percentage of completed tasks.
        /// </summary>
        /// <returns>The percentage of completed tasks</returns>
        public float GetCompletedPercentage()
        {
            if (tasks.Count == 0) return 0f;
            return completedIndex / ((float)tasks.Count - 1);
        }

        /// <summary>
        /// Get the current task index.
        /// </summary>
        /// <returns>The current task index</returns>
        public int GetCurrentTaskindex()
        {
            return index;
        }

        /// <summary>
        /// Get the completed task index.
        /// </summary>
        /// <returns>The completed task index</returns>
        public int GetCompletedTaskIndex()
        {
            return completedIndex;
        }

        /// <summary>
        /// Get the total number of tasks.
        /// </summary>
        /// <returns>The total number of tasks</returns>
        public int GetTotalTaskCount()
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
            if (index > completedIndex) completedIndex++;
            return true;
        }

        /// <summary>
        /// Skip a tutorial task
        /// </summary>
        /// <returns> Whether a task was skipped </returns>
        public bool NextTask()
        {
            return CompleteTask(GetIdentifier());
        }

        public bool PreviousTask()
        {
            if (index == 0) return false;
            index--;
            return true;
        }
    }
}

