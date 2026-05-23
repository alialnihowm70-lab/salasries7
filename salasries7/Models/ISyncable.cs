using System;

namespace salasries7.Models;

public interface ISyncable
{
    Guid SyncId { get; set; }
    DateTime UpdatedAt { get; set; }
    bool IsSynced { get; set; }
}
