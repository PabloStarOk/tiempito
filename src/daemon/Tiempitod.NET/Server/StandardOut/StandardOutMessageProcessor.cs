using System.Collections.Concurrent;

namespace Tiempitod.NET.Server.StandardOut;

/// <summary>
/// Process all messages that must be sent to the standard output of the current
/// connected client allowing to queue all messages in a kind of sink.
/// </summary>
public class StandardOutMessageProcessor : IStandardOutSink, IStandardOutQueue, IDisposable
{
    private readonly TextWriter _standardOut;
    private readonly BlockingCollection<string> _messages;
    private bool _isConnected;
    private bool _isDisposed;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Instantiates a <see cref="StandardOutMessageProcessor"/>.
    /// </summary>
    /// <param name="messagesBlkCollection">A producer/consumer collection to </param>
    /// <param name="standardOut">Standard output to send the messages.</param>
    public StandardOutMessageProcessor(
        BlockingCollection<string> messagesBlkCollection,
        TextWriter standardOut)
    {
        _messages = messagesBlkCollection;
        _standardOut = standardOut;
    }
    
    public void Start(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, typeof(StandardOutMessageProcessor));
        
        _isConnected = true;
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        ThreadPool.QueueUserWorkItem(_ => WriteMessages(_cancellationTokenSource.Token));
    }   
    
    public async Task StopAsync()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, typeof(StandardOutMessageProcessor));
        
        _isConnected = false;
        if (_cancellationTokenSource != null)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
        
        DrainMessages();
    }

    public void QueueMessage(string message)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, typeof(StandardOutMessageProcessor));
        
        if (_isConnected && !_isDisposed)
            _messages.TryAdd(message);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    
    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
           
        _isDisposed = true;

        if (!disposing)
            return;
        
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _messages.Dispose();
    }

    ~StandardOutMessageProcessor()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Sends the messages that are stored in the blocking collection
    /// in sequentially way.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the task.</param>
    private void WriteMessages(CancellationToken cancellationToken)
    {
        try
        {
            foreach (string line in _messages.GetConsumingEnumerable(cancellationToken))
            {
                _standardOut.WriteLine(line);
                _standardOut.Flush();
            }
        }
        catch (OperationCanceledException) { }
        catch (IOException) { }
    }
    
    /// <summary>
    /// Drains all messages stored in the blocking collection.
    /// </summary>
    private void DrainMessages()
    {
        while (_messages.TryTake(out _, 10)) ;
    }
}
