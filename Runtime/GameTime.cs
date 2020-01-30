namespace Nementic
{
    using UnityEngine;

    public static class GameTime
    {
        public static event System.Action<bool> PauseChanged;

        public static float deltaTime => timeProvider.deltaTime;

        public static float unscaledDeltaTime => timeProvider.unscaledDeltaTime;

        public static float time => timeProvider.time;

        public static float pausedTimeScale
        {
            get => m_PausedTimeScale;
            set => m_PausedTimeScale = Mathf.Max(value, 0f);
        }
        private static float m_PausedTimeScale = 0f;

        public static float defaultTimeScale
        {
            get => m_DefaultTimeScale;
            set => m_DefaultTimeScale = Mathf.Max(value, 0f);
        }
        private static float m_DefaultTimeScale = 1f;

        public static float timeScale
        {
            get => timeProvider.timeScale;
            set => timeProvider.timeScale = Mathf.Max(value, 0f);
        }

        /// <summary>
        /// The default time provider if not overriden by <see cref="SetTimeProvider(ITimeProvider)"/>.
        /// </summary>
        public static readonly ITimeProvider unityTimeProvider;

        private static ITimeProvider timeProvider;

        private static int m_PauseRequests = 0;

        static GameTime()
        {
            unityTimeProvider = new UnityTimeProvider();
            timeProvider = unityTimeProvider;
        }

        public static bool RequestPause()
        {
            m_PauseRequests++;

            if (m_PauseRequests == 1)
            {
                SetPause(true);
                return true;
            }
            return false;
        }

        public static bool RequestUnpause()
        {
            if (m_PauseRequests > 0)
            {
                m_PauseRequests--;

                if (m_PauseRequests == 0)
                {
                    SetPause(false);
                    return true;
                }
            }
            return false;
        }

        public static void ForcePause()
        {
            if (IsPaused)
                return;

            m_PauseRequests = 1;
            SetPause(true);
        }

        public static void ForceUnpause()
        {
            if (IsPaused == false)
                return;

            m_PauseRequests = 0;
            SetPause(false);
        }

        private static void SetPause(bool isPaused)
        {
            timeProvider.timeScale = isPaused ? pausedTimeScale : defaultTimeScale;
            PauseChanged?.Invoke(isPaused);
        }

        public static bool IsPaused => m_PauseRequests > 0;

        /// <summary>
        /// Override the default time provider for testing.
        /// To reset to the the provider pass <see cref="unityTimeProvider"/>.
        /// </summary>
        public static void SetTimeProvider(ITimeProvider timeProvider)
        {
            GameTime.timeProvider = timeProvider ?? throw new System.ArgumentNullException(nameof(timeProvider));
        }

        private class UnityTimeProvider : ITimeProvider
        {
            public float timeScale
            {
                get => Time.timeScale;
                set => Time.timeScale = value;
            }

            public float deltaTime => Time.deltaTime;
            public float unscaledDeltaTime => Time.unscaledDeltaTime;
            public float time => Time.unscaledDeltaTime;
            public int frameCount => Time.frameCount;
        }
    }

    public interface ITimeProvider
    {
        float timeScale { get; set; }
        float deltaTime { get; }
        float unscaledDeltaTime { get; }
        float time { get; }
        int frameCount { get; }
    }
}
