﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PegasusApi.Library.DataAccess;
using PegasusApi.Library.Models;

namespace PegasusApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectsData _projectsData;

        public ProjectController(IProjectsData projectsData)
        {
            _projectsData = projectsData;
        }

        [Route("GetAllProjects")]
        [HttpGet]
        public List<ProjectModel> GetAllProjects()
        {
            return _projectsData.GetProjects();
        }

        [Route("GetProject/{id}")]
        [HttpGet]
        public ProjectModel GetProject(int id)
        {
            return _projectsData.GetProject(id);
        }

        [Route("AddProject")]
        [HttpPost]
        public void AddProject(ProjectModel project)
        {
            _projectsData.AddProject(project);
        }

        [Route("UpdateProject")]
        [HttpPost]
        public void UpdateProject(ProjectModel project)
        {
            _projectsData.UpdateProject(project);
        }

        [Route("DeleteProject/{id}")]
        [HttpDelete]
        public void DeleteProject(int id)
        {
            _projectsData.DeleteProject(id);
        }
    }
}