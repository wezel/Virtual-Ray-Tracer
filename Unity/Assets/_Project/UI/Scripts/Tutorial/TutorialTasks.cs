using System.Collections.Generic;
using System;
using UnityEngine;

namespace _Project.UI.Scripts.Tutorial
{
    /// <summary>
    /// Class that manages several required and optional tasks.
    /// Includes some useful methods to manipulate the tasks.
    /// </summary>
    [Serializable]
    public class TutorialTasks
    {
        [SerializeField]
        private List<TutorialTask> requiredTasks;
        private List<TutorialTask> completedRequiredTasks;

        [SerializeField]
        private List<TutorialTask> optionalTasks;
        private List<TutorialTask> completedOptionalTasks;

        private int requiredIndex;
        private int optionalIndex;

    }
}

