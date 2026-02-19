using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.UnitTests.Domain;

/// <summary>
/// Unit tests for core domain entities.
/// </summary>
public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_ShouldHaveNewGuidId_OnCreation()
    {
        // Arrange & Act
        var entity = new Tenant { Code = "TEST", Name = "Test Tenant" };

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void BaseEntity_ShouldBeActiveByDefault()
    {
        var entity = new Tenant { Code = "TEST", Name = "Test Tenant" };
        Assert.True(entity.IsActive);
    }

    [Fact]
    public void BaseEntity_CreatedAt_ShouldBeSetToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var entity = new Tenant { Code = "TEST", Name = "Test Tenant" };
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(entity.CreatedAt, before, after);
    }
}

public class ApplicationUserTests
{
    [Fact]
    public void ApplicationUser_DisplayName_ShouldCombineFirstAndLastName()
    {
        var user = new ApplicationUser
        {
            FirstName = "Jane",
            LastName  = "Doe",
            Email     = "jane.doe@test.com",
            IdentityUserId = Guid.NewGuid().ToString()
        };

        Assert.Equal("Jane Doe", user.DisplayName);
    }

    [Fact]
    public void ApplicationUser_DisplayName_ShouldHandleEmptyLastName()
    {
        var user = new ApplicationUser
        {
            FirstName = "Jane",
            LastName  = "",
            Email     = "jane@test.com",
            IdentityUserId = Guid.NewGuid().ToString()
        };

        Assert.Equal("Jane", user.DisplayName);
    }
}

public class WorkflowDefinitionTests
{
    [Fact]
    public void WorkflowDefinition_ShouldDefaultVersion_ToOne()
    {
        var def = new WorkflowDefinition
        {
            Code     = "INCIDENT_APPROVAL",
            Name     = "Incident Approval Workflow",
            Category = "ITSM"
        };

        Assert.Equal(1, def.Version);
    }

    [Fact]
    public void WorkflowDefinition_ShouldNotBePublished_ByDefault()
    {
        var def = new WorkflowDefinition
        {
            Code     = "INCIDENT_APPROVAL",
            Name     = "Incident Approval Workflow",
            Category = "ITSM"
        };

        Assert.False(def.IsPublished);
    }
}

public class AuditLogTests
{
    [Fact]
    public void AuditLog_Action_ShouldMapCorrectly()
    {
        var log = new AuditLog
        {
            Module     = "ITSM",
            Action     = AuditAction.Create,
            EntityType = "WorkflowDefinition",
            EntityId   = Guid.NewGuid().ToString()
        };

        Assert.Equal(AuditAction.Create, log.Action);
        Assert.Equal(1, (int)log.Action);
    }
}

public class PagedResultTests
{
    [Fact]
    public void PagedResult_TotalPages_CalculatesCorrectly()
    {
        var result = new PagedResult<string>
        {
            Items      = ["a", "b", "c"],
            TotalCount = 25,
            PageNumber = 1,
            PageSize   = 10
        };

        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void PagedResult_LastPage_ShouldNotHaveNextPage()
    {
        var result = new PagedResult<string>
        {
            Items      = ["a"],
            TotalCount = 11,
            PageNumber = 2,
            PageSize   = 10
        };

        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }
}
