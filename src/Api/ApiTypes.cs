namespace Dodgeball.TrustServer.Api;

public class DodgeballError
{
    public string? category { get; set; }

    public string message { get; set; }

    public DodgeballError(string? category, string message)
    {
        this.category = category;
        this.message = message;
    }
}

public class DodgeballResponse
{
    public bool success;
    public DodgeballError[] errors;
}

public class DodgeballEvent
{
    public DodgeballEvent(string type, string ip, dynamic data, DateTime? eventTime = null)
    {
        this.type = type;
        this.ip = ip;
        this.data = data;
        this.eventTime = eventTime ?? DateTime.Now;
    }

    public string type
    {
        get;
        set;
    }

    public string ip
    {
        get;
        set;
    }

    public dynamic data
    {
        get;
        set;
    }

    public DateTime eventTime
    {
        get;
        set;
    }
}

public class CheckpointResponseOptions
{
    public CheckpointResponseOptions(
        bool sync,
        Int32? timeout = null,
        string? webhook = null)
    {
        this.Sync = sync;
        this.Timeout = timeout;
        this.Webhook = webhook;
    }
    
    public bool? Sync
    {
        get;
        set;
    }
    
    public Int32? Timeout
    {
        get;
        set;
    }

    public string? Webhook
    {
        get;
        set;
    }
}

public class CheckpointRequest
{
    public CheckpointRequest(
        DodgeballEvent dodgeballEvent,
        string? sourceToken,
        string? sessionId,
        string? userId = null,
        string? useVerificationId = null, 
        CheckpointResponseOptions? checkpointResponseOptions = null)
    {
        this.Event = dodgeballEvent;
        this.SourceToken = sourceToken;
        this.SessionId = sessionId;
        this.UserId = userId;
        this.ResponseOptions = checkpointResponseOptions;
    }

    public DodgeballEvent Event { get; set; }

    public  string SourceToken { get; set; }

    public string SessionId
    {
        get;
        set;
    }

    public string? UserId
    {
        get;
        set;
    }

    public string? PriorVerificationId
    {
        get;
        set;
    }


    public CheckpointResponseOptions? ResponseOptions
    {
        get;
        set;
    }
}

public static class VerificationStatus
{
    // In Process on the server
    public const string PENDING = "PENDING";

    // Waiting on some action, for example MFA
    public const string BLOCKED = "BLOCKED";

    // Workflow evaluated successfully
    public const string COMPLETE = "COMPLETE";

    // Workflow execution failure
    public const string FAILED = "FAILED";
}

public static class VerificationOutcome
{
    public const string APPROVED = "APPROVED";
    public const string DENIED = "DENIED";
    public const string PENDING = "PENDING";
    public const string ERROR = "ERROR";
}

public class DodgeballVerification
{
    public string id;
    public string status;
    public string outcome;
}

public class DodgeballCheckpointResponse : DodgeballResponse{
    public bool success;
    public DodgeballError[]? errors;
    public string? version;
    public DodgeballVerification? verification;
    public bool? isTimeout;
}