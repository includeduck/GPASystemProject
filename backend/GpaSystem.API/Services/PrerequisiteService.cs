using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;

namespace GpaSystem.API.Services;

public class PrerequisiteService : IPrerequisiteService
{
    private readonly ICourseRepository _courses;
    private readonly ICoursePrerequisiteRepository _prerequisites;

    public PrerequisiteService(ICourseRepository courses, ICoursePrerequisiteRepository prerequisites)
    {
        _courses = courses;
        _prerequisites = prerequisites;
    }

    public async Task<List<PrerequisiteResponse>> GetForCourseAsync(int courseId)
    {
        await FindCourseAsync(courseId, notFoundMessage: "Course was not found.");
        var prerequisites = await _prerequisites.GetForCourseAsync(courseId);
        return prerequisites.Select(Map).ToList();
    }

    public async Task<PrerequisiteResponse> AddAsync(int courseId, AddPrerequisiteRequest request)
    {
        if (courseId == request.PrerequisiteCourseId)
        {
            throw ApiException.BadRequest("A course cannot be its own prerequisite.");
        }

        var course = await FindCourseAsync(courseId, notFoundMessage: "Course was not found.");
        var prerequisiteCourse = await FindCourseAsync(
            request.PrerequisiteCourseId,
            notFoundMessage: "Prerequisite course was not found.");

        if (await _prerequisites.ExistsAsync(course.CourseId, prerequisiteCourse.CourseId))
        {
            throw ApiException.Conflict("This prerequisite is already defined for the course.");
        }

        if (await WouldCreateCycleAsync(course.CourseId, prerequisiteCourse.CourseId))
        {
            throw ApiException.Conflict("Adding this prerequisite would create a circular dependency.");
        }

        var prerequisite = new CoursePrerequisite
        {
            CourseId = course.CourseId,
            Course = course,
            PrerequisiteCourseId = prerequisiteCourse.CourseId,
            PrerequisiteCourse = prerequisiteCourse
        };

        await _prerequisites.AddAsync(prerequisite);
        await _prerequisites.SaveChangesAsync();
        return Map(prerequisite);
    }

    public async Task RemoveAsync(int courseId, int prerequisiteCourseId)
    {
        var prerequisite = await _prerequisites.GetAsync(courseId, prerequisiteCourseId)
            ?? throw ApiException.NotFound("Prerequisite relationship was not found.");

        _prerequisites.Remove(prerequisite);
        await _prerequisites.SaveChangesAsync();
    }

    private async Task<Course> FindCourseAsync(int id, string notFoundMessage)
    {
        return await _courses.GetByIdAsync(id)
            ?? throw ApiException.NotFound(notFoundMessage);
    }

    private async Task<bool> WouldCreateCycleAsync(int courseId, int prerequisiteCourseId)
    {
        var visited = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(prerequisiteCourseId);

        while (stack.Count > 0)
        {
            var currentCourseId = stack.Pop();
            if (currentCourseId == courseId)
            {
                return true;
            }

            if (!visited.Add(currentCourseId))
            {
                continue;
            }

            var nextPrerequisites = await _prerequisites.GetPrerequisiteIdsAsync(currentCourseId);
            foreach (var nextCourseId in nextPrerequisites)
            {
                stack.Push(nextCourseId);
            }
        }

        return false;
    }

    private static PrerequisiteResponse Map(CoursePrerequisite prerequisite)
    {
        return new PrerequisiteResponse
        {
            CourseId = prerequisite.CourseId,
            CourseCode = prerequisite.Course.CourseCode,
            CourseTitle = prerequisite.Course.CourseTitle,
            PrerequisiteCourseId = prerequisite.PrerequisiteCourseId,
            PrerequisiteCourseCode = prerequisite.PrerequisiteCourse.CourseCode,
            PrerequisiteCourseTitle = prerequisite.PrerequisiteCourse.CourseTitle
        };
    }
}
