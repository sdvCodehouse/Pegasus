﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasus.Domain.ProjectTask;
using Pegasus.Extensions;
using Pegasus.Library.Api;
using Pegasus.Library.Models;
using Pegasus.Models;
using Pegasus.Models.Settings;
using Pegasus.Models.TaskList;

namespace Pegasus.Controllers
{
    [Authorize(Roles = "PegasusUser")]
    public class TaskListController : Controller
    {
        private readonly ITaskFilterService _taskFilterService;
        private readonly IProjectsEndpoint _projectsEndpoint;
        private readonly ITasksEndpoint _tasksEndpoint;
        private readonly ICommentsEndpoint _commentsEndpoint;
        private readonly ISettingsModel _settingsModel;
        private readonly int _pageSize;

        public TaskListController(ITaskFilterService taskFilterService, 
            IProjectsEndpoint projectsEndpoint, ITasksEndpoint tasksEndpoint, 
            ICommentsEndpoint commentsEndpoint, ISettingsModel settingsModel)
        {
            _taskFilterService = taskFilterService;
            _projectsEndpoint = projectsEndpoint;
            _tasksEndpoint = tasksEndpoint;
            _commentsEndpoint = commentsEndpoint;
            _settingsModel = settingsModel;
            _pageSize = settingsModel.PageSize;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var taskFilterId = _settingsModel.GetSetting<int>(nameof(_settingsModel.TaskFilterId));
            var projectId = _settingsModel.GetSetting<int>(nameof(_settingsModel.ProjectId));
            var page = GetPage();

            var project = await _projectsEndpoint.GetProject(projectId) ?? new ProjectModel { Id = 0, Name = "All" };
            var projectTasks = projectId > 0 ? await _tasksEndpoint.GetTasks(projectId) : await _tasksEndpoint.GetAllTasks();

            var model = new IndexViewModel(projectTasks, taskFilterId, _settingsModel)
            {
                ProjectId = projectId,
                Page = page,
                PageSize = _pageSize,
                Projects = await _projectsEndpoint.GetAllProjects(),
                TaskFilters = _taskFilterService.GetTaskFilters(),
                Project = project
            };

            if (Request != null && Request.IsAjaxRequest())
            {
                return PartialView("../TaskList/_ProjectTaskList", model);
            }

            return View("../TaskList/Index", model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var projectId = _settingsModel.GetSetting<int>(nameof(_settingsModel.ProjectId));
            var project = await _projectsEndpoint.GetProject(projectId);
            var taskModel = new TaskModel
            {
                ProjectId = projectId,
                TaskRef = $"{project.ProjectPrefix}-<tbc>"
            };
            var model = await TaskViewModel.Create(new TaskViewModelArgs {
                ProjectsEndpoint = _projectsEndpoint,
                TasksEndpoint = _tasksEndpoint,
                CommentsEndpoint = _commentsEndpoint,
                ProjectTask = taskModel, 
                Project = project});

            model.ProjectTask = taskModel;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Name,ProjectId,TaskRef,TaskStatusId,TaskTypeId,TaskPriorityId,FixedInRelease")] TaskModel projectTask)
        {
            projectTask.Created = projectTask.Modified = DateTime.Now;
            if (ModelState.IsValid)
            {
                projectTask.UserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _tasksEndpoint.AddTask(projectTask);
                return RedirectToAction("Index");
            }
            var model = await TaskViewModel.Create(new TaskViewModelArgs { 
                ProjectsEndpoint = _projectsEndpoint, 
                TasksEndpoint = _tasksEndpoint, 
                CommentsEndpoint = _commentsEndpoint,
                ProjectTask = projectTask });

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var projectTask = await _tasksEndpoint.GetTask(id);
            _settingsModel.ProjectId = projectTask.ProjectId;
            _settingsModel.SaveSettings();

            var model = await TaskViewModel.Create(new TaskViewModelArgs {
                ProjectsEndpoint = _projectsEndpoint, 
                TasksEndpoint = _tasksEndpoint, 
                CommentsEndpoint = _commentsEndpoint,
                ProjectTask = projectTask});

            if (Request != null && Request.IsAjaxRequest())
            {
                return PartialView("_EditTaskContent", model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Description,Name,Created,ProjectId,TaskRef,TaskStatusId,TaskTypeId,TaskPriorityId,FixedInRelease")] TaskModel projectTask,
            int existingTaskStatus, string newComment, [Bind("Id,Comment")] IEnumerable<TaskCommentModel> comments)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                projectTask.UserId = userId;
                await _tasksEndpoint.UpdateTask(projectTask);
                await _commentsEndpoint.UpdateComments(comments);
                if (!string.IsNullOrWhiteSpace(newComment))
                {
                    await _commentsEndpoint.AddComment(new TaskCommentModel {TaskId = projectTask.Id, Comment = newComment, UserId = userId });
                }

                if (projectTask.IsClosed() && projectTask.TaskStatusId != existingTaskStatus)
                {
                    return RedirectToAction("Index");
                }

                return RedirectToAction("Edit", projectTask.Id);
            }

            var taskViewModelArgs = new TaskViewModelArgs {
                ProjectsEndpoint = _projectsEndpoint,
                TasksEndpoint = _tasksEndpoint,
                CommentsEndpoint = _commentsEndpoint,
                ProjectTask = projectTask,
                ExistingStatusId = existingTaskStatus,
                Comments = comments,
                NewComment = newComment};
            var model = await TaskViewModel.Create(taskViewModelArgs);

            return View(model);
        }

        public IActionResult Error()
        {
            var model = new BaseViewModel { ProjectId = 0 };
            return View(model);
        }

        private int GetPage()
        {
            const int defaultPageNo = 1;
            var qsPage = Request.Query["page"];
            return int.TryParse(qsPage, out var pageNo) ? pageNo : defaultPageNo;
        }
    }
}
