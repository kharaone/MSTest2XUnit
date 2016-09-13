﻿using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;

public static class SolutionProjects
{
    public static IEnumerable<Project> GetAllProjects(this Solution sln)
    {
        return sln.Projects
            .Cast<Project>()
            .SelectMany(GetProjects);
    }

    private static IEnumerable<Project> GetProjects(Project project)
    {
        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
        {
            return project.ProjectItems
                .Cast<ProjectItem>()
                .Select(x => x.SubProject)
                .Where(x => x != null)
                .SelectMany(GetProjects);
        }
        return new[] { project };
    }
}