using System;
using UnityEngine;

namespace _Project.UI.Scripts.Tutorial
{

    /// <summary>
    /// Simple class that describes a tutorial task.
    /// </summary>
    [Serializable]
    public class TutorialTask
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
        /// Descriptiop of this task that has to be completed by the user.
        /// </summary>
        [SerializeField]
        private string task;
        public string Task
        {
            get { return task; }
            set { task = value; }
        }

    }
}
