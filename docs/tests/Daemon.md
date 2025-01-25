# Tests for daemon

## Session Storage

``` csharp
bool AddSession();
void UpdateSession();
Session RemoveSession();
```

- `AddSession` must add a session to the right dictionary and change its state.
- `AddSession` mustn't add a session nor change its state if there's already a session with same ID in the dictionary.
- `UpdateSession` must update a session if it exist in the dictionary.
- `UpdateSession` mustn't update nor add a session if it doesn't exist in the dictionary.
- `RemoveSession` must remove a session from a dictionary.

## Session Timer

``` csharp
IProgress<Session> _timeProgress;

event OnTimeCompleted;
event OnSessionStarted;
event OnSessionCompleted;
event OnDelayElapsed;

void Start();
Session Stop();
Session[] StopAll();
```

- [X] `Start` must start a session when session storage returns true.
- [X] `Start` mustn't start a session when session storage returns false.
- Session is stopped when cancellation token source cancels the token.
- [X] `Stop` must stop and returns the session.
- [X] `StopAll` must stop and return all sessions.

- [X] `IProgress<Session>.Report()` is called while session is running.
- [X] `OnTimeCompleted` is raised when a time is completed.
- [X] `OnSessionCompleted` is raised when a session is completed.
- [ ] `OnSessionCompleted` must not be raised if TargetCycles property is 0.
- [X] `OnDelayElapsed` is raised when a second is elapsed in a delay between times.
- [ ] `OnDelayElapsed` must not be raised if DelayBetweenTimes property is 0.
