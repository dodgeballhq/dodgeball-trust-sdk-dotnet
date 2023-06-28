using System.Diagnostics;

namespace Dodgeball.TrustServer.Api
{

  using System;
  using System.Collections.Generic;
  using System.Data;
  using System.Threading;
  using System.Threading.Tasks;

  public class Dodgeball
  {
    public Dodgeball(string secretKey, DodgeballConfig? config = null)
    {
      if (String.IsNullOrEmpty(secretKey))
      {
        throw new ArgumentNullException(
          nameof(secretKey),
          "Must provide a non-null and non-empty Dodgeball Secret API Key");
      }

      this.secretKey = secretKey;
      this.config = config;
    }

    public async Task<DodgeballResponse> PostEvent(
      string? sourceToken,
      string sessionId,
      string? userId,
      DodgeballEvent dodgeballEvent)
    {

      try
      {
        string? baseUrl = this.config?.ApiUrl ?? BASE_URL;

        var headers = new Dictionary<string, string?>();
        headers["dodgeball-session-id"] = sessionId;
        headers["dodgeball-secret-key"] = this.secretKey;

        if (!String.IsNullOrEmpty(sourceToken))
        {
          headers["dodgeball-source-token"] = sourceToken;
        }

        if (!String.IsNullOrEmpty(userId))
        {
          headers["dodgeball-customer-id"] = userId;
        }

        var httpQuery = new HttpQuery(baseUrl, "/v1/track/"
        ).SetHeaders(headers).SetBody(dodgeballEvent);

        var response = await httpQuery.PostDodgeball();
        return response;
      }
      catch (Exception exc)
      {
        return QueryUtils.CreateErrorResponse(exc);
      }
    }

    public const int BASE_CHECKPOINT_TIMEOUT_MS = 100;
    public const int MAX_TIMEOUT = 10000;
    public const int MAX_RETRY_COUNT = 3;

    public async Task<DodgeballCheckpointResponse> Checkpoint(
      DodgeballEvent dodgeballEvent,
      string? sourceToken,
      string? sessionId,
      string? userId = null,
      string? useVerificationId = null,
      CheckpointResponseOptions? checkpointResponseOptions = null)
    {
      if (String.IsNullOrEmpty(dodgeballEvent.type))
      {
        throw new ArgumentNullException("dodgeballEvent.type");
      }

      if (String.IsNullOrEmpty(dodgeballEvent.ip))
      {
        throw new ArgumentNullException("dodgeballEvent.ip");
      }

      if (String.IsNullOrEmpty(sessionId))
      {
        throw new ArgumentException("sessionId");
      }

      if (dodgeballEvent.eventTime.HasValue)
      {
        // This must be set on the server side
        dodgeballEvent.eventTime = null;
      }

      try
      {
        checkpointResponseOptions = checkpointResponseOptions ?? new CheckpointResponseOptions
        {
          sync = true,
          timeout = -1
        };

        int timeout = checkpointResponseOptions.timeout ?? -1;
        var trivialTimeout = timeout <= 0;
        var largeTimeout = timeout > 5 * BASE_CHECKPOINT_TIMEOUT_MS;

        var mustPoll = trivialTimeout || largeTimeout;
        var activeTimeout = mustPoll
          ? BASE_CHECKPOINT_TIMEOUT_MS
          : checkpointResponseOptions?.timeout ?? BASE_CHECKPOINT_TIMEOUT_MS;

        var maximalTimeout = MAX_TIMEOUT;
        var syncResponse = !checkpointResponseOptions.sync.HasValue
          ? true
          : checkpointResponseOptions.sync.Value;

        if (activeTimeout > 0)
        {
          syncResponse = false;
        }

        CheckpointResponseOptions internalOptions = new CheckpointResponseOptions
        {
          sync = syncResponse,
          timeout = activeTimeout,
          webhook = checkpointResponseOptions.webhook
        };

        DodgeballCheckpointResponse? response = null;
        var numRepeats = 0;
        var numFailures = 0;

        bool isDisabled = this.config != null &&
                          this.config.isEnabled.HasValue &&
                          !this.config.isEnabled.Value;

        if (isDisabled)
        {
          return new DodgeballCheckpointResponse
          {
            success = true,
            errors = new DodgeballError[] { },
            version = DodgeballApiVersion.V1,
            verification = new DodgeballVerification
            {
              id = "DODGEBALL_IS_DISABLED",
              status = VerificationStatus.COMPLETE,
              outcome = VerificationOutcome.APPROVED
            },
          };
        }

        var headers = new Dictionary<string, string?>();

        if (sourceToken != null)
        {
          headers["dodgeball-source-token"] = sourceToken;
        }

        headers["dodgeball-secret-key"] = this.secretKey;
        var baseUrl = this.config?.ApiUrl ?? BASE_URL;

        if (sessionId != null)
        {
          headers["dodgeball-session-id"] = sessionId;
        }

        if (useVerificationId != null)
        {
          headers["dodgeball-verification-id"] = useVerificationId;
        }

        if (userId != null)
        {
          headers["dodgeball-customer-id"] = userId;
        }

        while (response == null && numRepeats < 3)
        {
          Dictionary<string, dynamic> body = new Dictionary<string, dynamic>();
          body["event"] = dodgeballEvent;
          body["options"] = internalOptions;

          var httpQuery = new HttpQuery(
            baseUrl,
            "/v1/checkpoint").SetHeaders(
            headers).SetBody(body);

          response = await httpQuery.PostCheckpoint();
          numRepeats += 1;
        }

        if (response == null)
        {
          return new DodgeballCheckpointResponse
          {
            success = false,
            errors = new DodgeballError[]
            {
              new DodgeballError("UNKNOWN", "Unknown evaluation error")
            }
          };
        }
        else if (!response.success)
        {
          return response;
        }

        var status = response.verification?.status ?? "";
        var outcome = response.verification?.outcome ?? "";
        var isResolved = status != VerificationStatus.PENDING;
        var verificationId = response.verification?.id ?? "";

        headers["dodgeball-verification-id"] = verificationId;
        while (
          (trivialTimeout ||
           (checkpointResponseOptions?.timeout ?? BASE_CHECKPOINT_TIMEOUT_MS) >
           numRepeats * activeTimeout) &&
          !isResolved &&
          numFailures < MAX_RETRY_COUNT
        )
        {
          await Task.Delay(activeTimeout);
          activeTimeout =
            activeTimeout < maximalTimeout ? 2 * activeTimeout : activeTimeout;

          var httpQuery = new HttpQuery(
            baseUrl,
            String.Format("/v1/verification/{0}", verificationId)).SetHeaders(
            headers);

          response = await httpQuery.GetVerification();
          numRepeats += 1;

          if (response != null && response.success)
          {
            status = response.verification?.status ?? "";
            if (String.IsNullOrEmpty(status))
            {
              numFailures += 1;
            }
            else
            {
              isResolved = status != VerificationStatus.PENDING;
              numRepeats += 1;
            }
          }
          else
          {
            numFailures += 1;
          }
        }

        if (numFailures >= MAX_RETRY_COUNT)
        {
          var timeoutResponse = new DodgeballCheckpointResponse
          {
            success = false,
            version = DodgeballApiVersion.V1,
            errors = new DodgeballError[]
            {
              new DodgeballError(
                "UNAVAILABLE",
                "Service Unavailable: Maximum retry count exceeded")
            },
            isTimeout = true,
          };

          return timeoutResponse;
        }

        return response;
      }
      catch (Exception exc)
      {
        var internalData = QueryUtils.CreateErrorResponse(exc);
        return new DodgeballCheckpointResponse
        {
          success = false,
          errors = internalData.errors,
        };
      }
    }

    #region State Query Functions

    public bool IsRunning(DodgeballCheckpointResponse checkpointResponse)
    {
      if (checkpointResponse.success)
      {
        switch (checkpointResponse.verification?.status ?? "")
        {
          case VerificationStatus.PENDING:
          case VerificationStatus.BLOCKED:
            return true;
          default:
            return false;
        }
      }

      return false;
    }

    public bool IsAllowed(DodgeballCheckpointResponse checkpointResponse)
    {
      var status = checkpointResponse.verification?.status ?? "";
      var outcome = checkpointResponse.verification?.outcome ?? "";

      return checkpointResponse.success &&
             status == VerificationStatus.COMPLETE &&
             outcome == VerificationOutcome.APPROVED;
    }

    public bool IsDenied(DodgeballCheckpointResponse checkpointResponse)
    {
      var outcome = checkpointResponse.verification?.outcome ?? "";
      return checkpointResponse.success &&
             outcome == VerificationOutcome.DENIED;
    }

    public bool IsUndecided(
      DodgeballCheckpointResponse checkpointResponse)
    {
      var status = checkpointResponse.verification?.status ?? "";
      var outcome = checkpointResponse.verification?.outcome ?? "";
      return checkpointResponse.success &&
             status == VerificationStatus.COMPLETE &&
             outcome == VerificationOutcome.PENDING;
    }

    public bool HasError(DodgeballCheckpointResponse checkpointResponse)
    {
      var status = checkpointResponse.verification?.status ?? "";
      var outcome = checkpointResponse.verification?.outcome ?? "";

      return !checkpointResponse.success ||
             (status == VerificationStatus.FAILED &&
              outcome == VerificationOutcome.ERROR);
    }

    public bool IsTimeout(DodgeballCheckpointResponse checkpointResponse)
    {
      var status = checkpointResponse.verification?.status ?? "";
      var outcome = checkpointResponse.verification?.outcome ?? "";
      return !checkpointResponse.success && checkpointResponse.isTimeout != null &&
             checkpointResponse.isTimeout.Value;

    }

    #endregion

    private string secretKey;
    private DodgeballConfig? config;
    private const string BASE_URL = "https://api.dodgeballhq.com/";

  }
}
