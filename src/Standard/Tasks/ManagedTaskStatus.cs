namespace Standard.Tasks;

public enum ManagedTaskStatus
{
    Pending,
    Waiting,
    Running,
    Cancelling,
    Cancelled,
    Completed,
    Faulted
}