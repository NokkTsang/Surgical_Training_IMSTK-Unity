using System;

namespace ImstkUnity
{
    /// <summary>
    /// Latching function, on Upate() will call the `checkCallback`, whenever that callback returns 
    /// a different value than the previous call the active or inactive callback will be called
    /// i.e. when check returns "true" after returning "false" the active callback will be called
    /// when check returns false after returning true the inactive callback will be called
    /// </summary>
    public class Latch
    {
        private bool isActive;
        private Func<bool> callback;
        private Action activeCallback;
        private Action inactiveCallback;

        public Latch(Func<bool> checkCallback, Action activeCallback, Action inactiveCallback, bool startStateActive = false)
        {
            this.isActive = startStateActive;
            this.callback = checkCallback;
            this.activeCallback = activeCallback;
            this.inactiveCallback = inactiveCallback;
        }

        public bool IsActive => isActive;

        public void Update()
        {
            bool previousState = isActive;

            if (isActive)
            {
                if (!callback.Invoke())
                {
                    isActive = false;
                    inactiveCallback?.Invoke();
                }
            }
            else
            {
                if (callback.Invoke())
                {
                    isActive = true;
                    activeCallback?.Invoke();
                }
            }
        }
    }
}
