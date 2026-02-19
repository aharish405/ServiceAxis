namespace ServiceAxis.Domain.Enums;

/// <summary>Workflow instance execution states (state-machine based).</summary>
public enum WorkflowStatus
{
    /// <summary>The workflow has been created but not yet started.</summary>
    Draft = 0,

    /// <summary>The workflow instance is actively running.</summary>
    Active = 1,

    /// <summary>Waiting for a human approval or external event.</summary>
    Pending = 2,

    /// <summary>The workflow completed all steps successfully.</summary>
    Completed = 3,

    /// <summary>The workflow was cancelled before completion.</summary>
    Cancelled = 4,

    /// <summary>The workflow encountered an unrecoverable error.</summary>
    Failed = 5
}

/// <summary>Step execution outcome.</summary>
public enum StepStatus
{
    /// <summary>The step is waiting to be processed.</summary>
    Pending = 0,

    /// <summary>The step is currently being processed.</summary>
    InProgress = 1,

    /// <summary>The step completed successfully.</summary>
    Completed = 2,

    /// <summary>The step was skipped due to a conditional branch.</summary>
    Skipped = 3,

    /// <summary>The step failed.</summary>
    Failed = 4
}
