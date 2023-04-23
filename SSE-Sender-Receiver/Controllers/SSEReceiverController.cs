using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace SSE_Sender_Receiver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSEReceiverController : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> Get(string userId)
        {
            // Set the response headers for SSE
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("Access-Control-Allow-Origin", "*");


            await Response.StartAsync(); // Start the response

            try
            {
                // Use an asynchronous StreamWriter
                using (var eventStreamWriter = new StreamWriter(Response.Body, Encoding.UTF8, 1024, true))
                {
                    // Create an observer for the current user
                    var observer = new SSEObserver();
                    SSESenderController.AddObserver(userId, observer);

                    // Write SSE events to the response stream
                    while (!Response.HttpContext.RequestAborted.IsCancellationRequested)
                    {
                        var eventData = observer.GetNextEvent();
                        if (eventData != null)
                        {
                            await eventStreamWriter.WriteAsync(eventData);
                            await eventStreamWriter.FlushAsync();
                            await Response.Body.FlushAsync();
                        }

                        await Task.Delay(1000); // Delay between events
                    }

                    // Dispose the observer when done
                    observer.Dispose();
                }
            }
            finally
            {
                // Remove the observer when the SSE connection is closed
                Response.ContentType = null;
                Response.ContentLength = null;
                Response.Headers.Clear();
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                Response.Headers.Add("Connection", "close");
            }

            return new EmptyResult();
        }

        private class SSEObserver : IObserver<string>, IDisposable
        {
            private Queue<string> eventQueue = new Queue<string>(); // Queue to hold SSE event data

            public void OnNext(string value)
            {
                lock (eventQueue)
                {
                    eventQueue.Enqueue(value); // Add event data to the queue
                }
            }

            public void OnError(Exception error)
            {
                // Handle SSE errors here
            }

            public void OnCompleted()
            {
                // Handle SSE completion here
            }

            public void Dispose()
            {
                // Implement your logic for disposing resources
            }

            public string GetNextEvent()
            {
                lock (eventQueue)
                {
                    if (eventQueue.Count > 0)
                    {
                        return eventQueue.Dequeue(); // Get the next event data from the queue
                    }
                    else
                    {
                        return null; // No event data in the queue
                    }
                }
            }
        }
    }
}
