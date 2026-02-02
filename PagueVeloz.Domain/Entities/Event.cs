using PagueVeloz.Domain.Enums;


namespace PagueVeloz.Domain.Entities
{
    public sealed class Event : BaseEntity
    {
        public OperationType Operation {  get; set; }
        public int Attempt {  get; set; } 
        public EventStatus Status { get; set; }
        public string Description { get; set; }

        public Event(OperationType operation, int attempt, EventStatus status, string description)
        {
            Operation = operation;
            Attempt = attempt;
            Status = status; 
            Description = description;
        }
    }
}
