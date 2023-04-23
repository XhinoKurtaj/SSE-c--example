using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SSE_Sender_Receiver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSESenderController : ControllerBase
    {
        private static readonly Dictionary<string, IObserver<string>> Observers = new Dictionary<string, IObserver<string>>();


        [HttpPost]
        public ActionResult NotifyLoggedIn(string userId)
        {
            if (Observers.ContainsKey(userId))
            {
                var observer = Observers[userId];
                observer.OnNext("User B is logged in."); // Send SSE message to user A
            }
            return Ok();
        }

        public static void AddObserver(string userId, IObserver<string> observer)
        {
            Observers[userId] = observer;
        }
    }
}
