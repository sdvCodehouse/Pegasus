﻿using System.Collections.Generic;
using Pegasus.Entities;
using TaskStatus = Pegasus.Entities.TaskStatus;

namespace Pegasus.Services
{
    public interface IPegasusData
    {
        IEnumerable<Project> GetAllProjects();
        Project GetProject(int id);
        void AddProject(Project project);
        void UpdateProject(Project project);

        IEnumerable<ProjectTask> GetAllTasks();
        ProjectTask GetTask(int id);
        IEnumerable<ProjectTask> GetTasks(int projectId);
        void AddTask(ProjectTask projectTask);
        void UpdateTask(ProjectTask projectTask, int existingTaskStatus);

        TaskComment GetComment(int id);
        IEnumerable<TaskComment> GetComments(int taskId);
        void AddComment(TaskComment taskComment);
        void UpdateComment(TaskComment taskComment);

        IEnumerable<TaskStatus> GetAllTaskStatuses();
        void AddTaskStatus(TaskStatus taskStatus);

        IEnumerable<TaskType> GetAllTaskTypes();
        void AddTaskType(TaskType taskType);


    }
}
