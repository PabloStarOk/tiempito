# Tests for daemon

## Sessions

### Session Storage

``` csharp
bool AddSession();
void UpdateSession();
Session RemoveSession();
```

- [X] `AddSession` must add a session to the right dictionary and change its state.
- [X] `AddSession` mustn't add a session nor change its state if there's already a session with same ID in the dictionary.
- [X] `UpdateSession` must update a session if it exist in the dictionary.
- [X] `UpdateSession` mustn't update nor add a session if it doesn't exist in the dictionary.
- [X] `RemoveSession` must remove a session from a dictionary.

### Session Timer

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
- [X] Session is stopped when cancellation token source cancels the token.
- [X] `Stop` must stop and returns the session.
- [X] `StopAll` must stop and return all sessions.

- [X] `IProgress<Session>.Report()` is called while session is running.
- [X] `OnTimeCompleted` is raised when a time is completed.
- [X] `OnSessionCompleted` is raised when a session is completed.
- [X] `OnSessionCompleted` must not be raised if TargetCycles property is 0.
- [X] `OnDelayElapsed` is raised when a second is elapsed in a delay between times.
- [X] `OnDelayElapsed` must not be raised if DelayBetweenTimes property is 0.

### SessionManager

#### StartService

``` csharp
void StartService();
```

- [X] Should subscribe to the events.

#### StopService

``` csharp
void StopService();
```

- [X] Should unsubscribe from the events and stop all timers.

#### StartSession

``` csharp
OperationResult StartSession();
```

All success operations must check the session was started.

- [X] Should start a session with the ID and config specified.
- [X] Should start a session with the the default config when config ID is not specified.
- [X] Should start a session with the ID of the config when session ID is not specified.
- [X] Should start a session with the ID of the config when nor session ID and config ID are specified.
- [X] Should return a failed operation result when config ID was not found.
- [X] Should return a failed operation result when session with the same ID already exists.

#### PauseSession

``` csharp
OperationResult PauseSession();
```

All success operations must check the session was paused.

- [X] Should return a success operation result when session is paused.
- [X] Should return a success operation result when session ID is not provided but exists a session to pause.
- [X] Should return a failed operation result when there are no sessions to pause.
- [X] Should return a failed operation result when session ID is not found.

#### ResumeSession

``` csharp
OperationResult ResumeSession();
```

All success operations must check the session was resumed.

- [X] Should resume a paused session when ID is specified.
- [X] Should resume a paused session when ID is not provided but exists a session that can be resumed.
- [X] Should return a failed operation result when session ID is not found.
- [X] Should return a failed operation result when there are no sessions.

#### CancelSession

``` csharp
OperationResult CancelSession();
```

All success operations must check the session was cancelled.

- [X] Should cancel a running session when ID is specified.
- [X] Should cancel a paused session when ID is specified.
- [X] Should cancel a running session when ID is not provided but exists a session that can be cancelled.
- [X] Should cancel a paused session when ID is not provided but exists a session that can be cancelled.
- [X] Should return a failed operation result when session ID is not found.
- [X] Should return a failed operation result when there are no active sessions.

#### Event Handlers

- [ ] `SessionManager` should report and notify when a session is started.
- [ ] `SessionManager` should report and notify when a session is completed.
- [ ] `SessionManager` should report progress of the running session.
- [ ] `SessionManager` should report and notify when a time is completed.
- [ ] `SessionManager` should report when a second of delay is elapsed.
