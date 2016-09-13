using System;
using System.Collections.Generic;
using EnvDTE;
using VSLangProj;

namespace MSTest2XUnit
{
    internal static class ProjectExtensions
    {
        public static IEnumerable<string> CollectReferenceNames(this Project project)
        {
            var vsproject = project.Object as VSProject;

            if (vsproject != null)
            {
                foreach (Reference reference in vsproject.References)
                {
                    if (reference.SourceProject == null)
                    {
                        yield return reference.Identity;
                    }
                }
            }
        }

        public static bool RemoveReferenceByName(this Project project, string referenceName)
        {
            var vsproject = project.Object as VSProject;
            if (vsproject == null)
            {
                return false;
            }
            var reference = vsproject.References.Find(referenceName);
            try
            {
                reference?.Remove();
                return true;
            }
            catch (Exception)
            {
                return false;
            }


        }
    }
}