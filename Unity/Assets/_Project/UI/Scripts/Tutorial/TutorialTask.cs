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
}
