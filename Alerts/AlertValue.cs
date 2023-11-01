using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Alerts
{
    public class AlertValue
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; } = string.Empty;
        public double Severity { get; set; } //Impact of the issue itself.
        public double Priority { get; set; } //Urgency for handling the issue.
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public AlertStatuses Status { get; set; } = AlertStatuses.None;
        
        public string SourceEntityId { get; set; } = string.Empty;
        public string SourceEntityName { get; set; } = string.Empty;
        public string SourceEntityType { get; set; } = string.Empty;

        public List<string> FaultBehaviorIds { get; set; } = new();

        public string Category { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Latch { get; set; } = false;

        public List<AlertActivity> Activities { get; set; } = new();
        public List<AlertValue> History { get; set; } = new();

        
        public bool Set(AlertValue alert)
        {
            bool hasChanged = false;
            var clone = Clone(includeActivities: true);
            if (Id == alert.Id)
            {
                
                if (Message != alert.Message)
                {
                    Activities.Add(new()
                    {
                        Activity = "Message changed.",
                        Description = alert.Message
                    });
                    Message = alert.Message;
                    hasChanged = true;
                }
                if (Severity != alert.Severity)
                {
                    Activities.Add(new()
                    {
                        Activity = "Severity changed.",
                        Description = alert.Severity.ToString("P2")
                    });
                    Severity = alert.Severity;
                    hasChanged = true;
                }
                if (Priority != alert.Priority)
                {
                    Activities.Add(new()
                    {
                        Activity = "Priority changed.",
                        Description = alert.Priority.ToString("P2")
                    });
                    Priority = alert.Priority;
                    hasChanged = true;
                }
                if (Status != alert.Status)
                {
                    Activities.Add(new()
                    {
                        Activity = "Status changed.",
                        Description = alert.Status.ToString()
                    });
                    Status = alert.Status;
                    hasChanged = true;
                }
                if (SourceEntityId != alert.SourceEntityId)
                {
                    
                    SourceEntityId = alert.SourceEntityId;
                    SourceEntityName = alert.SourceEntityName;
                    SourceEntityType = alert.SourceEntityType;
                    hasChanged = true;
                }
                if (Category != alert.Category)
                {
                   
                    Category = alert.Category;
                    hasChanged = true;
                }
                if (AssignedTo != alert.AssignedTo)
                {
                    Activities.Add(new()
                    {
                        Activity = "AssignedTo changed.",
                        Description = alert.AssignedTo
                    });
                    AssignedTo =alert.AssignedTo;
                    hasChanged = true;

                }
                if (Type != alert.Type)
                {
                   
                    Type = alert.Type;
                    hasChanged = true;
                }
                if (!FaultBehaviorIds.Equals(alert.FaultBehaviorIds))
                {
                    FaultBehaviorIds = alert.FaultBehaviorIds;
                    hasChanged = true;
                }
                Activities.AddRange(alert.Activities);
            } else
            { //new
                History.Add(clone);
                Id = alert.Id;
                Message = alert.Message;
                Severity = alert.Severity;
                Priority = alert.Priority;
                Timestamp = alert.Timestamp;
                Status = alert.Status;
                SourceEntityId = alert.SourceEntityId;
                Category = alert.Category;
                AssignedTo = alert.AssignedTo;
                Type = alert.Type;
                Activities = Helpers.ObjectUtils.Clone(alert.Activities)??new();
                Activities.Add(new() { Activity = "New", Description = $"Id: {Id} Status: {Status.ToString()}" });
                hasChanged = true;
            }
            return hasChanged;
        }

        public AlertValue Clone(bool includeActivities = false, bool includeHistory = false)
        {
            var clone = new AlertValue();
            clone.Id = Id;
            clone.Message = Message;
            clone.Severity = Severity;
            clone.Priority = Priority;
            clone.Timestamp = Timestamp;
            clone.Status = Status;
            clone.SourceEntityId = SourceEntityId;
            clone.Category = Category;
            clone.AssignedTo = AssignedTo;
            clone.Type = Type;
            if (includeActivities) { clone.Activities = Helpers.ObjectUtils.Clone(Activities)??new(); }
            if (includeHistory) { clone.History = Helpers.ObjectUtils.Clone(History) ?? new(); }
            return clone;
        }

        
    }
}
